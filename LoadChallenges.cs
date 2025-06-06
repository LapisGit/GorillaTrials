using BepInEx;
using System.IO;
using System.Linq;
using System.Reflection;
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
                Debug.Log("Failed to load AssetBundle!");
                return;
            }
            
            CreateChallenge("Test Trial", "testtrial", new Vector3(0,0,0));
        }

        public static void CreateChallenge(string triallongname, string trialservername, Vector3 position)
        {
            trialUIObject = bundle.LoadAsset<GameObject>("Trial");
            trialUIObject.name = triallongname;
            trialUIObject.transform.position = position;
        }
        
        }
    }

