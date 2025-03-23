using BepInEx;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using System.Collections;

namespace GorillaTrials
{
    [BepInPlugin("com.Lapis.GorillaTrials", "GorillaTrials", "0.0.1")]
    public class Plugin : BaseUnityPlugin
    {
        private GameObject loadedPrefab;

        void Awake()
        {
            Logger.LogInfo("GorillaTrials - Waiting for game to load...");
            StartCoroutine(WaitForGameLoad());
        }

        IEnumerator WaitForGameLoad()
        {
            while (GameObject.Find("Player Objects") == null)
            {
                yield return new WaitForSeconds(1f);
            }

            Logger.LogInfo("Game Loaded! Now initializing mod...");
            LoadAssetBundle();
        }

        void LoadAssetBundle()
        {
            Logger.LogInfo("Loading AssetBundle...");

            string resourceName = Assembly.GetExecutingAssembly().GetManifestResourceNames()
                                          .FirstOrDefault(name => name.EndsWith("mybundle"));

            string tempPath = Path.Combine(Path.GetTempPath(), "mybundle");

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                resourceStream.CopyTo(fileStream);
            }

            AssetBundle bundle = AssetBundle.LoadFromFile(tempPath);
            if (bundle == null)
            {
                Logger.LogError("Failed to load AssetBundle!");
                return;
            }

            GameObject prefab = bundle.LoadAsset<GameObject>("ForestTrial"); 
            if (prefab == null)
            {
                Logger.LogError("Failed to load prefab from AssetBundle!");
                return;
            }

            Vector3 spawnPosition = new Vector3(-66.5785f, 11.8871f, -82.7937f);
            loadedPrefab = Instantiate(prefab, spawnPosition, Quaternion.identity);

            Logger.LogInfo($"Prefab Instantiated at {spawnPosition}");
        }
    }
}
