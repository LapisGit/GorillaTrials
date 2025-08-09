using System;
using System.Threading.Tasks;
using GorillaTrials.Behaviours.UI;
using GorillaTrials.Tools;
using PlayFab.CloudScriptModels;
using TMPro;
using UnityEngine;

namespace GorillaTrials.Behaviours;

// not done yet
public class FirstTimeUIManager : MonoBehaviour
{
    public static FirstTimeUIManager instance;

    public GameObject UIRoot, UI;
    public int currentPage = 1;
    public int maxPage = 2;
    public int minPage = 1;

    void Awake()
    {
        instance = this;
        if (PlayerPrefs.GetString("firsttimedone") != "yes :3")
        { 
           Initialize();
        }
    }

    async Task Initialize()
    {
        
        UIRoot = await AssetLoader.LoadAsset<GameObject>("FirstTimeUI");
        UIRoot = Instantiate(UIRoot);
        DontDestroyOnLoad(UIRoot);
        UIRoot.transform.position = new Vector3(-67.8684f, 11.9874f, -84.028f);
        UIRoot.transform.rotation = Quaternion.Euler(358.9055f, 50f, 0f);

        UI = UIRoot.transform.Find("UI").gameObject;

        UI.transform.Find("Buttons/PrevPage").gameObject.layer = (int)UnityLayer.GorillaInteractable;
        UI.transform.Find("Buttons/NextPage").gameObject.layer = (int)UnityLayer.GorillaInteractable;
        UI.transform.Find("Buttons/Done").gameObject.layer = (int)UnityLayer.GorillaInteractable;
        TrialButton nextpage = UI.transform.Find("Buttons/NextPage").AddComponent<TrialButton>();
        TrialButton prevpage = UI.transform.Find("Buttons/PrevPage").AddComponent<TrialButton>();
        TrialButton done = UI.transform.Find("Buttons/Done").AddComponent<TrialButton>();
        UI.transform.Find("Buttons/Done").gameObject.GetComponent<BoxCollider>().isTrigger = true;
        UI.transform.Find("Buttons/NextPage").gameObject.GetComponent<BoxCollider>().isTrigger = true;
        UI.transform.Find("Buttons/PrevPage").gameObject.GetComponent<BoxCollider>().isTrigger = true;


        nextpage.onPressed = () =>
        {
            UI.transform.Find($"StuffLol/Page{currentPage}").gameObject.SetActive(false);
            currentPage += 1;
            if (currentPage > maxPage)
            {
                currentPage = maxPage;
            }

            if (currentPage == maxPage)
            {
                UI.transform.Find("Buttons/Done").gameObject.SetActive(true);
            }

            if (currentPage != maxPage)
            {
                UI.transform.Find("Buttons/Done").gameObject.SetActive(false);
            }

            UI.transform.Find($"StuffLol/Page{currentPage}").gameObject.SetActive(true);
        };

        prevpage.onPressed = () =>
        {
            UI.transform.Find($"StuffLol/Page{currentPage}").gameObject.SetActive(false);
            currentPage -= 1;
            if (currentPage < minPage)
            {
                currentPage = minPage;
            }

            if (currentPage == maxPage)
            {
                UI.transform.Find("Buttons/Done").gameObject.SetActive(true);
            }

            if (currentPage != maxPage)
            {
                UI.transform.Find("Buttons/Done").gameObject.SetActive(false);
            }

            UI.transform.Find($"StuffLol/Page{currentPage}").gameObject.SetActive(true);
        };

        done.onPressed = () =>
        {
            UIRoot.gameObject.SetActive(false);
            PlayerPrefs.SetString("firsttimedone", "yes :3");
            PlayerPrefs.Save();
        };
    }
}
