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
            Trial trial = new Trial(position, triallongname, trialservername, (int)TrialType.Box, null, TrialPositions.trialTestBoxes);
        }
    }
}

