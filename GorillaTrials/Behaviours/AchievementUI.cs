using GorillaTrials.Tools;
using System.Threading.Tasks;
using GorillaTrials.Behaviours.UI;
using TMPro;
using UnityEngine;

namespace GorillaTrials.Behaviours;

public class AchievementUI : MonoBehaviour
{
    public static AchievementUI instance;
    public GameObject achievementUIRoot, achievementUI;
    public int currentPage = 1;
    public int maxPage = 2;
    public int minPage = 1;
    async void Start()
    {
        await Initialize();
        UpdateAchievements();
    }

    async Task Initialize()
    {
        achievementUIRoot = await AssetLoader.LoadAsset<GameObject>("AchievementsUI");
        TrialManager.Instance.achievementsUI = achievementUIRoot;
        achievementUIRoot = Instantiate(achievementUIRoot);
        DontDestroyOnLoad(achievementUIRoot);
        achievementUIRoot.transform.position = new Vector3(-69.3592f, 12.1929f,-83.4284f);
        achievementUIRoot.transform.rotation = Quaternion.Euler(358.9055f, 242.0654f, 0f);

        achievementUI = achievementUIRoot.transform.Find("UI").gameObject;

        achievementUI.transform.Find("Buttons/PrevPage").gameObject.layer = (int)UnityLayer.GorillaInteractable;
        achievementUI.transform.Find("Buttons/NextPage").gameObject.layer = (int)UnityLayer.GorillaInteractable;
        TrialButton nextpage = achievementUI.transform.Find("Buttons/NextPage").AddComponent<TrialButton>();
        TrialButton prevpage = achievementUI.transform.Find("Buttons/PrevPage").AddComponent<TrialButton>();
        achievementUI.transform.Find("Info/Page").gameObject.GetComponent<TextMeshProUGUI>().text = $"Page {currentPage}/{maxPage}";
        
 
        nextpage.onPressed = () =>
        {
            achievementUI.transform.Find($"Achievements/Page{currentPage}").gameObject.SetActive(false);
            currentPage += 1;
            if (currentPage > maxPage)
            {
                currentPage = maxPage;
            }
            achievementUI.transform.Find($"Achievements/Page{currentPage}").gameObject.SetActive(true);
            achievementUI.transform.Find("Info/Page").gameObject.GetComponent<TextMeshProUGUI>().text = $"Page {currentPage}/{maxPage}";
            UpdateAchievements();
        };

        prevpage.onPressed = () =>
        {
            achievementUI.transform.Find($"Achievements/Page{currentPage}").gameObject.SetActive(false);
            currentPage -= 1;
            if (currentPage < minPage)
            {
                currentPage = minPage;
            }
            achievementUI.transform.Find($"Achievements/Page{currentPage}").gameObject.SetActive(true);
            achievementUI.transform.Find("Info/Page").gameObject.GetComponent<TextMeshProUGUI>().text = $"Page {currentPage}/{maxPage}";
            UpdateAchievements();
        };
    }


    public void TestLOL()
    {
        UpdateAchievements();
    }
    public void UpdateAchievements()
    {
        if (Plugin.achievementManager.IsUnlocked("first_trial"))
        {
            achievementUI.transform.Find("Achievements/Page1/FirstTrial/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("stump_climb_champ"))
        {
            achievementUI.transform.Find("Achievements/Page1/StumpClimbMaster/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("adv_hp2"))
        {
            achievementUI.transform.Find("Achievements/Page1/HP2SM/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("5trials"))
        {
            achievementUI.transform.Find("Achievements/Page1/5Trials/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("10trials"))
        {
            achievementUI.transform.Find("Achievements/Page1/10Trials/CompletedText").gameObject.SetActive(true);
        }
        if (Plugin.achievementManager.IsUnlocked("20trials"))
        {
            achievementUI.transform.Find("Achievements/Page1/20Trials/CompletedText").gameObject.SetActive(true);
        }
    }
}