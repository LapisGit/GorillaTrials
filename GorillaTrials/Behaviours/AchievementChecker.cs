using GorillaTrials.Models;
using System.Collections;
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
            }
            if (trial.TrialServerName == "swingingaround" && submitTime.Value < 10 &&
                Plugin.achievementManager.IsUnlocked("vinemaster") == false)
            {
                Plugin.achievementManager.UnlockAchievement("vinemaster");
                HUDManager.instance.SetHUDText("Unlocked Achievement: Vine Master!");
            }
            if (submitTime.Value >= 120 &&
                Plugin.achievementManager.IsUnlocked("slowpoke") == false)
            {
                Plugin.achievementManager.UnlockAchievement("slowpoke");
                HUDManager.instance.SetHUDText("Unlocked Achievement: Slowpoke!");
            }
            if (submitTime.Value >= 300 &&
                Plugin.achievementManager.IsUnlocked("ultraslowpoke") == false)
            {
                Plugin.achievementManager.UnlockAchievement("ultraslowpoke");
                HUDManager.instance.SetHUDText("Unlocked Achievement: Ultra Slowpoke!");
            }
            if (trial.TrialServerName == "hp2sprintadvanced" &&
                Plugin.achievementManager.IsUnlocked("adv_hp2") == false)
            {
                Plugin.achievementManager.UnlockAchievement("adv_hp2");
                HUDManager.instance.SetHUDText("Unlocked Achievement: Hoverpark 2 Sprint Master!");
            }
            if (trial.TrialServerName == "masterswimmer" &&
                Plugin.achievementManager.IsUnlocked("masterswimmer") == false)
            {
                Plugin.achievementManager.UnlockAchievement("masterswimmer");
                HUDManager.instance.SetHUDText("Unlocked Achievement: Master Swimmer!");
            }
            if (Plugin.achievementManager.IsUnlocked("first_trial") == false && submitTime != null)
            {
                Plugin.achievementManager.UnlockAchievement("first_trial");
                HUDManager.instance.SetHUDText("Unlocked Achievement: First Trial!");
            }
        }

        if (TrialManager.GetTrialsWithPBCount(TrialManager.Instance.Trials) >= 5 && Plugin.achievementManager.IsUnlocked("5trials") == false)
        {
            Plugin.achievementManager.UnlockAchievement("5trials");
            HUDManager.instance.SetHUDText("Unlocked Achievement: 5 Trials!");
        }
        if (TrialManager.GetTrialsWithPBCount(TrialManager.Instance.Trials) >= 10 && Plugin.achievementManager.IsUnlocked("10trials") == false)
        {
            Plugin.achievementManager.UnlockAchievement("10trials");
            HUDManager.instance.SetHUDText("Unlocked Achievement: 10 Trials!");
        }
        if (TrialManager.GetTrialsWithPBCount(TrialManager.Instance.Trials) >= 20 && Plugin.achievementManager.IsUnlocked("20trials") == false)
        {
            Plugin.achievementManager.UnlockAchievement("20trials");
            HUDManager.instance.SetHUDText("Unlocked Achievement: 20 Trials!");
        }
        if (TrialManager.GetTrialsWithPBCount(TrialManager.Instance.Trials) >= 30 && Plugin.achievementManager.IsUnlocked("30trials") == false)
        {
            Plugin.achievementManager.UnlockAchievement("30trials");
            HUDManager.instance.SetHUDText("Unlocked Achievement: 30 Trials!");
        }
        
        int goldBadgeCount = TrialManager.GetTotalBadgeCount(BadgeType.Gold);
        int trialsWithBadges = TrialManager.GetTrialsWithBadgesConfigured(TrialManager.Instance.Trials);
        int trialAttempts =  PlayerPrefs.GetInt("Stats_TrialsAttempted", 0);
        int trialCompletions =  PlayerPrefs.GetInt("Stats_TrialsCompleted", 0);
        
        if (trialsWithBadges > 0 && goldBadgeCount >= trialsWithBadges && Plugin.achievementManager.IsUnlocked("goldhoarder") == false)
        {
            Plugin.achievementManager.UnlockAchievement("goldhoarder");
            HUDManager.instance.SetHUDText("Unlocked Achievement: Gold Hoarder!");
        }
        else
        {
            if (Plugin.achievementManager.IsUnlocked("goldhoarder") && goldBadgeCount < trialsWithBadges)
            {
                Plugin.achievementManager.LockAchievement("goldhoarder");
            }
        }
        
        if (trialAttempts >= 50 && Plugin.achievementManager.IsUnlocked("trialanderror") == false)
        {
            Plugin.achievementManager.UnlockAchievement("trialanderror");
            HUDManager.instance.SetHUDText("Unlocked Achievement: Trial and Error!");
        }
        
        if (trialAttempts >= 100 && Plugin.achievementManager.IsUnlocked("dedication") == false)
        {
            Plugin.achievementManager.UnlockAchievement("dedication");
            HUDManager.instance.SetHUDText("Unlocked Achievement: Dedication!");
        }
        
        if (trialAttempts >= 200 && Plugin.achievementManager.IsUnlocked("perseverance") == false)
        {
            Plugin.achievementManager.UnlockAchievement("perseverance");
            HUDManager.instance.SetHUDText("Unlocked Achievement: Perseverance!");
        }
        
        if (trialAttempts >= 500 && Plugin.achievementManager.IsUnlocked("timeandtimeagain") == false)
        {
            Plugin.achievementManager.UnlockAchievement("timeandtimeagain");
            HUDManager.instance.SetHUDText("Unlocked Achievement: Time and Time Again!");
        }
        
        if (trialAttempts >= 1000 && Plugin.achievementManager.IsUnlocked("giveup") == false)
        {
            Plugin.achievementManager.UnlockAchievement("giveup");
            HUDManager.instance.SetHUDText("Unlocked Achievement: Give Up!");
        }
        
        if (trialAttempts >= 2500 && Plugin.achievementManager.IsUnlocked("gooutside") == false)
        {
            Plugin.achievementManager.UnlockAchievement("gooutside");
            HUDManager.instance.SetHUDText("Unlocked Achievement: Go Outside!");
        }
        
        if (trialCompletions >= 20 && Plugin.achievementManager.IsUnlocked("pbpro") == false)
        {
            Plugin.achievementManager.UnlockAchievement("pbpro");
            HUDManager.instance.SetHUDText("Unlocked Achievement: PB Pro!");
        }
        
        if (trialCompletions >= 20 && Plugin.achievementManager.IsUnlocked("whatarethose") == false)
        {
            Plugin.achievementManager.UnlockAchievement("whatarethose");
            HUDManager.instance.SetHUDText("Unlocked Achievement: WHAT ARE THOSE!!??");
        }
        
        if (trialCompletions >= 20 && Plugin.achievementManager.IsUnlocked("trialmaster") == false)
        {
            Plugin.achievementManager.UnlockAchievement("trialmaster");
            HUDManager.instance.SetHUDText("Unlocked Achievement: Trial Master!");
        }
        
        ControlPanel.instance.UpdateAchievements();
    }
}