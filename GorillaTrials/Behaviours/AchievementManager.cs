using System;
using System.Collections.Generic;
using System.IO;
using BepInEx;
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
        private Dictionary<string, Achievement> achievements = new Dictionary<string, Achievement>();

        private readonly string saveFilePath = Path.Combine(Paths.ConfigPath, "gtrialsachievements.json");

        public AchievementManager()
        {
            LoadAchievements();
        }

        public void RegisterAchievement(Achievement achievement)
        {
            if (!achievements.ContainsKey(achievement.ID))
            {
                achievements.Add(achievement.ID, achievement);
                Logging.Info($"registered: {achievement.Name}");
            }
        }

        public void UnlockAchievement(string id)
        {
            if (achievements.TryGetValue(id, out var achievement))
            {
                if (!achievement.Unlocked)
                {
                    achievement.Unlocked = true;
                    Logging.Info($"unlocked: {achievement.Name} - {achievement.Description}");
                    SaveAchievements();
                    AchievementUI.instance.UpdateAchievements();
                }
            }
            else
            {
                Logging.Error($"tried to unlock unknown achievement: {id}");
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

        public void LoadAchievements()
        {
            try
            {
                if (!File.Exists(saveFilePath))
                {
                    Logging.Warning("Achievements save file does not exist, creating new one.");
                    SaveAchievements();
                    return;
                }
                
                if (File.Exists(saveFilePath))
                {
                    string json = File.ReadAllText(saveFilePath);
                    var savedList = JsonUtility.FromJson<AchievementListWrapper>(json);

                    foreach (var savedAchievement in savedList.achievements)
                    {
                        if (achievements.ContainsKey(savedAchievement.ID))
                        {
                            achievements[savedAchievement.ID].Unlocked = savedAchievement.Unlocked;
                        }
                        else
                        {
                            achievements.Add(savedAchievement.ID, savedAchievement);
                        }
                    }

                    Logging.Info("loaded saved achievements yay wahoo :3 :3 :3 :3 :3 :3 :3 :3 :3 :3");
                }
            }
            catch (Exception e)
            {
                Logging.Error($"failed to load achievements: {e} 3:");
            }
        }

        public void SaveAchievements()
        { 
            try
            {
                var list = new AchievementListWrapper
                {
                    achievements = new List<Achievement>(achievements.Values)
                };
                string json = JsonUtility.ToJson(list, true);
                File.WriteAllText(saveFilePath, json);
                Logging.Info("saved achievements");
            }
            catch (Exception e)
            {
                Logging.Error($"failed to save achievements: {e}");
            }
        }

        [Serializable]
        public class AchievementListWrapper
        {
            public List<Achievement> achievements;
        }
    }
}
