using System.Collections;
using GorillaTrials.Models;
using UnityEngine;

namespace GorillaTrials.Behaviours;

public class AchievementChecker : MonoBehaviour
{
    public static AchievementChecker instance;

    public void Awake()
    {
        instance = this;
    }
    public void UpdateAchievements(double? submitTime, Trial trial)
    {
        if (submitTime.HasValue)
        {
            if (trial.TrialServerName == "stumpclimb" && submitTime.Value < 11 &&
                Plugin.achievementManager.IsUnlocked("stump_climb_champ") == false)
            {
                Plugin.achievementManager.UnlockAchievement("stump_climb_champ");
                HUDManager.instance.SetHUDText("Unlocked Achievement: Stump Climb Champion!");
                StartCoroutine(ClearHUDDelayed(2.5f));
            }   
            if (trial.TrialServerName == "hp2sprintadvanced" &&
                Plugin.achievementManager.IsUnlocked("adv_hp2") == false)
            {
                Plugin.achievementManager.UnlockAchievement("adv_hp2");
                HUDManager.instance.SetHUDText("Unlocked Achievement: Hoverpark 2 Sprint Master!");
                StartCoroutine(ClearHUDDelayed(2.5f));
            }   
            if (Plugin.achievementManager.IsUnlocked("first_trial") == false && submitTime != null)
            {
                Plugin.achievementManager.UnlockAchievement("first_trial");
                HUDManager.instance.SetHUDText("Unlocked Achievement: First Trial!");
                StartCoroutine(ClearHUDDelayed(2.5f));
            }
        }

        if (TrialManager.GetTrialsWithPBCount(TrialManager.Instance.Trials) >= 5 && Plugin.achievementManager.IsUnlocked("5trials") == false)
        {
            Plugin.achievementManager.UnlockAchievement("5trials");
            HUDManager.instance.SetHUDText("Unlocked Achievement: 5 Trials!");
            StartCoroutine(ClearHUDDelayed(2.5f));
        }
        if (TrialManager.GetTrialsWithPBCount(TrialManager.Instance.Trials) >= 10 && Plugin.achievementManager.IsUnlocked("10trials") == false)
        {
            Plugin.achievementManager.UnlockAchievement("10trials");
            HUDManager.instance.SetHUDText("Unlocked Achievement: 10 Trials!");
            StartCoroutine(ClearHUDDelayed(2.5f));
        }
        if (TrialManager.GetTrialsWithPBCount(TrialManager.Instance.Trials) >= 20 && Plugin.achievementManager.IsUnlocked("20trials") == false)
        {
            Plugin.achievementManager.UnlockAchievement("20trials");
            HUDManager.instance.SetHUDText("Unlocked Achievement: 20 Trials!");
            StartCoroutine(ClearHUDDelayed(2.5f));
        }
        if (TrialManager.GetTrialsWithPBCount(TrialManager.Instance.Trials) >= 30 && Plugin.achievementManager.IsUnlocked("30trials") == false)
        {
            Plugin.achievementManager.UnlockAchievement("30trials");
            HUDManager.instance.SetHUDText("Unlocked Achievement: 30 Trials!");
            StartCoroutine(ClearHUDDelayed(2.5f));
        }
    }
    
    private IEnumerator ClearHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        HUDManager.instance.ClearHUD();
        AchievementUI.instance.TestLOL();
    }
}