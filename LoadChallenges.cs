using BepInEx;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace GorillaTrials
{
    public class LoadChallenges : BaseUnityPlugin
    {
        public static GameObject trialUIObject;
        public static AssetBundle bundle = AssetBundle.LoadFromFile(tempPath);
        public static string tempPath = Path.Combine(Path.GetTempPath(), Constants.AssetBundleName);
        public static void LoadAssetBundle()
        {
            Debug.Log("Loading Trials...");

            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                                          .FirstOrDefault(name => name.EndsWith(Constants.AssetBundleName));

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                resourceStream.CopyTo(fileStream);
            }
            
            if (bundle == null)
            {
                Debug.LogError("Failed to load AssetBundle! Please report this to Lapis!");
                return;
            }
            
            CreateChallenge("Test Trial", "testtrial", new Vector3(-65.6918f,2.5123f,-72.0744f), false);
        }

        public static void CreateChallenge(string triallongname, string trialservername, Vector3 position, bool ZoneTrial)
        {
            trialUIObject = bundle.LoadAsset<GameObject>("Trial");
            trialUIObject.name = trialservername;
            trialUIObject.transform.position = position;
            trialUIObject.transform.Find(trialservername+"UI/Info/TrialName").gameObject.GetComponent<TextMeshProUGUI>().text = triallongname;
            if (ZoneTrial == false)
            { 
                trialUIObject.transform.Find(trialservername+"UI/Info/TrialType").gameObject.GetComponent<TextMeshProUGUI>().text = "Box Trial";
            }
            else
            { 
                trialUIObject.transform.Find(trialservername+"UI/Info/TrialType").gameObject.GetComponent<TextMeshProUGUI>().text = "Zone Trial";
            }
        }
        
        }
    }

