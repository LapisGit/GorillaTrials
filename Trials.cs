using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GorillaTrials.Behaviors;
using GorillaTrials.Models;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using TMPro;


namespace GorillaTrials
{
    public class Trials : MonoBehaviour
    {
        private static readonly System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        public static List<Trial> All = new List<Trial>()
       
        
        {
           // new Trial("")
        };

        public static bool trialStarted;
        public static int boxesCollected;
        public static int boxesToCollect;

        public static float trialTime;

        public static Trial currentTrial;
        
        public static int index = 0;
        public static void StartTrial(Trial trialData)
        {
            if (trialStarted)
            {
                return;
            }

            currentTrial = trialData;
            currentTrial.trialObject.SetActive(false);

            stopwatch.Restart(); // Start or reset the stopwatch

            switch (currentTrial.TrialType)
            {
                case (int)TrialType.Box:
                    trialStarted = true;
                    PopulateTrialBoxes();
                    break;
                case (int)TrialType.Zone:
                    Debug.LogError("Not implemented yet.  (Guys we're not Another Axiom chill.)");
                    EndTrial(false);
                    break;
                default:
                    Console.WriteLine("default isn't supposed to be called what.");
                    EndTrial(false);
                    break;
            }
        }


        public static void PopulateTrialBoxes()
        {
            index = 0;
            boxesCollected = 0;
            boxesToCollect = currentTrial.boxPositions.Count;
            if (currentTrial != null)
            {
                if (currentTrial.boxPositions == null)
                {
                    Debug.LogError("Current trial box positions are null.");
                    return;
                }
                
                foreach (Vector3 boxPosition in currentTrial.boxPositions)
                {
                    GameObject box = Instantiate(LoadTrials.TrialBoxPrefab);
                    box.SetActive(true);
                    box.layer = 15; //Gorilla Boundary layer (body collider only triggers this with OnTriggerEnter iirc)

                    box.GetComponent<SphereCollider>().isTrigger = true;

                    TrialBoxCollider trialBoxCollider = box.AddComponent<TrialBoxCollider>();
                    trialBoxCollider.index = index;
                    trialBoxCollider.trialservername = currentTrial.TrialServerName;

                    box.transform.position = boxPosition;
                    box.name = $"Box_{index}";
                    Debug.Log("Instantiated box (" + index + ")");
                    index++;
                }
            }
            else
            {
                Debug.LogError("Current trial is null.");
            }
        }

        public static void EndTrial(bool shouldSubmit = true)
        {
            Debug.Log("Trial " + currentTrial.TrialLongName + " completed!");
            stopwatch.Stop();

            TimeSpan elapsed = stopwatch.Elapsed;
            double submitTime = Math.Round(elapsed.TotalSeconds, 3);
            Debug.Log($"Trial ended. Duration: {submitTime} seconds.");

            if (shouldSubmit)
            {
                string playerName = NetworkSystem.Instance.LocalPlayer.NickName;
                string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
            
                string jsonBody = JsonUtility.ToJson(new TrialResult
                {
                    PlayerName = playerName,
                    Time = submitTime,
                    PlayerId = playerId
                });
            
                SendTrialResult(jsonBody);
            
            
            
                string pbKey = $"PB_{currentTrial.TrialServerName}";

                if (PlayerPrefs.HasKey(pbKey))
                {
                    if (PlayerPrefs.GetFloat(pbKey) > submitTime)
                    {
                        PlayerPrefs.SetFloat(pbKey, (float)submitTime);
                        PlayerPrefs.Save();
                        var props = new CustomProps();
                        props.AddPB(pbKey, (float)submitTime);
                        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(pbKey, out object value) && value is double pb)
                        {
                            GameObject.Find(currentTrial.TrialServerName).transform.Find("UI/Info/PB").gameObject.GetComponent<TextMeshProUGUI>().text = "PB: "+value;
                        }
                    }
                }
                else
                {
                    PlayerPrefs.SetFloat(pbKey, (float)submitTime);    
                    PlayerPrefs.Save();
                    var props = new CustomProps();
                    props.AddPB(pbKey, (float)submitTime);
                    if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(pbKey, out object value) && value is double pb)
                    {
                        GameObject.Find(currentTrial.TrialServerName).transform.Find("UI/Info/PB").gameObject.GetComponent<TextMeshProUGUI>().text = "PB: "+value;
                    }
                }
            }
            
            for (int i = 0; i < index; i++)
            {
                string boxName = $"Box_{i}";
                GameObject box = GameObject.Find(boxName);
                if (box != null)
                {
                    Destroy(box);
                }
            }

            trialStarted = false;
            index = 0;
            currentTrial.trialObject.SetActive(true);
            currentTrial = null; //keep this line at the end of the method kplsthx :3
        }
        
        
        public static void SendTrialResult(string json)
        {
            FindObjectOfType<MonoBehaviour>().StartCoroutine(PostRequest("https://trials.freebranchcoins.xyz/leaderboard/"+currentTrial.TrialServerName, json));
        }
        
        [System.Serializable]
        public class TrialResult
        {
            public string PlayerName;
            public double Time;
            public string PlayerId;
        }


        public static IEnumerator PostRequest(string url, string json)
        {
            string apiKey = Plugin.apiKeyEntry.Value;
            
            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", apiKey);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully sent trial result.");
            }
            else
            {
                Debug.LogError($"Error sending trial result: {request.error}");
            }
        }

    }
}
