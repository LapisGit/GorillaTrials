using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using GorillaLibrary.Extensions;
using GorillaLibrary.Models;
using GorillaTrials.Tools;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaTrials.Behaviours;

public class RigBadgeManager : MonoBehaviour
{
    public static RigBadgeManager Instance;

    private Vector3 badgeLocalPosition = new(0.095f, 0.275f, 0.1f);
    private Vector3 badgeLocalEulerAngles = new(350f, 0, 0);
    private Vector3 badgeLocalScale = Vector3.one;

    private GameObject bronzeBadge, silverBadge, goldBadge, wrBadge, contribBadge, moderatorBadge, betaBadge;
    
    private GameObject activeBadge;

    public readonly MelonEvent onCosmeticUpdate = new();

    public async void Awake()
    {
        Instance = this;
        
        await InitializeBadgeObjects();
        SpawnLocalBadge(contribBadge);
    }

    async Task InitializeBadgeObjects()
    {
        bronzeBadge = await AssetLoader.LoadAsset<GameObject>("BronzeBadge");
        silverBadge = await AssetLoader.LoadAsset<GameObject>("SilverBadge");
        goldBadge = await AssetLoader.LoadAsset<GameObject>("GoldBadge");
        wrBadge = await AssetLoader.LoadAsset<GameObject>("Trophy");
        contribBadge = await AssetLoader.LoadAsset<GameObject>("ContribBadge");
        moderatorBadge = await AssetLoader.LoadAsset<GameObject>("ModeratorBadge");
        betaBadge = await AssetLoader.LoadAsset<GameObject>("BetaBadge");
    }

    public GameObject SpawnLocalBadge(GameObject badgePrefab)
    {
        if (badgePrefab == null)
        {
            return null;
        }

        ClearBadge(VRRig.LocalRig);

        activeBadge = Instantiate(badgePrefab, VRRig.LocalRig.GetBone(GorillaRigBone.Body), false);
        ApplyConfiguredTransform(activeBadge.transform);
        activeBadge.gameObject.tag = "gtrialsbadge";
        return activeBadge;
    }
    
    public void SpawnOtherPlayerBadge(GameObject badgePrefab, VRRig rig)
    {
        ClearBadge(rig);

        activeBadge = Instantiate(badgePrefab, VRRig.LocalRig.GetBone(GorillaRigBone.Body), false);
        ApplyConfiguredTransform(activeBadge.transform);
    }

    public void SetBadgeTransform(Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
    {
        badgeLocalPosition = localPosition;
        badgeLocalEulerAngles = localEulerAngles;
        badgeLocalScale = localScale;

        if (activeBadge != null)
        {
            ApplyConfiguredTransform(activeBadge.transform);
        }
    }

    public void ClearBadge(VRRig rig)
    {
        Transform badge = rig.GetComponentsInChildren<Transform>().FirstOrDefault(t => t.CompareTag("gtrialsbadge"));
        if (badge != null)
        {
            Destroy(badge.gameObject);
        }
    }

    private void ApplyConfiguredTransform(Transform badgeTransform)
    {
        badgeTransform.localPosition = badgeLocalPosition;
        badgeTransform.localRotation = Quaternion.Euler(badgeLocalEulerAngles);
        badgeTransform.localScale = badgeLocalScale;
    }
    
    private IEnumerator GetInventory()
    {
        string url = $"{Constants.ServerURL}/inventory";
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to fetch inventory: {request.error}");
                yield break;
            }
            
            string jsonResponse = request.downloadHandler.text;
            
            BadgeArray badgeArray = JsonUtility.FromJson<BadgeArray>($"{{\"badges\":{jsonResponse}}}");
            if (badgeArray?.badges != null && badgeArray.badges.Length > 0)
            {
                onCosmeticUpdate?.Invoke();
            }
        }
    }

    [System.Serializable]
    private class BadgeArray
    {
        public string[] badges;
    }
}