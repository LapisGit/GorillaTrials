using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using GorillaLocomotion;
using GorillaNetworking;
using GorillaTrials.Behaviours.UI;
using GorillaTrials.Models;
using GorillaTrials.Models.StateMachine;
using GorillaTrials.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaTrials.Behaviours;

public class TrialEditor : MonoBehaviour
{
    public bool inEditorMode = false;
    
    public List<Vector3> positions = new List<Vector3>();
    public static string trialName = "Trial";
    public ETrialType trialType = ETrialType.Box;
    public string TrialDifficulty = "Easy";

    private Vector3 trialStandPosition = Vector3.zero;
    private float trialStandRotation = 0f;
    
    public static TrialEditor instance;
    public GameObject editorUI;
    
    private GameObject trialBoxPrefab;
    private GameObject trialZonePrefab;
    private GameObject trialStandPositionPrefab;
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private GameObject spawnedStandPosition = null;
    
    private float guiMaxTime = 0f;
    private string guiMaxTimeString = "0";
    private float guiBronze = 0f;
    
    private int loadCurrentPage = 1;
    private int loadTotalPages = 1;
    private int trialsPerPage = 9;
    private float guiSilver = 0f;
    private float guiGold = 0f;
    
    private bool lastLeftPrimaryButton = false;
    private bool lastRightPrimaryButton = false;
    private bool lastLeftSecondaryButton = false;
    
    private bool isUploading = false;
    
    private string[] trialFiles;
    private Vector2 loadScrollPosition;
    private string selectedFile = "";

    public GameObject panel;
    
    private Trial playtestTrial = null;
    private bool trialLoadedFromFile = false;
    
    async void Start()
    {
        await Initialize();
        instance = this;
    }
    
