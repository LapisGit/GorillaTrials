using TMPro;
using UnityEngine;
using System;
using System.Collections.Generic;
using GorillaTrials.Behaviours.UI;
using GorillaTrials.Tools;

namespace GorillaTrials.Behaviours
{
    public class TrialKeyboard : MonoBehaviour
    {
        public static TrialKeyboard instance;
        public TextMeshProUGUI displayText;
        public Action<string> onSubmit;
        public Action onCancel;
        public GameObject keyboard, keyboardPrefab;

        private string currentText = "";
        public int maxLength = 500;
        private bool shiftActive = false;
        public bool forUsername;
        private Dictionary<Transform, (string primary, string shifted)> buttonTexts = new();

        public void Start()
        {
            Initialize();
            instance = this;
        }

        public async void Initialize()
        {
            keyboardPrefab = await AssetLoader.LoadAsset<GameObject>("Keyboard");
            keyboard = Instantiate(keyboardPrefab);
            keyboard.transform.rotation = Quaternion.Euler(30.8838f, 241.1243f, -0.0001f);
            keyboard.transform.position = new Vector3(-68.9272f, 11.6256f, -83.9839f);
            keyboard.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            keyboard.SetActive(false);
            DontDestroyOnLoad(keyboard);
            displayText = keyboard.transform.Find("Canvas/PreviewPanel/Text").GetComponent<TextMeshProUGUI>();

            SetupKeyboardButtons();
        }

        private void SetupKeyboardButtons()
        {
            string[] letters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
                                 "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

            foreach (string letter in letters)
            {
                Transform btn = keyboard.transform.Find($"Canvas/KeyboardPanel/Keys/{letter}");
                if (btn != null)
                {
                    TrialButton trialBtn = btn.GetComponent<TrialButton>() ?? btn.gameObject.AddComponent<TrialButton>();
                    string capturedLetter = letter;
                    trialBtn.onPressed = () => AddCharacter(shiftActive ? capturedLetter : capturedLetter.ToLower());
                    
                    buttonTexts[btn] = (capturedLetter.ToLower(), capturedLetter);
                    UpdateButtonText(btn, capturedLetter.ToLower());
                }
            }

            var numberKeys = new[]
            {
                ("One-ExclamationMark", "1", "!"),
                ("Two-AtSymbol", "2", "@"),
                ("Three-Hashtag", "3", "#"),
                ("Four-DollarSign", "4", "$"),
                ("Five-Percent", "5", "%"),
                ("Six-Carrot", "6", "^"),
                ("Seven-AndSymbol", "7", "&"),
                ("Eight-Star", "8", "*"),
                ("Nine-LeftParentheses", "9", "("),
                ("Zero-RightParentheses", "0", ")")
            };

            foreach (var (keyName, number, special) in numberKeys)
            {
                Transform btn = keyboard.transform.Find($"Canvas/KeyboardPanel/Keys/{keyName}");
                if (btn != null)
                {
                    TrialButton trialBtn = btn.GetComponent<TrialButton>() ?? btn.gameObject.AddComponent<TrialButton>();
                    string primary = number;
                    string shifted = special;
                    trialBtn.onPressed = () => AddCharacter(shiftActive ? shifted : primary);
                    
                    buttonTexts[btn] = (primary, shifted);
                    UpdateButtonText(btn, primary);
                }
            }

            var symbolKeys = new[]
            {
                ("Minus-Underscore", "-", "_"),
                ("Equals-Plus", "=", "+"),
                ("RightBracket-CurlyBracket", "]", "}"),
                ("LeftBracket-CurlyBracket", "[", "{"),
                ("Colon-Semicolon", ";", ":"),
                ("Quote", "'", "\""),
                ("Comma-LeftArrow", ",", "<"),
                ("Period-RightArrow", ".", ">"),
                ("ForwardSlash-QuestionMark", "/", "?")
            };

            foreach (var (keyName, primary, special) in symbolKeys)
            {
                Transform btn = keyboard.transform.Find($"Canvas/KeyboardPanel/Keys/{keyName}");
                if (btn != null)
                {
                    TrialButton trialBtn = btn.GetComponent<TrialButton>() ?? btn.gameObject.AddComponent<TrialButton>();
                    string primaryChar = primary;
                    string specialChar = special;
                    trialBtn.onPressed = () => AddCharacter(shiftActive ? specialChar : primaryChar);
                    
                    buttonTexts[btn] = (primaryChar, specialChar);
                    UpdateButtonText(btn, primaryChar);
                }
            }

            Transform shift = keyboard.transform.Find("Canvas/KeyboardPanel/BigKeys/Shift");
            if (shift != null)
            {
                TrialButton trialBtn = shift.GetComponent<TrialButton>() ?? shift.gameObject.AddComponent<TrialButton>();
                trialBtn.onPressed = () => ToggleShift();
            }
            
            Transform backspace = keyboard.transform.Find("Canvas/KeyboardPanel/BigKeys/Delete");
            if (backspace != null)
            {
                TrialButton trialBtn = backspace.GetComponent<TrialButton>() ?? backspace.gameObject.AddComponent<TrialButton>();
                trialBtn.onPressed = () => Backspace();
            }

            Transform space = keyboard.transform.Find("Canvas/KeyboardPanel/BigKeys/Space");
            if (space != null)
            {
                TrialButton trialBtn = space.GetComponent<TrialButton>() ?? space.gameObject.AddComponent<TrialButton>();
                trialBtn.onPressed = () => AddCharacter(" ");
            }

            Transform submit = keyboard.transform.Find("Canvas/KeyboardPanel/BigKeys/Enter");
            if (submit != null)
            {
                TrialButton trialBtn = submit.GetComponent<TrialButton>() ?? submit.gameObject.AddComponent<TrialButton>();
                trialBtn.onPressed = () => Submit();
            }

            Transform cancel = keyboard.transform.Find("Canvas/KeyboardPanel/BigKeys/Return");
            if (cancel != null)
            {
                TrialButton trialBtn = cancel.GetComponent<TrialButton>() ?? cancel.gameObject.AddComponent<TrialButton>();
                trialBtn.onPressed = () => Cancel();
            }
        }

