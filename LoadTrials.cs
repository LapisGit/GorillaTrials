using System.Collections;
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

            // Forest Trials
            CreateChallenge("Stump Climb", "stumpclimb", new Vector3(-65.6918f, 2.5123f, -72.0744f), 180, false, TrialPositions.stumpClimbBoxes);
            CreateChallenge("Cross The Forest", "ctf", new Vector3(-46.75191f,5.50911f,-26.79142f), 180, false, TrialPositions.ctfBoxes);
            // City Trials
            CreateChallenge("Shopping Spree Basics", "shoppingspreebasics", new Vector3(-65.72206f,16.42499f,-121.2781f), 180, false, TrialPositions.shoppingSpreeBasicsBoxes);
            CreateChallenge("Wraparound", "wraparound", new Vector3(-30.88225f,14.99187f,-108.6642f),269.5f, false, TrialPositions.wraparoundBoxes);
            
            // Canyons Trials
            CreateChallenge("Canyon Run", "canyonrun", new Vector3(-80.93035f,10.34146f,-103.9011f),0f, false, TrialPositions.canyonRunBoxes);
            CreateChallenge("Swing", "swing", new Vector3(-87.95385f,9.952705f,-117.7568f),0f, false, TrialPositions.swingBoxes);
            // Caves (NEW) Trials
            CreateChallenge("Cave Run", "caverun", new Vector3(-87.95385f,9.952705f,-117.7568f),0f, false, TrialPositions.caveRunBoxes);
            // Caves (OLD) Trials
            
            // MonkeBlocks Trials
            
            // Clouds Trials
            
            // Beach Trials
            
            // Hoverpark Trials
            
            // Hoverpark2 Trials
            
            
        }

        public static void CreateChallenge(string triallongname, string trialservername, Vector3 position, float yRotation, bool ZoneTrial, List<Vector3> boxData)
        {
            Trial trial = new Trial(position, yRotation, triallongname, trialservername, (int)TrialType.Box, null, boxData);
        }
    }
}

