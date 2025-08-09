using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GorillaTrials.Models;
using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace GorillaTrials.Behaviours
{
    internal class CustomMapManager : MonoBehaviour
    {
        public static CustomMapManager instance;
        public bool approvedMap = false;

        public void Awake()
        {
            instance = this;
        }

        public void LoadTrialsFromScene()
        {

            DestroyAllTrialsFromCustomMap();

            var scene = SceneManager.GetSceneByName(CustomMapLoader.initialSceneName);
            var rootObjects = scene.GetRootGameObjects();

            foreach (var root in rootObjects)
            {
                TraverseHierarchy(root.transform);
            }

            Logging.Info("scene trial loading complete! :3");
        }

        void TraverseHierarchy(Transform parent)
        {
            ProcessObject(parent.gameObject);

            foreach (Transform child in parent)
            {
                TraverseHierarchy(child);
            }
        }

        void ProcessObject(GameObject obj)
        {
            if (!obj.name.StartsWith("Trial_"))
                return;
            

            string[] parts = obj.name.Split('_');
            if (parts.Length < 5)
            {
                Logging.Warning($"Trial object '{obj.name}' does not match naming convention.");
                return;
            }

            string displayName = parts[1];
            string difficultyStr = parts[2];
            string typeStr = parts[3];
            string trialId = parts[4];

            if (!Enum.TryParse(typeStr, true, out ETrialType trialType))
            {
                Logging.Warning($"Invalid trial type '{typeStr}' on '{obj.name}'.");
                return;
            }

            if (!Enum.TryParse(difficultyStr, true, out ETrialDifficulty trialDifficulty))
            {
                Logging.Warning($"Invalid trial difficulty '{difficultyStr}' on '{obj.name}'. Defaulting to Easy.");
                trialDifficulty = ETrialDifficulty.Easy;
            }

            float angle = obj.transform.eulerAngles.y;
            Vector3 position = obj.transform.position;
            object[] parameters = null;
            float maxTime = 0;

            if (trialType == ETrialType.Box)
            {
                List<Vector3> boxPositions = new();
                foreach (Transform child in obj.transform)
                {
                    boxPositions.Add(child.position);
                }

                parameters = new object[] { boxPositions };
            }
            else if (trialType == ETrialType.Zone)
            {
                Transform start = obj.transform.Find("Start");
                Transform end = obj.transform.Find("End");

                if (start == null || end == null)
                {
                    Logging.Warning($"Zone trial '{displayName}' missing 'Start' or 'End' child.");
                    return;
                }

                List<Vector3> zonePoints = new() { start.position, end.position };
                parameters = new object[] { zonePoints };
            }
            
            TrialManager.Instance.CreateTrial(displayName, trialId, position, angle, trialType, trialDifficulty, maxTime, true, parameters);
        }
        
        public void DestroyAllTrialsFromCustomMap()
        {
            
            approvedMap = false;
            
            
            var customTrials = TrialManager.Instance.Trials
                .Where(t => t.isFromCustomMap)
                .ToList();

            foreach (var trial in customTrials)
            {
                if (trial.trialUIObject != null)
                {
                    Destroy(trial.trialUIObject);
                }

                TrialManager.Instance.Trials.Remove(trial);
            }
            
            if (TrialManager.Instance.Started)
            {
                TrialManager.Instance.currentTrial.stateMachine.SwitchState(new Trial_End(TrialManager.Instance.currentTrial, false));
            }
        }

        private const string approvedMapsUrl = "https://raw.githubusercontent.com/LapisGit/GorillaTrials/refs/heads/main/approvedmaps.json";

        
        public class ApprovedMapsWrapper
        {
            public List<long> approvedMaps { get; set; }
        }
        public async Task CheckIfApprovedMap(long mapID)
        {
            try
            {
                using (UnityWebRequest request = UnityWebRequest.Get(approvedMapsUrl))
                {
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string json = request.downloadHandler.text;
                        
                        ApprovedMapsWrapper wrapper = JsonConvert.DeserializeObject<ApprovedMapsWrapper>(json);

                        if (wrapper?.approvedMaps != null)
                        {
                            foreach (long id in wrapper.approvedMaps)
                            {
                                if (id == mapID)
                                {
                                    approvedMap = true;
                                    LoadTrialsFromScene();
                                    return;
                                }
                            }
                        }
                        
                        LoadTrialsFromScene();
                        Logging.Info("Map is NOT approved >:3");
                    }
                    else
                    {
                        Logging.Fatal($"Failed to fetch approved maps JSON");
                        Logging.Error(request.error);
                        // even if we cant fetch approved maps, load trials but act like they are not approved
                        approvedMap = false;
                        LoadTrialsFromScene();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Fatal("An error occurred while checking approved maps");
                Logging.Error(ex);
                // even if we cant fetch approved maps, load trials but act like they are not approved
                approvedMap = false;
                LoadTrialsFromScene();
            }
        }
        
    }
}
