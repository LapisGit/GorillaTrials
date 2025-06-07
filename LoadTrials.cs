using BepInEx;
using GorillaTrials.Behaviors.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using GorillaTrials.Models;

namespace GorillaTrials
{
    public class LoadTrials : MonoBehaviour
    {
        public static AssetBundle bundle;
        public static GameObject gorillaTrialsAssets;

        public static GameObject TrialUIPrefab;
        public static GameObject TrialBoxPrefab;

        public static void LoadAssetBundle()
        {
            Debug.Log("Loading Trials...");
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaTrials.Assets.bundle");
            bundle = AssetBundle.LoadFromStream(str);
            gorillaTrialsAssets = Instantiate(bundle.LoadAsset<GameObject>("GorillaTrials"));
            gorillaTrialsAssets.SetActive(false);

            TrialUIPrefab = gorillaTrialsAssets.transform.Find("Trial").gameObject;
            TrialBoxPrefab = gorillaTrialsAssets.transform.Find("Trial Box").gameObject;

            if (bundle == null)
            {
                Debug.LogError("Failed to load AssetBundle! Please report this to Lapis!");
                return;
            }
            else
            {
                Debug.Log("Bundle isn't null.");
            }
#if DEBUG
            CreateChallenge("Test Trial", "testtrial", new Vector3(-65.6918f, 2.5123f, -72.0744f), false);
#endif
        }

        public static void CreateChallenge(string triallongname, string trialservername, Vector3 position, bool ZoneTrial)
        {
            GameObject trialUIObject = Instantiate(TrialUIPrefab);
            trialUIObject.name = trialservername;
            trialUIObject.transform.position = position;
            trialUIObject.transform.Find("UI/Info/TrialName").gameObject.GetComponent<TextMeshProUGUI>().text = triallongname;
            trialUIObject.transform.Find("UI/Buttons/PlayTrial").gameObject.layer = 18; //Gorilla Interactable
            UIButton trialButton = trialUIObject.transform.Find("UI/Buttons/PlayTrial").AddComponent<UIButton>();
            if (ZoneTrial == false)
            {
                trialUIObject.transform.Find("UI/Info/TrialType").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Box Trial";
            }
            else
            {
                trialUIObject.transform.Find("UI/Info/TrialType").gameObject
                    .GetComponent<TextMeshProUGUI>().text = "Zone Trial";
            }

            //Trial trial = new Trial()
            //{
            //    trialObject = trialUIObject,
            //    TrialServerName = trialservername,
            //    TrialLongName = triallongname,
            //    TrialType = (int)TrialType.Box,
            //    zoneData = null,
            //};

            //Trials.All.Add(trial);

            trialButton.onPressed = () =>
            {
                //Trials.StartTrial(trial);
                Debug.Log("TrialButton pressed!");
            };
        }
    }
}

