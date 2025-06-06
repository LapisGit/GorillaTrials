using BepInEx;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace GorillaTrials
{
    public class LoadChallenges : BaseUnityPlugin
    {
        public static GameObject buttonObject { get; private set; }

        public static void LoadAssetBundle()
        {
            Debug.Log("Loading AssetBundle...");

            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                                          .FirstOrDefault(name => name.EndsWith(Constants.AssetBundleName));

            string tempPath = Path.Combine(Path.GetTempPath(), Constants.AssetBundleName);

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                resourceStream.CopyTo(fileStream);
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(tempPath);
            if (bundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                return;
            }

            // challenge 1
            
            GameObject challenge1 = bundle.LoadAsset<GameObject>("ForestBoxTrial1");
            if (challenge1 == null)
            {
                Debug.Log("Failed to load prefab from AssetBundle!");
                return;
            }

            Vector3 spawnPositionchallenge1 = new Vector3(-66.5785f, 11.8871f, -82.7937f);
            Plugin.loadedPrefab = Instantiate(challenge1, spawnPositionchallenge1, Quaternion.identity);
        }
        
        }
    }