    async Task Initialize()
    { 
        GameObject editorPrefab = await AssetLoader.LoadAsset<GameObject>("TrialEditor");
        editorUI = Instantiate(editorPrefab);
        panel = editorUI.transform.Find("Canvas/MainPanel").gameObject;
        panel.transform.rotation = Quaternion.Euler(38.1132f, 242.0654f, 0f);
        panel.transform.position = new Vector3(-68.9191f, 11.9129f, -83.9994f);
        panel.SetActive(false);
        DontDestroyOnLoad(editorUI);
        
        trialBoxPrefab = editorUI.transform.Find("TrialBox").gameObject;
        trialZonePrefab = editorUI.transform.Find("TrialZone").gameObject;
        trialStandPositionPrefab = editorUI.transform.Find("TrialStandPosition").gameObject;
        trialBoxPrefab.SetActive(false);
        trialZonePrefab.SetActive(false);
        trialStandPositionPrefab.SetActive(false);
        
        TrialButton back = editorUI.transform.Find("Canvas/MainPanel/Editor/Back").AddComponent<TrialButton>();
        TrialButton backupload = editorUI.transform.Find("Canvas/MainPanel/Upload/Back").AddComponent<TrialButton>();
        TrialButton backload = editorUI.transform.Find("Canvas/MainPanel/Load/Back").AddComponent<TrialButton>();
        TrialButton close = editorUI.transform.Find("Canvas/MainPanel/TypeSelection/Close").AddComponent<TrialButton>();
        TrialButton zone = editorUI.transform.Find("Canvas/MainPanel/TypeSelection/Zone").AddComponent<TrialButton>();
        TrialButton box = editorUI.transform.Find("Canvas/MainPanel/TypeSelection/Box").AddComponent<TrialButton>();
        TrialButton save = editorUI.transform.Find("Canvas/MainPanel/Editor/Save").AddComponent<TrialButton>();
        TrialButton upload = editorUI.transform.Find("Canvas/MainPanel/Editor/Upload").AddComponent<TrialButton>();
        TrialButton load = editorUI.transform.Find("Canvas/MainPanel/TypeSelection/Load").AddComponent<TrialButton>();
        TrialButton playtest = editorUI.transform.Find("Canvas/MainPanel/Editor/Playtest").AddComponent<TrialButton>();
        TrialButton easy = editorUI.transform.Find("Canvas/MainPanel/Upload/DifficultySelection/Easy").AddComponent<TrialButton>();
        TrialButton medium = editorUI.transform.Find("Canvas/MainPanel/Upload/DifficultySelection/Medium").AddComponent<TrialButton>();
        TrialButton hard = editorUI.transform.Find("Canvas/MainPanel/Upload/DifficultySelection/Hard").AddComponent<TrialButton>();
        TrialButton insane = editorUI.transform.Find("Canvas/MainPanel/Upload/DifficultySelection/Insane").AddComponent<TrialButton>();
        TrialButton extreme = editorUI.transform.Find("Canvas/MainPanel/Upload/DifficultySelection/Extreme").AddComponent<TrialButton>();
        TrialButton editname = editorUI.transform.Find("Canvas/MainPanel/Upload/Trial Name Label/Edit").AddComponent<TrialButton>();
        TrialButton actuallyupload = editorUI.transform.Find("Canvas/MainPanel/Upload/UploadSpecific/Upload").AddComponent<TrialButton>();
        TrialButton actuallysave = editorUI.transform.Find("Canvas/MainPanel/Upload/SaveSpecific/Save").AddComponent<TrialButton>();
        TrialButton endplaytest = editorUI.transform.Find("Canvas/MainPanel/Playtesting/StopPlaytesting").AddComponent<TrialButton>();

        endplaytest.onPressed = () =>
        {
            StopPlaytest();
        };

        actuallysave.onPressed = () =>
        {
            SaveTrialToJsonFile();
            panel.transform.Find("TypeSelection").gameObject.SetActive(true);
            panel.transform.Find("Upload").gameObject.SetActive(false);
            ClearAllSpawnedObjects();
        };
        
        actuallyupload.onPressed = () =>
        {
            SaveTrialToJsonFile();
            StartCoroutine(UploadTrialToServer());
            panel.transform.Find("TypeSelection").gameObject.SetActive(true);
            panel.transform.Find("Upload").gameObject.SetActive(false);
            ClearAllSpawnedObjects();
        };
        
        editname.onPressed = () =>
        {
            OpenKeyboard();
        };
        
        easy.onPressed = () =>
        {
            TrialDifficulty = "Easy";
            var difficultyText = editorUI.transform.Find("Canvas/MainPanel/Upload/SelectedDifficulty")?.GetComponent<TextMeshProUGUI>();
            if (difficultyText != null)
                difficultyText.text = $"Selected: <color=#90EE90>{TrialDifficulty}";
        };
        
        medium.onPressed = () =>
        {
            TrialDifficulty = "Medium";
            var difficultyText = editorUI.transform.Find("Canvas/MainPanel/Upload/SelectedDifficulty")?.GetComponent<TextMeshProUGUI>();
            if (difficultyText != null)
                difficultyText.text = $"Selected: <color=#FDFA72>{TrialDifficulty}";
        };
        
        hard.onPressed = () =>
        {
            TrialDifficulty = "Hard";
            var difficultyText = editorUI.transform.Find("Canvas/MainPanel/Upload/SelectedDifficulty")?.GetComponent<TextMeshProUGUI>();
            if (difficultyText != null)
                difficultyText.text = $"Selected: <color=#FF6700>{TrialDifficulty}";
        };
        
        insane.onPressed = () =>
        {
            TrialDifficulty = "Insane";
            var difficultyText = editorUI.transform.Find("Canvas/MainPanel/Upload/SelectedDifficulty")?.GetComponent<TextMeshProUGUI>();
            if (difficultyText != null)
                difficultyText.text = $"Selected: <color=#EE61BD>{TrialDifficulty}";
        };
        
        extreme.onPressed = () =>
        {
            TrialDifficulty = "Extreme";
            var difficultyText = editorUI.transform.Find("Canvas/MainPanel/Upload/SelectedDifficulty")?.GetComponent<TextMeshProUGUI>();
            if (difficultyText != null)
                difficultyText.text = $"Selected: <color=#FF474D>{TrialDifficulty}";
        };
        
        playtest.onPressed = () =>
        {
            string validationError = ValidateTrial();
            if (!string.IsNullOrEmpty(validationError))
            {
                if (HUDManager.instance != null)
                {
                    HUDManager.instance.SetHUDText(validationError);
                }
                return;
            }
            
            StartPlaytest();
        };
        
        back.onPressed = () =>
        {
            panel.transform.Find("TypeSelection").gameObject.SetActive(true);
            panel.transform.Find("Editor").gameObject.SetActive(false);
            inEditorMode = false;
            ClearAllSpawnedObjects();
        };
        
        backload.onPressed = () =>
        {
            panel.transform.Find("TypeSelection").gameObject.SetActive(true);
            panel.transform.Find("Load").gameObject.SetActive(false);
            inEditorMode = false;
        };
        
        backupload.onPressed = () =>
        {
            panel.transform.Find("Editor").gameObject.SetActive(true);
            panel.transform.Find("Upload").gameObject.SetActive(false);
            inEditorMode = true;
        };
        
        close.onPressed = () =>
        {
            inEditorMode = false;
            editorUI.SetActive(false);
            ClearAllSpawnedObjects();
        };
        
        save.onPressed = () =>
        {
            string validationError = ValidateTrial();
            if (!string.IsNullOrEmpty(validationError))
            {
                if (HUDManager.instance != null)
                {
                    HUDManager.instance.SetHUDText(validationError);
                }
                return;
            }
            
            panel.transform.Find("Editor").gameObject.SetActive(false);
            panel.transform.Find("Upload").gameObject.SetActive(true);
            panel.transform.Find("Upload/UploadSpecific").gameObject.SetActive(false);
            panel.transform.Find("Upload/SaveSpecific").gameObject.SetActive(true);
        };
        
        upload.onPressed = () =>
        {
            string validationError = ValidateTrial();
            if (!string.IsNullOrEmpty(validationError))
            {
                if (HUDManager.instance != null)
                {
                    HUDManager.instance.SetHUDText(validationError);
                }
                Logging.Warning($"Upload blocked: {validationError}");
                return;
            }
            
            panel.transform.Find("Editor").gameObject.SetActive(false);
            panel.transform.Find("Upload").gameObject.SetActive(true);
            panel.transform.Find("Upload/UploadSpecific").gameObject.SetActive(true);
            panel.transform.Find("Upload/SaveSpecific").gameObject.SetActive(false);
        };
        
        box.onPressed = () =>
        {
            trialLoadedFromFile = false;
            trialType = ETrialType.Box;
            editorUI.transform.Find("Canvas/MainPanel/Editor/Info").GetComponent<TextMeshProUGUI>().text =
                "To place a box, click down your left primary button, to delete the last box you placed, click down your right primary button. To set the trial stand position, click down your left secondary button.";
            editorUI.transform.Find("Canvas/MainPanel/TypeSelection").gameObject.SetActive(false);
            editorUI.transform.Find("Canvas/MainPanel/Editor").gameObject.SetActive(true);
            inEditorMode = true;
            editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Boxes").GetComponent<TextMeshProUGUI>().text =
                $"Amount of Boxes Placed: {positions.Count}";
            editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Type").GetComponent<TextMeshProUGUI>().text =
                "Box Trial";
        };
        
        zone.onPressed = () =>
        {
            trialLoadedFromFile = false;
            trialType = ETrialType.Zone;
            editorUI.transform.Find("Canvas/MainPanel/Editor/Info").GetComponent<TextMeshProUGUI>().text =
                "To place a zone, click down your left primary button, to delete the last zone you placed, click down your right primary button. The start zone automatically sets the trial stand position.";
            editorUI.transform.Find("Canvas/MainPanel/TypeSelection").gameObject.SetActive(false);
            editorUI.transform.Find("Canvas/MainPanel/Editor").gameObject.SetActive(true);
            inEditorMode = true;
            editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Boxes").GetComponent<TextMeshProUGUI>().text =
                "";
            editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Type").GetComponent<TextMeshProUGUI>().text =
                "Zone Trial";
        };
        
        load.onPressed = () =>
        {
            panel.transform.Find("TypeSelection").gameObject.SetActive(false);
            panel.transform.Find("Load").gameObject.SetActive(true);
            
            string trialsDir = Path.Combine(Path.GetDirectoryName(Paths.GameRootPath), "trials");
            
            if (!Directory.Exists(trialsDir))
            {
                Directory.CreateDirectory(trialsDir);
            }
            
            trialFiles = Directory.GetFiles(trialsDir, "*.json");
            loadCurrentPage = 1;
            
            Transform nextPageBtnTransform = panel.transform.Find("Load/NextPage");
            if (nextPageBtnTransform != null)
            {
                nextPageBtnTransform.gameObject.layer = (int)UnityLayer.GorillaInteractable;
                TrialButton nextPageBtn = nextPageBtnTransform.GetComponent<TrialButton>();
                if (nextPageBtn == null)
                {
                    nextPageBtn = nextPageBtnTransform.AddComponent<TrialButton>();
                }
                
                nextPageBtn.onPressed = () =>
                {
                    if (loadCurrentPage < loadTotalPages)
                    {
                        loadCurrentPage++;
                        UpdateLoadTrialsUI(trialFiles);
                    }
                };
            }
            
            Transform prevPageBtnTransform = panel.transform.Find("Load/PrevPage");
            if (prevPageBtnTransform != null)
            {
                prevPageBtnTransform.gameObject.layer = (int)UnityLayer.GorillaInteractable;
                TrialButton prevPageBtn = prevPageBtnTransform.GetComponent<TrialButton>();
                if (prevPageBtn == null)
                {
                    prevPageBtn = prevPageBtnTransform.AddComponent<TrialButton>();
                }
                
                prevPageBtn.onPressed = () =>
                {
                    if (loadCurrentPage > 1)
                    {
                        loadCurrentPage--;
                        UpdateLoadTrialsUI(trialFiles);
                    }
                };
            }
            
            UpdateLoadTrialsUI(trialFiles);
        };
    }
    
