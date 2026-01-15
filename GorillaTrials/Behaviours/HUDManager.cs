using GorillaLocomotion;
using GorillaTrials.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaTrials.Behaviours
{
    public class HUDManager : MonoBehaviour
    {
        public GameObject hud;
        public Transform text1;
        public static HUDManager instance;
        
        private Queue<string> messageQueue = new Queue<string>();
        private bool isDisplayingMessage = false;
        private Coroutine clearCoroutine = null;
        private string overlayMessage = "";
        private float overlayUntilTime = 0f;

        void Awake()
        {
            instance = this;
        }

        public async void Init()
        {
            hud = await AssetLoader.LoadAsset<GameObject>("HUDUI");
            TrialManager.Instance.achievementsUI = hud;

            hud = Instantiate(hud);
            DontDestroyOnLoad(hud);

            GameObject cameraObj = GTPlayer._instance.mainCamera.gameObject;
            if (cameraObj == null)
            {
                Logging.Error("camera not found! not initializing HUD!");
                return;
            }

            Camera cam = cameraObj.GetComponent<Camera>();
            var canvas = hud.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam;

            hud.transform.SetParent(cameraObj.transform);

            hud.GetComponent<RectTransform>().localPosition = new Vector3(0f, -0.5f, 1f);
            hud.GetComponent<RectTransform>().localRotation = Quaternion.Euler(20f, 0f, 0f);
            hud.GetComponent<RectTransform>().localScale = Vector3.one * 0.0025f;

            Transform text = hud.transform.Find("Text");
            if (text == null)
            {
                Logging.Error("idk why this would happen but the text component is null! not initializing HUD!");
                return;
            }

            Text textComp = text.GetComponent<Text>();
            if (textComp != null)
            {
                textComp.alignment = TextAnchor.MiddleCenter;
                textComp.material = new Material(Shader.Find("GUI/Text Shader"));
            }

            text1 = text;
            ClearHUD();
        }

        public void SetHUDText(string message)
        {
            messageQueue.Enqueue(message);
            
            if (!isDisplayingMessage)
            {
                StartCoroutine(DisplayNextMessage());
            }
        }

        public void ShowNotificationAlert(string message)
        {
            if (TrialManager.Instance != null && TrialManager.Instance.Started)
            {
                overlayMessage = message;
                overlayUntilTime = Time.time + 5f;
            }
            else
            {
                SetHUDText(message);
            }
        }

        private IEnumerator DisplayNextMessage()
        {
            while (messageQueue.Count > 0)
            {
                isDisplayingMessage = true;
                string message = messageQueue.Dequeue();
                
                if (text1 != null)
                {
                    text1.GetComponent<Text>().text = message;
                }
                
                if (clearCoroutine != null)
                {
                    StopCoroutine(clearCoroutine);
                }
                
                yield return new WaitForSeconds(5f);
                
                if (text1 != null)
                {
                    text1.GetComponent<Text>().text = "";
                }
            }
            
            isDisplayingMessage = false;
        }

        public void ClearHUD()
        {
            messageQueue.Clear();
            
            if (clearCoroutine != null)
            {
                StopCoroutine(clearCoroutine);
                clearCoroutine = null;
            }
            
            if (text1 != null)
            {
                text1.GetComponent<Text>().text = "";
            }
            
            isDisplayingMessage = false;
        }

        public void TrialTime()
        {
            if (TrialManager.Instance.Started)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(
                    TrialManager.Instance.currentTrial.stopwatch.Elapsed.TotalSeconds
                );

                string formattedTime = timeSpan.TotalHours >= 1
                    ? timeSpan.ToString(@"h\:mm\:ss\.fff")
                    : timeSpan.ToString(@"mm\:ss\.fff");

                if (overlayUntilTime > 0f && Time.time > overlayUntilTime)
                {
                    overlayUntilTime = 0f;
                    overlayMessage = "";
                }

                string displayText = formattedTime;
                if (!string.IsNullOrEmpty(overlayMessage) && overlayUntilTime > Time.time)
                {
                    displayText += "\n" + overlayMessage;
                }

                if (text1 != null)
                {
                    text1.GetComponent<Text>().text = displayText;
                }
            }
        }

        private void FixedUpdate()
        {
            if (TrialManager.Instance.Started)
            {
                TrialTime();
            }
        }
    }
}
