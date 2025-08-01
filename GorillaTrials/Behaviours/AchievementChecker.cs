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
        if (Plugin.achievementManager.IsUnlocked("first_trial") == false && submitTime != null)
        {
            Plugin.achievementManager.UnlockAchievement("first_trial");
            HUDManager.instance.SetHUDText("Unlocked Achievement: First Trial!");
            StartCoroutine(ClearHUDDelayed(2.5f));
        }

        if (submitTime.HasValue)
        {
            if (trial.TrialServerName == "stumpclimb" && submitTime.Value < 11 &&
                Plugin.achievementManager.IsUnlocked("stump_climb_champ") == false)
            {
                Plugin.achievementManager.UnlockAchievement("stump_climb_champ");
                HUDManager.instance.SetHUDText("Unlocked Achievement: Stump Climb Champion!");
                StartCoroutine(ClearHUDDelayed(2.5f));
            }   
        }
    }
    
    private IEnumerator ClearHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        HUDManager.instance.ClearHUD();
    }
}