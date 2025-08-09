using System;
using GorillaLocomotion;
using GorillaTrials.Models;
using GorillaTrials.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaTrials.Behaviours
{
    public class HUDManager : MonoBehaviour
    {
        public GameObject hud;
        public Transform text1;
        public static HUDManager instance;

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

            //SetHUDText("HUD Initialized :3");
            // debug
        }

        public void SetHUDText(string message)
        {
            if (text1 != null)
                text1.GetComponent<Text>().text = message;
        }

        public void ClearHUD()
        {
            if (text1 != null)
                text1.GetComponent<Text>().text = "";
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

                SetHUDText(formattedTime);
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