    void SaveTrialToJsonFile()
    {
        Vector3 positionToUse = trialStandPosition;
        if (!trialLoadedFromFile)
        {
            positionToUse = new Vector3(
                trialStandPosition.x,
                trialStandPosition.y + 0.25f,
                trialStandPosition.z + 0.05f
            );
        }
        
        var trialData = new TrialJson
        {
            displayName = trialName,
            trialId = trialName.ToLower().Replace(" ", ""),
            position = positionToUse,
            angle = trialStandRotation,
            trialType = trialType.ToString(),
            trialDifficulty = TrialDifficulty,
            maxTime = 0,
            customMapTrial = true,
            points = positions,
            bronzeTime = guiBronze,
            silverTime = guiSilver,
            goldTime = guiGold
        };

        string json = JsonUtility.ToJson(trialData, true);
        string trialsDir = Path.Combine(Path.GetDirectoryName(Paths.GameRootPath), "trials");
        
        if (!Directory.Exists(trialsDir))
        {
            Directory.CreateDirectory(trialsDir);
        }
        
        string filePath = Path.Combine(trialsDir, $"{trialName}.json");
        File.WriteAllText(filePath, json);
        
        HUDManager.instance.SetHUDText("Trial saved: "+ trialName);
    }

    void LoadTrialData(TrialJson trialData)
    {
        ClearAllSpawnedObjects();
        trialLoadedFromFile = true;
        
        trialName = trialData.displayName;
        TrialDifficulty = trialData.trialDifficulty;
        trialStandPosition = trialData.position;
        trialStandRotation = trialData.angle;
        
        panel.transform.Find("Upload/TrialName").GetComponent<TextMeshProUGUI>().text = trialName;
        panel.transform.Find("Upload/SelectedDifficulty").GetComponent<TextMeshProUGUI>().text = TrialDifficulty;
        
        if (Enum.TryParse(trialData.trialType, out ETrialType loadedTrialType))
        {
            trialType = loadedTrialType;
        }
        
        positions = new List<Vector3>(trialData.points);
        
        if (trialType == ETrialType.Box)
        {
            foreach (Vector3 pos in positions)
            {
                SpawnBox(pos);
            }
            
            editorUI.transform.Find("Canvas/MainPanel/Editor/Info").GetComponent<TextMeshProUGUI>().text =
                "To place a box, click down your left primary button, to delete the last box you placed, click down your right primary button. To set the trial stand position, click down your left secondary button.";
            editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Boxes").GetComponent<TextMeshProUGUI>().text = $"Amount of Boxes Placed: {positions.Count}";
            editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Type").GetComponent<TextMeshProUGUI>().text =
                "Box Trial";
        }
        else if (trialType == ETrialType.Zone)
        {
            editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Boxes").GetComponent<TextMeshProUGUI>().text = "";
            editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Type").GetComponent<TextMeshProUGUI>().text =
                "Zone Trial";
            if (positions.Count >= 1)
            {
                SpawnZone(positions[0], Quaternion.identity);
                editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/StartZone").GetComponent<TextMeshProUGUI>().text = "Start Zone Point Set";
            }
            if (positions.Count >= 2)
            {
                SpawnZone(positions[1], Quaternion.identity);
                editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/EndZone").GetComponent<TextMeshProUGUI>().text = "End Zone Point Set";
            }
            
            editorUI.transform.Find("Canvas/MainPanel/Editor/Info").GetComponent<TextMeshProUGUI>().text =
                "To place a zone, click down your left primary button, to delete the last zone you placed, click down your right primary button. The start zone automatically sets the trial stand position.";
        }
        
        if (trialStandPosition != Vector3.zero)
        {
            SpawnTrialStandPosition(trialStandPosition, Quaternion.Euler(0, trialStandRotation, 0));
        }
        
        editorUI.transform.Find("Canvas/MainPanel/TypeSelection").gameObject.SetActive(false);
        editorUI.transform.Find("Canvas/MainPanel/Editor").gameObject.SetActive(true);
        inEditorMode = true;
        
        Logging.Info($"Trial '{trialName}' loaded successfully with {positions.Count} points/boxes");
        HUDManager.instance.SetHUDText($"Trial '{trialName}' loaded successfully!");
    }

