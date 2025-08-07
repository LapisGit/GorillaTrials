using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using GorillaNetworking;
using GorillaTrials.Models;
using GorillaTrials.Tools;
using UnityEngine;
using Newtonsoft.Json;

namespace GorillaTrials.Behaviours
{
    public class ReplayManager : MonoBehaviour
    {
        public static ReplayManager Instance;

        private List<GameObject> trackedObjects = new();
        private List<FrameData> recordedFrames = new();
        public bool isRecording = false;
        private float startTime;

        public bool isReplaying = false;
        private float replayTime;
        private List<FrameData> replayFrames;
        private int currentFrameIndex = 0;
        
        private float lastRecordTime = 0f;
        private const float RECORD_INTERVAL = 1f / 30f; // ~0.0333s
        private float lastPlaybackTime = 0f;
        private const float PLAYBACK_INTERVAL = 1f / 30f;



        public GameObject replayObjects, replayleftHand, replayrightHand, replayHead;

        private async void Awake()
        {
            replayObjects = await AssetLoader.LoadAsset<GameObject>("Replay");
            TrialManager.Instance.achievementsUI = replayObjects;
            replayObjects = Instantiate(replayObjects);
            DontDestroyOnLoad(replayObjects);
            replayleftHand = replayObjects.gameObject.transform.Find("leftHand").gameObject;
            replayrightHand = replayObjects.gameObject.transform.Find("rightHand").gameObject;
            replayHead = replayObjects.gameObject.transform.Find("Head").gameObject;
            replayHead.SetActive(false);
            replayleftHand.SetActive(false);
            replayrightHand.SetActive(false);
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Update()
        {
            if (isRecording)
            {
                RecordFrame();
            }

            if (isReplaying)
                PlayFrame();
        }

        public void SetTrackedObjects(GameObject obj1, GameObject obj2, GameObject obj3)
        {
            trackedObjects = new List<GameObject> { obj1, obj2, obj3 };
        }

        public void StartRecording()
        {
            recordedFrames.Clear();
            startTime = Time.time;
            lastRecordTime = 0f;
            isRecording = true;
            isReplaying = false;
        }


        public void StopRecording()
        {
            isRecording = false;
        }

        public void SaveRecording(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Logging.Warning("Invalid file name.");
                return;
            }

            string path = Path.Combine(Application.persistentDataPath, fileName.EndsWith(".json") ? fileName : fileName + ".json");

            try
            {
                string json = JsonConvert.SerializeObject(recordedFrames, Formatting.None);
                File.WriteAllText(path, json);
                Logging.Info($"Saved replay to {path}");
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to save replay: {ex.Message}");
            }
        }

        public void LoadRecording(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Logging.Warning("Invalid file name.");
                return;
            }

            string path = Path.Combine(Application.persistentDataPath, fileName.EndsWith(".json") ? fileName : fileName + ".json");

            if (!File.Exists(path))
            {
                Logging.Warning($"Replay file not found at: {path}");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                replayFrames = JsonConvert.DeserializeObject<List<FrameData>>(json);
                Logging.Info($"Successfully loaded replay from {path}");
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to load replay: {ex.Message}");
            }
        }

        public void StartReplay(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Logging.Warning("Invalid replay filename.");
                return;
            }

            string path = Path.Combine(Application.persistentDataPath, fileName.EndsWith(".json") ? fileName : fileName + ".json");

            if (!File.Exists(path))
            {
                Logging.Warning($"Replay file not found at: {path}");
                return;
            }

            try
            {
                string json = File.ReadAllText(path);
                replayFrames = JsonConvert.DeserializeObject<List<FrameData>>(json);

                trackedObjects = new List<GameObject> { replayHead, replayleftHand, replayrightHand };

                foreach (var obj in trackedObjects)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }

                replayTime = 0f;
                currentFrameIndex = 0;
                isReplaying = true;
                isRecording = false;
                lastPlaybackTime = Time.time;

