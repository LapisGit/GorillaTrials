using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using GorillaTrials.Tools;
using UnityEngine;

namespace GorillaTrials.Behaviours
{
    [Serializable]
    public class Achievement
    {
        public string ID;
        public string Name;
        public string Description;
        public bool Unlocked;

        internal ConfigEntry<bool> ConfigEntry;

        public Achievement(string id, string name, string description)
        {
            ID = id;
            Name = name;
            Description = description;
            Unlocked = false;
        }
    }

    public class AchievementManager
    {
        private readonly Dictionary<string, Achievement> achievements = new();
        private readonly ConfigFile config;

        public AchievementManager(ConfigFile configFile)
        {
            config = configFile;
        }

        public void RegisterAchievement(Achievement achievement)
        {
            if (!achievements.ContainsKey(achievement.ID))
            {
                // Register config entry for unlocked state
                achievement.ConfigEntry = config.Bind(
                    "Achievements",
                    achievement.ID,
                    false,
                    $"Whether the achievement '{achievement.Name}' is unlocked."
                );

                // Load saved unlocked state
                achievement.Unlocked = achievement.ConfigEntry.Value;

                achievements.Add(achievement.ID, achievement);
# if DEBUG
                Logging.Info($"Registered achievement: {achievement.Name} (unlocked: {achievement.Unlocked})");
#endif
            }
        }

        public void UnlockAchievement(string id)
        {
            if (achievements.TryGetValue(id, out var achievement))
            {
                if (!achievement.Unlocked)
                {
                    achievement.Unlocked = true;
                    achievement.ConfigEntry.Value = true; // update config
                    config.Save();
                    Logging.Info($"Unlocked achievement: {achievement.Name} - {achievement.Description}");
                    AchievementUI.instance?.UpdateAchievements();
                }
            }
            else
            {
                Logging.Error($"Tried to unlock unknown achievement: {id}");
            }
        }

        public bool IsUnlocked(string id)
        {
            return achievements.TryGetValue(id, out var achievement) && achievement.Unlocked;
        }

        public List<Achievement> GetAllAchievements()
        {
            return new List<Achievement>(achievements.Values);
        }
    }
}