        private void ToggleShift()
        {
            shiftActive = !shiftActive;
            UpdateAllButtonTexts();
        }

        private void UpdateAllButtonTexts()
        {
            foreach (var kvp in buttonTexts)
            {
                string textToShow = shiftActive ? kvp.Value.shifted : kvp.Value.primary;
                UpdateButtonText(kvp.Key, textToShow);
            }
        }

        private void UpdateButtonText(Transform button, string text)
        {
            TextMeshProUGUI textComponent = button.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = text;
            }
        }

        public void SetMaxLength(int length)
        {
            maxLength = length;
        }

        public void AddCharacter(string character)
        {
            if (currentText.Length < maxLength)
            {
                currentText += character;
                UpdateDisplay();
            }
        }

        public void Backspace()
        {
            if (forUsername)
            {
                if (currentText.Length > 1)
                {
                    currentText = currentText.Substring(0, currentText.Length - 1);
                    UpdateDisplay();
                }
                return;
            }
            if (currentText.Length > 0)
            {
                currentText = currentText.Substring(0, currentText.Length - 1);
                UpdateDisplay();
            }
        }

        public void Clear()
        {
            currentText = "";
            UpdateDisplay();
        }

        public void SetText(string text)
        {
            currentText = text ?? "";
            UpdateDisplay();
        }

        public void Submit()
        {
            onSubmit?.Invoke(currentText);
            Clear();
        }

        public void Cancel()
        {
            onCancel?.Invoke();
            Clear();
        }

        private void UpdateDisplay()
        {
            if (displayText != null)
            {
                displayText.text = currentText;
            }
        }
    }
}