    private void SpawnBox(Vector3 position)
    {
        GameObject box = Instantiate(trialBoxPrefab);
        box.transform.position = position;
        box.SetActive(true);
        spawnedObjects.Add(box);
    }

    private void SpawnZone(Vector3 position, Quaternion rotation)
    {
        GameObject zone = Instantiate(trialZonePrefab);
        zone.transform.position = position;
        zone.transform.rotation = rotation;
        zone.SetActive(true);
        spawnedObjects.Add(zone);
    }

    private void SpawnTrialStandPosition(Vector3 position, Quaternion rotation)
    {
        Vector3 adjustedPosition = position;
        if (!trialLoadedFromFile)
        {
            adjustedPosition = new Vector3(
                position.x,
                position.y + 0.25f,
                position.z + 0.05f
            );
        }
        
        if (spawnedStandPosition != null)
        {
            Destroy(spawnedStandPosition);
        }
        
        spawnedStandPosition = Instantiate(trialStandPositionPrefab);
        spawnedStandPosition.transform.position = adjustedPosition;
        spawnedStandPosition.transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        spawnedStandPosition.SetActive(true);
    }

    private void RemoveLastSpawnedObject()
    {
        if (spawnedObjects.Count > 0)
        {
            GameObject lastObject = spawnedObjects[spawnedObjects.Count - 1];
            spawnedObjects.RemoveAt(spawnedObjects.Count - 1);
            Destroy(lastObject);
        }
    }

