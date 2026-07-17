using GorillaTrials.Tools;
using GorillaLibrary.Models;
using GorillaLibrary.Extensions;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaTrials.Behaviours.Networking;

[RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
internal class NetworkedBadgePlayer : MonoBehaviour
{
    public RigContainer Container;
    public VRRig PlayerRig;

    private int? _currentBadgeIndex;
    private GameObject _currentBadgeInstance;
    private bool _initialized;
    private RigBadgeManager _badgeManager;

    public void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        Container = GetComponent<RigContainer>();
        _badgeManager = RigBadgeManager.Instance;

        if (Container == null)
        {
            enabled = false;
            return;
        }

        PlayerRig = Container.GetComponent<VRRig>();
        if (PlayerRig == null)
        {
            enabled = false;
        }
    }

    public void OnDestroy()
    {
        if (!_initialized) return;

        if (_currentBadgeInstance != null)
        {
            Destroy(_currentBadgeInstance);
        }
    }

    public void OnPlayerPropertyChanged(ExitGames.Client.Photon.Hashtable properties)
    {
        if (!_initialized) Initialize();

        try
        {
            if (properties.TryGetValue("BadgeIndex", out object badgeIndexObj))
            {
                int badgeIndex = (int)badgeIndexObj;
                StartCoroutine(VerifyAndApplyBadge(badgeIndex));
            }
        }
        catch (Exception ex)
        {
            Logging.Error($"Failed to process networked badge: {ex}");
        }
    }

    private IEnumerator VerifyAndApplyBadge(int badgeIndex)
    {
        if (badgeIndex == -1)
        {
            ApplyBadge(-1);
            yield break;
        }

        string playerId = PlayerRig.Creator.UserId;
        string url = $"{Constants.ServerURL}/doesPlayerOwnItem/{playerId}/{badgeIndex}";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to verify badge ownership: {request.error}");
                yield break;
            }

            string response = request.downloadHandler.text.Trim().ToLower();
            if (response == "true")
            {
                ApplyBadge(badgeIndex);
                Logging.Message($"Badge {badgeIndex} verified and applied");
            }
            else
            {
                Logging.Message($"Player does not own badge {badgeIndex}");
            }
        }
    }

    private void ApplyBadge(int badgeIndex)
    {
        if (_badgeManager == null || PlayerRig == null) return;

        if (_currentBadgeIndex == badgeIndex && _currentBadgeInstance != null)
            return;

        if (_currentBadgeInstance != null)
        {
            Destroy(_currentBadgeInstance);
            _currentBadgeInstance = null;
        }

        _currentBadgeIndex = badgeIndex;

        if (badgeIndex == -1)
        {
            Logging.Message("Badge removed");
            return;
        }

        GameObject badgePrefab = GetBadgePrefabForIndex(badgeIndex);
        if (badgePrefab == null)
        {
            Logging.Message($"Badge index {badgeIndex} not found");
            return;
        }

        Transform badgeParent = PlayerRig.GetBone(GorillaRigBone.Body);
        _currentBadgeInstance = Instantiate(badgePrefab, badgeParent, false);
        _currentBadgeInstance.name = badgePrefab.name;
        ApplyBadgeTransform(_currentBadgeInstance.transform);
    }

    private void ApplyBadgeTransform(Transform badgeTransform)
    {
        badgeTransform.localPosition = new Vector3(0.0977f, 0.2845f, 0.1104f);
        badgeTransform.localRotation = Quaternion.Euler(0, 0, 0);
        badgeTransform.localScale = Vector3.one;
    }

    private GameObject GetBadgePrefabForIndex(int index)
    {
        return index switch
        {
            0 => _badgeManager.bronzeBadge,
            1 => _badgeManager.silverBadge,
            2 => _badgeManager.goldBadge,
            3 => _badgeManager.wrBadge,
            4 => _badgeManager.contribBadge,
            5 => _badgeManager.moderatorBadge,
            6 => _badgeManager.betaBadge,
            _ => null
        };
    }
}



