using GorillaTrials.Tools;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaTrials.Behaviours.Networking
{
    [RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
    internal class NetworkedPlayer : MonoBehaviour
    {
        public VRRig Rig;
        public NetPlayer Owner;

        public bool IsTrialMonke;

        public void Start()
        {
            NetworkHandler.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

            if (!IsTrialMonke && Owner is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef)
                NetworkHandler.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);
        }

        public void OnDestroy()
        {
            NetworkHandler.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

            if (IsTrialMonke)
            {
                IsTrialMonke = false;
            }
        }

        public void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
        {
            if (player == Owner)
            {
                Logging.Info($"{player.NickName} got properties: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");

                if (properties.TryGetValue("Trials", out object obj))
                {
                    Logging.Info(":3");
                }
            }
        }
    }
}