    private void ClearAllSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
        positions.Clear();
        
        if (spawnedStandPosition != null)
        {
            Destroy(spawnedStandPosition);
            spawnedStandPosition = null;
        }
    }

    private void FixedUpdate()
    {
        // stuff for making it so it doesn't spam boxes/zones
        bool currentLeftPrimaryButton = ControllerInputPoller.instance.leftControllerPrimaryButton;
        bool currentRightPrimaryButton = ControllerInputPoller.instance.rightControllerPrimaryButton;
        bool currentLeftSecondaryButton = ControllerInputPoller.instance.leftControllerSecondaryButton;
        
        bool placeButtonDown = currentLeftPrimaryButton && !lastLeftPrimaryButton;
        bool removeButtonDown = currentRightPrimaryButton && !lastRightPrimaryButton;
        bool standPositionButtonDown = currentLeftSecondaryButton && !lastLeftSecondaryButton;

        
        if (inEditorMode && editorUI != null)
        {
            if (standPositionButtonDown && trialType == ETrialType.Box)
            {
                Vector3 standPos = GTPlayer.Instance.LeftHand.handFollower.transform.position;
                Quaternion standRot = GTPlayer.Instance.LeftHand.handFollower.transform.rotation;
                trialStandPosition = standPos;
                trialStandRotation = standRot.eulerAngles.y;
                SpawnTrialStandPosition(standPos, standRot);
                
                Logging.Info($"Trial stand position set at: {standPos} with rotation: {trialStandRotation}");
            }
            
            if (placeButtonDown && trialType == ETrialType.Box)
            {
                Vector3 placePosition = GTPlayer.Instance.LeftHand.handFollower.transform.position;
                positions.Add(placePosition);
                SpawnBox(placePosition);
                var boxesText = editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Boxes")?.GetComponent<TextMeshProUGUI>();
                if (boxesText != null)
                    boxesText.text = $"Amount of Boxes Placed: {positions.Count}";
                Logging.Info($"Box placed at position: {placePosition}");
            }
            if (removeButtonDown && trialType == ETrialType.Box)
            {
                if (positions.Count > 0)
                {
                    Vector3 removedPosition = positions[positions.Count - 1];
                    positions.RemoveAt(positions.Count - 1);
                    RemoveLastSpawnedObject();
                    var boxesText = editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/Boxes")?.GetComponent<TextMeshProUGUI>();
                    if (boxesText != null)
                        boxesText.text = $"Amount of Boxes Placed: {positions.Count}";
                    Logging.Info($"Box removed from position: {removedPosition}");
                }
            }

            if (placeButtonDown && trialType == ETrialType.Zone)
            {
                if (positions.Count == 0)
                {
                    Vector3 startPosition = GTPlayer.Instance.LeftHand.handFollower.transform.position;
                    Quaternion startRotation = GTPlayer.Instance.LeftHand.handFollower.transform.rotation;
                    positions.Add(startPosition);
                    trialStandPosition = startPosition;
                    trialStandRotation = startRotation.eulerAngles.y;
                    SpawnZone(startPosition, Quaternion.identity);
                    SpawnTrialStandPosition(startPosition, startRotation);
                    var startZoneText = editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/StartZone")?.GetComponent<TextMeshProUGUI>();
                    if (startZoneText != null)
                        startZoneText.text = "Start Zone Point Set";
                    Logging.Info($"Zone start point set at position: {startPosition}");
                }
                else if (positions.Count == 1)
                {
                    Vector3 endPosition = GTPlayer.Instance.LeftHand.handFollower.transform.position;
                    positions.Add(endPosition);
                    SpawnZone(endPosition, Quaternion.identity);
                    var endZoneText = editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/EndZone")?.GetComponent<TextMeshProUGUI>();
                    if (endZoneText != null)
                        endZoneText.text = "End Zone Point Set";
                    Logging.Info($"Zone end point set at position: {endPosition}");
                }
                else if (positions.Count >= 2)
                {
                    Logging.Info("dude why are you trying to add more than 2 points to a zone trial, smh...");
                }
            }
            if (removeButtonDown && trialType == ETrialType.Zone)
            {
                if (positions.Count > 0)
                {
                    Vector3 removedPosition = positions[positions.Count - 1];
                    positions.RemoveAt(positions.Count - 1);
                    RemoveLastSpawnedObject();
                    if (positions.Count == 1)
                    {
                        var endZoneText = editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/EndZone")?.GetComponent<TextMeshProUGUI>();
                        if (endZoneText != null)
                            endZoneText.text = "End Zone Point Removed";
                    }
                    else if (positions.Count == 0)
                    {
                        trialStandPosition = Vector3.zero;
                        trialStandRotation = 0f;
                        if (spawnedStandPosition != null)
                        {
                            Destroy(spawnedStandPosition);
                            spawnedStandPosition = null;
                        }
                        var startZoneText = editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/StartZone")?.GetComponent<TextMeshProUGUI>();
                        if (startZoneText != null)
                            startZoneText.text = "Start Zone Point Removed";
                    }
                    Logging.Info($"Zone point removed from position: {removedPosition}");
                }
            }
        }
        
        lastLeftPrimaryButton = currentLeftPrimaryButton;
        lastRightPrimaryButton = currentRightPrimaryButton;
        lastLeftSecondaryButton = currentLeftSecondaryButton;
    }
    
    private string ValidateTrial()
    {
        if (trialStandPosition == Vector3.zero)
        {
            return "Trial stand position not set!";
        }
        
        if (trialType == ETrialType.Box)
        {
            if (positions.Count < 1)
            {
                return "Box trial must have at least 1 box!";
            }
        }
        else if (trialType == ETrialType.Zone)
        {
            if (positions.Count < 2)
            {
                return "Zone trial must have at least 2 zones (start and end)!";
            }
        }
        
        return null;
    }

    private void UpdateLoadTrialsUI(string[] trialFilePaths)
    {
        if (trialFilePaths == null || trialFilePaths.Length == 0)
        {
            Transform noneTransform = panel.transform.Find("Load/None");
            if (noneTransform != null)
            {
                noneTransform.gameObject.SetActive(true);
            }
            
            Transform pageTextTransform = panel.transform.Find("Load/Text/Page");
            if (pageTextTransform != null)
            {
                pageTextTransform.GetComponent<TextMeshProUGUI>().text = "";
            }
            
            Transform optionsContainer = panel.transform.Find("Load/Options");
            if (optionsContainer != null)
            {
                for (int i = 1; i <= 9; i++)
                {
                    optionsContainer.Find(i.ToString())?.gameObject.SetActive(false);
                }
            }
            return;
        }
        
        Transform noneTransform2 = panel.transform.Find("Load/None");
        if (noneTransform2 != null)
        {
            noneTransform2.gameObject.SetActive(false);
        }
        
        loadTotalPages = Mathf.CeilToInt(trialFilePaths.Length / (float)trialsPerPage);
        if (loadCurrentPage > loadTotalPages)
        {
            loadCurrentPage = loadTotalPages;
        }
        
        Transform pageTextTransform2 = panel.transform.Find("Load/Text/Page");
        if (pageTextTransform2 != null)
        {
            pageTextTransform2.GetComponent<TextMeshProUGUI>().text = $"Page {loadCurrentPage}/{loadTotalPages}";
        }
        
        Transform optionsContainer2 = panel.transform.Find("Load/Options");
        if (optionsContainer2 == null) return;
        
        int startIndex = (loadCurrentPage - 1) * trialsPerPage;
        int endIndex = Mathf.Min(startIndex + trialsPerPage, trialFilePaths.Length);
        
        for (int i = 1; i <= 9; i++)
        {
            Transform trialSlot = optionsContainer2.Find(i.ToString());
            if (trialSlot == null) continue;
            
            int actualIndex = startIndex + (i - 1);
            
            if (actualIndex < endIndex)
            {
                string filePath = trialFilePaths[actualIndex];
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                
                trialSlot.gameObject.SetActive(true);
                trialSlot.gameObject.layer = (int)UnityLayer.GorillaInteractable;
                
                Transform nameTransform = trialSlot.Find("TrialName");
                if (nameTransform != null)
                {
                    TextMeshProUGUI nameText = nameTransform.GetComponent<TextMeshProUGUI>();
                    if (nameText != null)
                    {
                        nameText.text = fileName;
                    }
                }
                
                TrialButton slotButton = trialSlot.GetComponent<TrialButton>();
                if (slotButton == null)
                {
                    slotButton = trialSlot.gameObject.AddComponent<TrialButton>();
                }
                
                string trialFilePath = filePath;
                slotButton.onPressed = () =>
                {
                    try
                    {
                        string json = File.ReadAllText(trialFilePath);
                        TrialJson trialData = JsonUtility.FromJson<TrialJson>(json);
                        LoadTrialData(trialData);
                        
                        panel.transform.Find("Load").gameObject.SetActive(false);
                        panel.transform.Find("Editor").gameObject.SetActive(true);
                    }
                    catch (Exception ex)
                    {
                        Logging.Error($"Failed to load trial '{fileName}': {ex.Message}");
                        if (HUDManager.instance != null)
                        {
                            HUDManager.instance.SetHUDText($"Failed to load trial: {ex.Message}");
                        }
                    }
                };
            }
            else
            {
                trialSlot.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator UploadTrialToServer()
    {
        string validationError = ValidateTrial();
        if (!string.IsNullOrEmpty(validationError))
        {
            Logging.Error($"Upload validation failed: {validationError}");
            isUploading = false;
            yield break;
        }
        
        isUploading = true;
        
        // Only apply adjustment if trial wasn't loaded from a file
        Vector3 positionToUse = trialStandPosition;
        if (!trialLoadedFromFile)
        {
            positionToUse = new Vector3(
                trialStandPosition.x,
                trialStandPosition.y + 0.25f,
                trialStandPosition.z + 0.05f
            );
        }
        
        var trialData = new TrialJson
        {
            displayName = trialName,
            trialId = trialName.ToLower().Replace(" ", ""),
            position = positionToUse,
            angle = trialStandRotation,
            trialType = trialType.ToString(),
            trialDifficulty = TrialDifficulty,
            maxTime = 0,
            customMapTrial = true,
            points = positions
        };
        
        string json = JsonUtility.ToJson(trialData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        
        string url = $"{Constants.ServerURL}/trials/upload";
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            string playerId = PlayFabAuthenticator.instance.GetPlayFabPlayerId();
            
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", Plugin.APIKey.Value);
            www.SetRequestHeader("playerid", playerId);

            
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 401)
                {
                    Logging.Error("Upload failed: Unauthorized (401)");
                }
                else if (www.responseCode == 429)
                {
                    Logging.Error("Upload failed: Rate limited (429)");
                }
                else if (www.responseCode == 400)
                {
                    Logging.Error("Upload failed: Bad request (400)");
                }
                else if (www.responseCode == 500)
                {
                    Logging.Error("Upload failed: Server error (500)");
                }
                else
                {
                    Logging.Error($"Upload failed: {www.error}");
                }
                
                isUploading = false;
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<UploadResponse>(www.downloadHandler.text);
                    Logging.Info($"Trial uploaded successfully! Trial ID: {response.trialId}");
                    
                    ControlPanel.IncrementCustomTrialsUploaded();
                    
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Trial '{trialName}' uploaded successfully!");
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error($"Failed to parse upload response: {ex.Message}");
                }
                
                yield return new WaitForSeconds(3f);
                isUploading = false;
            }
        }
    }
    
    private void OpenKeyboard()
    {
        TrialKeyboard keyboard = FindFirstObjectByType<TrialKeyboard>();

        keyboard.forUsername = true;
        
        keyboard.SetMaxLength(30);
        
        keyboard.onSubmit = (text) =>
        {
            trialName = text;
            panel.transform.Find("Upload/TrialName").GetComponent<TextMeshProUGUI>().text = trialName;
            keyboard.keyboard.SetActive(false);
        };
        
        keyboard.onCancel = () =>
        {
            keyboard.keyboard.SetActive(false);
        };
        
        keyboard.keyboard.SetActive(true);
    }

    [Serializable]
    public class UploadResponse
    {
        public string message;
        public string trialId;
    }

    [Serializable]
    public class TrialJson
    {
        public string displayName;
        public string trialId;
        public Vector3 position;
        public float angle;
        public string trialType;
        public string trialDifficulty;
        public float maxTime;
        public bool customMapTrial;
        public List<Vector3> points;
        public float bronzeTime;
        public float silverTime;
        public float goldTime;
    }

    private void StartPlaytest()
    {
        try
        {
            if (!Enum.TryParse(TrialDifficulty, true, out ETrialDifficulty difficulty))
            {
                difficulty = ETrialDifficulty.Easy;
            }

            string playtestTrialId = $"playtest_{trialName.ToLower().Replace(" ", "")}";

            Vector3 positionToUse = trialStandPosition;
            if (!trialLoadedFromFile)
            {
                positionToUse = new Vector3(
                    trialStandPosition.x,
                    trialStandPosition.y + 0.25f,
                    trialStandPosition.z + 0.05f
                );
            }

            TrialManager.Instance.CreateTrial(
                trialName,
                playtestTrialId,
                positionToUse,
                trialStandRotation,
                trialType,
                difficulty,
                0f,
                true,
                trialType == ETrialType.Box ? new object[] { positions } : new object[] { positions },
                0,
                0,
                0,
                null,
                true
            );
            
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
            
            if (spawnedStandPosition != null)
            {
                spawnedStandPosition.SetActive(false);
            }
            
            inEditorMode = false;
            panel.transform.Find("Editor").gameObject.SetActive(false);
            panel.transform.Find("Playtesting").gameObject.SetActive(true);
        }
        catch (Exception ex)
        {
            Logging.Error($"Error starting playtest: {ex.Message}");
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText($"Error starting playtest: {ex.Message}");
            }
        }
    }

    public void StopPlaytest()
    {
        try
        {
            TrialManager.Instance.DeleteAllPlaytestTrials();
            
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
            
            if (spawnedStandPosition != null)
            {
                spawnedStandPosition.SetActive(true);
            }
            
            panel.transform.Find("Playtesting").gameObject.SetActive(false);
            panel.transform.Find("Editor").gameObject.SetActive(true);
            inEditorMode = true;
        }
        catch (Exception ex)
        {
            Logging.Error($"Error stopping playtest: {ex.Message}");
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText($"Error stopping playtest: {ex.Message}");
            }
        }
    }
}
