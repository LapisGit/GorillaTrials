using ExitGames.Client.Photon;
using GorillaTrials.Tools;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using GorillaLibrary.Models;
using GorillaNetworking;

namespace GorillaTrials.Behaviours.Networking;

internal class NetworkBadgeSolution_RaiseEvent : NetworkBadgeSolution
{
    public override bool TransferOnlyInRooms => true;

    private readonly byte eventCode = 177;

    private readonly int id = "GorillaTrials".GetHashCode();

    public void Awake()
    {
        Instance = this;
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new() { { "Version", Constants.Version } });
    }

    public override bool IsCompatiblePlayer(Player player) => player != null;

    public override void SendProperties(Hashtable properties, Player[] targetPlayers)
    {
        object[] content = [id, properties];

        RaiseEventOptions raiseEventOptions = new()
        {
            TargetActors = [.. from player in targetPlayers select player.ActorNumber]
        };

        PhotonNetwork.RaiseEvent(eventCode, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void OnEvent(EventData data)
    {
        if (data.Code != eventCode) return;

        object[] eventData = (object[])data.CustomData;

        if (eventData.Length < 2 || eventData[0] is not int)
        {
            Logging.Error("Invalid badge sync parameters");
            return;
        }

        int eventId = (int)eventData[0];
        if (eventId != id) return;

        Player player = PhotonNetwork.CurrentRoom.GetPlayer(data.Sender);
        if (player.IsLocal) return;

        if (eventData[1] is not Hashtable properties) return;

        NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(player.ActorNumber);
        if (!VRRigCache.Instance.TryGetVrrig(netPlayer, out RigContainer playerRig)) return;

        NetworkedBadgePlayer networkedBadgePlayer = playerRig.GetComponent<NetworkedBadgePlayer>();
        if (networkedBadgePlayer == null)
        {
            networkedBadgePlayer = playerRig.gameObject.AddComponent<NetworkedBadgePlayer>();
        }

        networkedBadgePlayer.OnPlayerPropertyChanged(properties);
        NotifyPropertiesRecieved(player, properties);
    }
}