                Logging.Info($"Started replay from {fileName}");
            }
            catch (Exception ex)
            {
                Logging.Error($"Failed to load replay: {ex.Message}");
            }
        }

        private void RecordFrame()
        {
            if (trackedObjects.Count != 3)
                return;

            float currentTime = Time.time;
            if (currentTime - lastRecordTime < RECORD_INTERVAL)
                return;

            lastRecordTime = currentTime;

            FrameData frame = new FrameData
            {
                time = currentTime - startTime,
                positions = new List<Vector3>(),
                rotations = new List<Quaternion>()
            };

            foreach (var obj in trackedObjects)
            {
                frame.positions.Add(obj.transform.position);
                frame.rotations.Add(obj.transform.rotation);
            }

            recordedFrames.Add(frame);
        }


        private void PlayFrame()
        {
            if (trackedObjects.Count != 3 || replayFrames == null || currentFrameIndex >= replayFrames.Count)
            {
                StopReplay();
                return;
            }

            if (Time.time - lastPlaybackTime < PLAYBACK_INTERVAL)
                return;

            lastPlaybackTime = Time.time;
            replayTime += PLAYBACK_INTERVAL;

            while (currentFrameIndex < replayFrames.Count - 1 &&
                   replayFrames[currentFrameIndex + 1].time <= replayTime)
            {
                currentFrameIndex++;
            }

            FrameData frame = replayFrames[currentFrameIndex];

            for (int i = 0; i < trackedObjects.Count; i++)
            {
                trackedObjects[i].transform.position = frame.positions[i];
                trackedObjects[i].transform.rotation = frame.rotations[i];
            }

            if (currentFrameIndex >= replayFrames.Count - 1 &&
                replayTime > frame.time + 0.05f)
            {
                StopReplay();
            }
        }


        public void StopReplay()
        {
            isReplaying = false;

            foreach (var obj in trackedObjects)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            Logging.Info("Replay complete. Ghost objects deactivated.");
        }

        public async Task UploadReplayWR(string track, string playerId, double time)
        {
            try
            {
                string json = JsonConvert.SerializeObject(recordedFrames, Formatting.None);

                var body = new
                {
                    replayData = recordedFrames,
                    Time = time,
                    PlayerName = NetworkSystem.Instance.GetMyNickName().ToUpper(),
                    PlayerId = playerId
                };

                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("Authorization", Plugin.APIKey.Value);

                StringContent content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync($"{Constants.ServerURL}/wr/{track}", content);

                if (response.IsSuccessStatusCode)
                {
                    Logging.Info("WR replay uploaded successfully.");
                }
                else
                {
                    Logging.Warning($"WR replay upload failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception e)
            {
                Logging.Error("Failed to upload WR replay: " + e);
            }
        }


        public async Task<List<FrameData>> DownloadReplayWR(string track, string playerId)
        {
            try
            {
                using HttpClient client = new HttpClient();
                string url = $"{Constants.ServerURL}/wr_replays/{track}_{playerId}.json";
                Logging.Info($"getting data from {url}");

                string json = await client.GetStringAsync(url);
                replayFrames = JsonConvert.DeserializeObject<List<FrameData>>(json);

                if (replayFrames == null || replayFrames.Count == 0)
                {
                    Logging.Warning("Downloaded replay is empty or invalid.");
                    return null;
                }

                trackedObjects = new List<GameObject> { replayHead, replayleftHand, replayrightHand };

                foreach (var obj in trackedObjects)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }

                replayTime = 0f;
                currentFrameIndex = 0;
                isReplaying = true;
                isRecording = false;
                lastPlaybackTime = Time.time;


                Logging.Info($"started replay for {track}_{playerId}");

                return replayFrames;
            }
            catch (Exception e)
            {
                Logging.Error("Failed to download WR replay: " + e);
                return null;
            }
        }
    }

    [Serializable]
    public class FrameData
    {
        public float time;
        public List<Vector3> positions;
        public List<Quaternion> rotations;
    }
}