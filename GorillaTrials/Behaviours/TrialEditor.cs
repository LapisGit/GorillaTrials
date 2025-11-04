using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using GorillaLocomotion;
using GorillaTrials.Behaviours.UI;
using GorillaTrials.Models;
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
    
    private bool showSaveDialog = false;
    private bool showUploadDialog = false;
    private bool showLoadDialog = false;
    private Rect saveWindowRect = new Rect(100, 100, 500, 450);
    private Rect uploadWindowRect = new Rect(100, 100, 500, 500);
    private Rect loadWindowRect = new Rect(100, 100, 700, 550);
    private string guiTrialName = "Trial";
    private string guiTrialDifficulty = "Easy";
    private float guiMaxTime = 0f;
    private string guiMaxTimeString = "0";
    
    private bool lastLeftPrimaryButton = false;
    private bool lastRightPrimaryButton = false;
    private bool lastLeftSecondaryButton = false;
    
    private bool isUploading = false;
    private string uploadStatusMessage = "";
    
    private string[] trialFiles;
    private Vector2 loadScrollPosition;
    private string selectedFile = "";
    
    async void Start()
    {
        await Initialize();
    }
    
    async Task Initialize()
    { 
        GameObject editorPrefab = await AssetLoader.LoadAsset<GameObject>("TrialEditor");
        editorUI = Instantiate(editorPrefab);
        GameObject panel = editorUI.transform.Find("Canvas/MainPanel").gameObject;
        panel.transform.rotation = Quaternion.Euler(38.1132f, 242.0654f, 0f);
        panel.transform.position = new Vector3(-68.9191f, 11.9129f, -83.9994f);
        panel.SetActive(true);
        DontDestroyOnLoad(editorUI);
        
        trialBoxPrefab = editorUI.transform.Find("TrialBox").gameObject;
        trialZonePrefab = editorUI.transform.Find("TrialZone").gameObject;
        trialStandPositionPrefab = editorUI.transform.Find("TrialStandPosition").gameObject;
        trialBoxPrefab.SetActive(false);
        trialZonePrefab.SetActive(false);
        trialStandPositionPrefab.SetActive(false);
        
        TrialButton back = editorUI.transform.Find("Canvas/MainPanel/TypeSelection/Close").AddComponent<TrialButton>();
        TrialButton close = editorUI.transform.Find("Canvas/MainPanel/Editor/Back").AddComponent<TrialButton>();
        TrialButton zone = editorUI.transform.Find("Canvas/MainPanel/TypeSelection/Zone").AddComponent<TrialButton>();
        TrialButton box = editorUI.transform.Find("Canvas/MainPanel/TypeSelection/Box").AddComponent<TrialButton>();
        TrialButton save = editorUI.transform.Find("Canvas/MainPanel/Editor/Save").AddComponent<TrialButton>();
        TrialButton upload = editorUI.transform.Find("Canvas/MainPanel/Editor/Upload").AddComponent<TrialButton>();
        TrialButton load = editorUI.transform.Find("Canvas/MainPanel/TypeSelection/Load").AddComponent<TrialButton>();
        
        back.onPressed = () =>
        {
            panel.transform.Find("TypeSelection").gameObject.SetActive(true);
            panel.transform.Find("Editor").gameObject.SetActive(false);
            inEditorMode = false;
            ClearAllSpawnedObjects();
        };
        
        close.onPressed = () =>
        {
            inEditorMode = false;
            editorUI.SetActive(false);
            ClearAllSpawnedObjects();
        };
        
        save.onPressed = () =>
        {
            showSaveDialog = true;
            guiTrialName = trialName;
            guiTrialDifficulty = TrialDifficulty;
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Continue with saving your Trial in the window on your desktop.");
                StartCoroutine(ClearHUDDelayed(5f));
            }
        };
        
        upload.onPressed = () =>
        {
            showUploadDialog = true;
            guiTrialName = trialName;
            guiTrialDifficulty = TrialDifficulty;
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Continue with uploading your Trial in the window on your desktop.");
                StartCoroutine(ClearHUDDelayed(5f));
            }
        };
        
        box.onPressed = () =>
        {
            trialType = ETrialType.Box;
            editorUI.transform.Find("Canvas/MainPanel/Editor/Info").GetComponent<TextMeshProUGUI>().text =
                "To place a box, click down your left primary button, to delete the last box you placed, click down your right primary button. To set the trial stand position, click down your left secondary button.";
            editorUI.transform.Find("Canvas/MainPanel/TypeSelection").gameObject.SetActive(false);
            editorUI.transform.Find("Canvas/MainPanel/Editor").gameObject.SetActive(true);
            inEditorMode = true;
        };
        
        zone.onPressed = () =>
        {
            trialType = ETrialType.Zone;
            editorUI.transform.Find("Canvas/MainPanel/Editor/Info").GetComponent<TextMeshProUGUI>().text =
                "To place a zone, click down your left primary button, to delete the last zone you placed, click down your right primary button. The start zone automatically sets the trial stand position.";
            editorUI.transform.Find("Canvas/MainPanel/TypeSelection").gameObject.SetActive(false);
            editorUI.transform.Find("Canvas/MainPanel/Editor").gameObject.SetActive(true);
            inEditorMode = true;
        };
        
        load.onPressed = () =>
        {
            string trialsDir = Path.Combine(Path.GetDirectoryName(Paths.ExecutablePath), "trials");
            
            if (!Directory.Exists(trialsDir))
            {
                Directory.CreateDirectory(trialsDir);
            }
            
            trialFiles = Directory.GetFiles(trialsDir, "*.json");
            showLoadDialog = true;
            selectedFile = "";
            if (HUDManager.instance != null)
            {
                HUDManager.instance.SetHUDText("Select a trial file from the window on your desktop.");
                StartCoroutine(ClearHUDDelayed(5f));
            }
        };
    }
    
    void SaveTrialToJsonFile()
    {
        var trialData = new TrialJson
        {
            displayName = trialName,
            trialId = trialName.ToLower().Replace(" ", ""),
            position = trialStandPosition,
            angle = trialStandRotation,
            trialType = trialType.ToString(),
            trialDifficulty = TrialDifficulty,
            maxTime = 0,
            customMapTrial = true,
            points = positions
        };

        string json = JsonUtility.ToJson(trialData, true);
        string trialsDir = Path.Combine(Path.GetDirectoryName(Paths.ExecutablePath), "trials");
        
        // create directory if it doesn't exit, i think it should auto create but just to be safe
        if (!Directory.Exists(trialsDir))
        {
            Directory.CreateDirectory(trialsDir);
        }
        
        string filePath = Path.Combine(trialsDir, $"{trialName}.json");
        File.WriteAllText(filePath, json);
        
        Logging.Info($"Trial saved to: {filePath}");
    }

    void LoadTrialData(TrialJson trialData)
    {
        ClearAllSpawnedObjects();
        
        trialName = trialData.displayName;
        guiTrialName = trialData.displayName;
        TrialDifficulty = trialData.trialDifficulty;
        guiTrialDifficulty = trialData.trialDifficulty;
        trialStandPosition = trialData.position;
        trialStandRotation = trialData.angle;
        
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
        }
        else if (trialType == ETrialType.Zone)
        {
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
        StartCoroutine(ClearHUDDelayed(3f));
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
        if (spawnedStandPosition != null)
        {
            Destroy(spawnedStandPosition);
        }
        
        spawnedStandPosition = Instantiate(trialStandPositionPrefab);
        spawnedStandPosition.transform.position = position;
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
                
                var standPosText = editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/StandPosition")?.GetComponent<TextMeshProUGUI>();
                if (standPosText != null)
                    standPosText.text = $"Stand Position Set: {standPos}";
                
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
                        // Clear trial stand position when start zone is removed
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
                        var standPosText = editorUI.transform.Find("Canvas/MainPanel/Editor/TrialData/StandPosition")?.GetComponent<TextMeshProUGUI>();
                        if (standPosText != null)
                            standPosText.text = "Stand Position: Not Set";
                    }
                    Logging.Info($"Zone point removed from position: {removedPosition}");
                }
            }
        }
        
        lastLeftPrimaryButton = currentLeftPrimaryButton;
        lastRightPrimaryButton = currentRightPrimaryButton;
        lastLeftSecondaryButton = currentLeftSecondaryButton;
    }

    private void OnGUI()
    {
        GUI.skin.label.fontSize = 14;
        GUI.skin.button.fontSize = 14;
        GUI.skin.textField.fontSize = 14;
        GUI.skin.toggle.fontSize = 14;
        GUI.skin.box.fontSize = 12;
        
        if (showSaveDialog)
        {
            saveWindowRect = GUI.Window(1, saveWindowRect, SaveDialogWindow, "Save Trial");
        }
        
        if (showUploadDialog)
        {
            uploadWindowRect = GUI.Window(2, uploadWindowRect, UploadDialogWindow, "Upload Trial");
        }
        
        if (showLoadDialog)
        {
            loadWindowRect = GUI.Window(3, loadWindowRect, LoadDialogWindow, "Load Trial");
        }
    }

    private void SaveDialogWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        GUILayout.Space(10);
        
        GUILayout.Label("Trial Name:");
        guiTrialName = GUILayout.TextField(guiTrialName, 50);
        
        GUILayout.Space(10);
        
        GUILayout.Label("Difficulty:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(guiTrialDifficulty == "Easy", "Easy"))
            guiTrialDifficulty = "Easy";
        if (GUILayout.Toggle(guiTrialDifficulty == "Medium", "Medium"))
            guiTrialDifficulty = "Medium";
        if (GUILayout.Toggle(guiTrialDifficulty == "Hard", "Hard"))
            guiTrialDifficulty = "Hard";
        if (GUILayout.Toggle(guiTrialDifficulty == "Insane", "Insane"))
            guiTrialDifficulty = "Insane";
        if (GUILayout.Toggle(guiTrialDifficulty == "Extreme", "Extreme"))
            guiTrialDifficulty = "Extreme";
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        GUILayout.Label($"Trial Type: {trialType}");
        GUILayout.Label($"Points/Boxes: {positions.Count}");
        
        GUILayout.Space(20);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.Height(30)))
        {
            trialName = guiTrialName;
            TrialDifficulty = guiTrialDifficulty;
            SaveTrialToJsonFile();
            showSaveDialog = false;
            Logging.Info($"Saving Trial '{trialName}'...");
        }
        
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
        {
            showSaveDialog = false;
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    private void UploadDialogWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        GUILayout.Space(10);
        
        GUILayout.Label("Trial Name:");
        guiTrialName = GUILayout.TextField(guiTrialName, 50);
        
        GUILayout.Space(10);
        
        GUILayout.Label("Difficulty:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(guiTrialDifficulty == "Easy", "Easy"))
            guiTrialDifficulty = "Easy";
        if (GUILayout.Toggle(guiTrialDifficulty == "Medium", "Medium"))
            guiTrialDifficulty = "Medium";
        if (GUILayout.Toggle(guiTrialDifficulty == "Hard", "Hard"))
            guiTrialDifficulty = "Hard";
        if (GUILayout.Toggle(guiTrialDifficulty == "Insane", "Insane"))
            guiTrialDifficulty = "Insane";
        if (GUILayout.Toggle(guiTrialDifficulty == "Extreme", "Extreme"))
            guiTrialDifficulty = "Extreme";
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        GUILayout.Label($"Trial Type: {trialType}");
        GUILayout.Label($"Points/Boxes: {positions.Count}");
        
        GUILayout.Space(10);
        
        GUILayout.Label("Upload Notes:");
        GUILayout.Label("This will upload your Trial for ANYONE to play, and will be listed under YOUR NAME. Uploading a Trial will also save it locally to your computer.", GUI.skin.box);
        
        GUILayout.Space(10);
        
        if (!string.IsNullOrEmpty(uploadStatusMessage))
        {
            GUILayout.Label(uploadStatusMessage, GUI.skin.box);
        }
        
        GUILayout.Space(20);
        
        GUI.enabled = !isUploading;
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(isUploading ? "Uploading..." : "Upload", GUILayout.Height(30)))
        {
            trialName = guiTrialName;
            TrialDifficulty = guiTrialDifficulty;
            SaveTrialToJsonFile();
            StartCoroutine(UploadTrialToServer());
        }
        
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
        {
            if (!isUploading)
            {
                showUploadDialog = false;
                uploadStatusMessage = "";
            }
        }
        GUILayout.EndHorizontal();
        GUI.enabled = true;
        
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }

    private void LoadDialogWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        GUILayout.Space(10);
        
        GUILayout.Label("Select a Trial to Load:");
        GUILayout.Label($"Trials Directory: {Path.Combine(Path.GetDirectoryName(Paths.ExecutablePath), "trials")}", GUI.skin.box);
        
        GUILayout.Space(10);
        
        if (trialFiles == null || trialFiles.Length == 0)
        {
            GUILayout.Label("No trial files found in the trials directory.", GUI.skin.box);
        }
        else
        {
            loadScrollPosition = GUILayout.BeginScrollView(loadScrollPosition, GUILayout.Height(300));
            
            foreach (string filePath in trialFiles)
            {
                string fileName = Path.GetFileName(filePath);
                bool isSelected = selectedFile == filePath;
                
                GUILayout.BeginHorizontal(GUI.skin.box);
                
                if (GUILayout.Toggle(isSelected, "", GUILayout.Width(20)))
                {
                    selectedFile = filePath;
                }
                
                GUILayout.Label(fileName);
                
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndScrollView();
        }
        
        GUILayout.Space(10);
        
        GUILayout.Label(!string.IsNullOrEmpty(selectedFile) ? $"Selected: {Path.GetFileName(selectedFile)}" : "No file selected");
        
        GUILayout.Space(20);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Load", GUILayout.Height(30)))
        {
            if (!string.IsNullOrEmpty(selectedFile) && File.Exists(selectedFile))
            {
                try
                {
                    string json = File.ReadAllText(selectedFile);
                    TrialJson trialData = JsonUtility.FromJson<TrialJson>(json);
                    LoadTrialData(trialData);
                    showLoadDialog = false;
                }
                catch (Exception ex)
                {
                    Logging.Error($"Failed to load trial: {ex.Message}");
                    HUDManager.instance.SetHUDText($"Failed to load trial: {ex.Message}");
                    StartCoroutine(ClearHUDDelayed(5f));
                }
            }
            else
            {
                HUDManager.instance.SetHUDText("Please select a valid trial file.");
                StartCoroutine(ClearHUDDelayed(3f));
            }
        }
        
        if (GUILayout.Button("Refresh", GUILayout.Height(30)))
        {
            string trialsDir = Path.Combine(Path.GetDirectoryName(Paths.ExecutablePath), "trials");
            trialFiles = Directory.GetFiles(trialsDir, "*.json");
            selectedFile = "";
        }
        
        if (GUILayout.Button("Cancel", GUILayout.Height(30)))
        {
            showLoadDialog = false;
        }
        GUILayout.EndHorizontal();
        
        GUILayout.EndVertical();
        GUI.DragWindow(new Rect(0, 0, 10000, 10000));
    }
    
    private IEnumerator ClearHUDDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        HUDManager.instance.ClearHUD();
    }

    private IEnumerator UploadTrialToServer()
    {
        isUploading = true;
        uploadStatusMessage = "Preparing upload...";
        
        var trialData = new TrialJson
        {
            displayName = trialName,
            trialId = trialName.ToLower().Replace(" ", ""),
            position = trialStandPosition,
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
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Authorization", Plugin.APIKey.Value);

            uploadStatusMessage = "Uploading to server...";
            
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                if (www.responseCode == 401)
                {
                    uploadStatusMessage = "Error: Unauthorized. Check your API key.";
                    Logging.Error("Upload failed: Unauthorized (401)");
                }
                else if (www.responseCode == 429)
                {
                    uploadStatusMessage = "Error: You can only upload one trial per 10 minutes.";
                    Logging.Error("Upload failed: Rate limited (429)");
                }
                else if (www.responseCode == 400)
                {
                    uploadStatusMessage = "Error: Invalid trial data.";
                    Logging.Error("Upload failed: Bad request (400)");
                }
                else if (www.responseCode == 500)
                {
                    uploadStatusMessage = "Error: Server error. Please try again later.";
                    Logging.Error("Upload failed: Server error (500)");
                }
                else
                {
                    uploadStatusMessage = $"Error: {www.error}";
                    Logging.Error($"Upload failed: {www.error}");
                }
                
                isUploading = false;
            }
            else
            {
                try
                {
                    var response = JsonUtility.FromJson<UploadResponse>(www.downloadHandler.text);
                    uploadStatusMessage = $"Success! Trial uploaded with ID: {response.trialId}";
                    Logging.Info($"Trial uploaded successfully! Trial ID: {response.trialId}");
                    
                    if (HUDManager.instance != null)
                    {
                        HUDManager.instance.SetHUDText($"Trial '{trialName}' uploaded successfully!");
                        StartCoroutine(ClearHUDDelayed(5f));
                    }
                }
                catch (Exception ex)
                {
                    uploadStatusMessage = "Upload succeeded but failed to parse response.";
                    Logging.Error($"Failed to parse upload response: {ex.Message}");
                }
                
                yield return new WaitForSeconds(3f);
                showUploadDialog = false;
                uploadStatusMessage = "";
                isUploading = false;
            }
        }
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
    }
}

