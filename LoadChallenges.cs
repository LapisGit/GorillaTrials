using BepInEx;
using GorillaTrials.Behaviors.UI;
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

            GameObject challenge1 = bundle.LoadAsset<GameObject>("ForestBoxTrial1");
            if (challenge1 == null)
            {
                Debug.Log("Failed to load prefab from AssetBundle!");
                return;
            }

            Vector3 spawnPositionchallenge1 = new Vector3(-66.5785f, 11.8871f, -82.7937f);
            Plugin.loadedPrefab = Instantiate(challenge1, spawnPositionchallenge1, Quaternion.identity);

            Debug.Log($"Prefab Instantiated at {spawnPositionchallenge1}");


            // challenge2

            GameObject challenge2 = bundle.LoadAsset<GameObject>("ForestZoneTrial1");
            if (challenge1 == null)
            {
                Debug.Log("Failed to load prefab from AssetBundle!");
                return;
            }

            Vector3 spawnPositionchallenge2 = new Vector3(-67.5785f, 11.8871f, -83.7937f);
            Plugin.loadedPrefab = Instantiate(challenge2, spawnPositionchallenge2, Quaternion.identity);

            Debug.Log($"Prefab Instantiated at {spawnPositionchallenge2}");


        }

        public static void FindAndModifyButton()
        {
            // Locate the button in the scene
            buttonObject = GameObject.Find("ForestZoneTrial1(Copy)/Stool/Button");

            if (buttonObject == null)
            {
                Debug.LogError("Button not found in scene!");
                return;
            }

            // Ensure it's on the correct layer
            if (buttonObject.layer != 18)
            {
                Debug.LogWarning("Button is not on the GorillaInteraction layer!");
            }

            // Locate the FinishZone object that needs to be activated
            GameObject finishZone = GameObject.Find("ForestZoneTrial1(Copy)/FinishZone");
            if (finishZone == null)
            {
                Debug.LogError("FinishZone not found in scene!");
                return;
            }

            // Add the Button script if not already present
            Button buttonScript = buttonObject.GetComponent<Button>();
            if (buttonScript == null)
                buttonScript = buttonObject.AddComponent<Button>();

            // Assign an action to log a message and activate FinishZone
            buttonScript.action = () =>
            {
                Debug.Log("Trial Button Pressed!");
                finishZone.SetActive(true);  // Activate FinishZone
            };
        }
    }
}
