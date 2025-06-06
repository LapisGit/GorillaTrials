using BepInEx;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace GorillaTrials
{
    public class LoadChallenges : MonoBehaviour
    {
        public static GameObject trialUIObject;
        public static AssetBundle bundle;
        public static void LoadAssetBundle()
        {
            Debug.Log("Loading Trials...");
            Stream str = Assembly.GetExecutingAssembly().GetManifestResourceStream("GorillaTrials.Assets.bundle");
            bundle = AssetBundle.LoadFromStream(str);
            
            if (bundle == null)
            {
                Debug.LogError("Failed to load AssetBundle! Please report this to Lapis!");
                return;
            }
            else
            {
                Debug.Log("Bundle isn't null.");
            }
            
            CreateChallenge("Test Trial", "testtrial", new Vector3(-65.6918f,2.5123f,-72.0744f), false);
        }

        public static void CreateChallenge(string triallongname, string trialservername, Vector3 position, bool ZoneTrial)
        {
            trialUIObject = Instantiate(bundle.LoadAsset<GameObject>("Trial"));
            trialUIObject.name = trialservername;
            trialUIObject.transform.position = position;
            trialUIObject.transform.Find("UI/Info/TrialName").gameObject
                .GetComponent<TextMeshProUGUI>().text = triallongname;
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
        }
    }
}

