using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LLBML.Players;
using LLBML.States;
using LLBML.Utils;
using LLGUI;
using LLHandlers;
using LLScreen;
using SuperRandomLLB;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SuperRandom
{
    [BepInPlugin("us.Cubical.plugins.llb.SuperRandom", "SuperRandom", "1.0.0.0")]

    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]
    public class SuperRandom : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;
        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        public static SuperRandom Instance { get; private set; } = null;

        public ConfigEntry<bool> superRandomOn;
        public ConfigEntry<bool> allowRepeats;
        public ConfigEntry<bool> useKarmicRandom;
        public ConfigEntry<bool> resetWeightButton;

        public LLButton superRandomButton;


        public ScreenPlayers sP;

        public PlayersCharacterButton pCB;

        public LLButton characterButtons;

        public GameObject characterButton;

        public void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Instance = this;
            ConfigInit();
            harmony.PatchAll();
        }

        public void Start()
        {


            Logger.LogInfo("SuperRandom Started");

            ModDependenciesUtils.RegisterToModMenu(this.Info, new List<String> {
                "<b>Testing</b>:",
                "",
                "0 : <b>None</b>",
                "1 : <b>1</b>",
                "2 : <b>2</b>",
                "3 : <b>3</b>",
            });





        }
        void ConfigInit()
        {

            superRandomOn = Config.Bind<bool>("Toggles", "SuperRandomOn", true);
            allowRepeats = Config.Bind<bool>("Toggles", "AllowRepeats", true);
            useKarmicRandom = Config.Bind<bool>("Toggles", "UseKarmicRandom", true);
            resetWeightButton = Config.Bind<bool>("Toggles", "ResetKarmaWeights", false);

        }


        public void FixedUpdate()
        {

        }



        private bool uiCreated = false;

        public void Update()
        {
            if (GameStates.IsInLobby() && !uiCreated)
            {
                sP = FindObjectOfType<ScreenPlayers>();
            }


            if (GameStates.IsInLobby() && !uiCreated && sP != null)
            {
                CreateUI();
                uiCreated = true;

            }
            else if (!GameStates.IsInLobby() && uiCreated)
            {
                DestroyUI();
                sP = null;
                uiCreated = false;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                GetRandomCharacters();
            }

            if (resetWeightButton.Value == true)
            {
                ResetWeights();
                resetWeightButton.Value = false;
            }

            if (GameStates.IsInMatch() && Input.GetKeyDown(KeyCode.Q))
            {
               
            }

        }



        

        

        #region UIMaker
        public void CreateUI()
        {
            superRandomButton = null;



            superRandomButton = Instantiate(sP.btOptions, sP.btOptions.transform.parent);
            superRandomButton.name = "btSR";
            superRandomButton.SetText("Super Random ???");

            superRandomButton.onClick = new LLClickable.ControlDelegate(HandleSuperRandomButtonClick);


            RectTransform buttonRect = superRandomButton.GetComponent<RectTransform>();
            RectTransform referenceRect = sP.btOptions.GetComponent<RectTransform>();


            buttonRect.anchoredPosition = referenceRect.anchoredPosition + new Vector2(250, 0);

        }

        private void DestroyUI()
        {
            Destroy(superRandomButton);
            superRandomButton = null;
        }
        #endregion



        private bool sRToggled = false;

        public void HandleSuperRandomButtonClick(int playerNr)
        {


            sRToggled = !sRToggled;


            if (sRToggled)
            {
                superRandomButton.SetText("Super Random !!!");
                Debug.Log("Super Random is ON");
                OverlayButtonsON();
            }
            else
            {
                superRandomButton.SetText("Super Random ???");
                Debug.Log("Super Random is OFF");
                OverlayButtonsOFF();
            }
        }
        public void OverlayButtonsON()
        {
            float buttonOffset = 65f;
            float leftShift = -375f;
            for (int i = 0; i < 13; i++)
            {
                characterButton = Instantiate(sP.pfCharacterButton, sP.btOptions.transform.parent);

                characterButton.transform.localPosition = new Vector3(i * buttonOffset + leftShift, -200f, 0f);
                characterButton.name = "CharacterButton_" + i;



                PlayersCharacterButton component = characterButton.GetComponent<PlayersCharacterButton>();

                int buttonIndex = i;

                component.btCharacter.onClick = new LLClickable.ControlDelegate((playerNr) => HandleOverlay(playerNr, buttonIndex, component));
            }
        }

        public void OverlayButtonsOFF()
        {
            foreach (Transform child in sP.btOptions.transform.parent)
            {
                // Disable buttons by name
                if (child.name.StartsWith("CharacterButton"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private List<int> addedCharacters = new List<int>();
        private Dictionary<int, float> characterWeights = new Dictionary<int, float>();
        private bool addedCharacter;

        private const float minWeight = 0.1f;
        private const float maxWeight = 5f;
        private const float decayFactor = 0.5f;

        private int numberOfStocks = 5;



        public void SetNumberOfStocks(int newCount)
        {
            numberOfStocks = Mathf.Max(1, newCount);
        }
        public void SetAllowRepeats(bool value)
        {
            allowRepeats.Value = value;
        }
        public void SetUseKarmicRandom(bool value)
        {
            useKarmicRandom.Value = value;
        }

        public void HandleOverlay(int playerNr, int buttonIndex, PlayersCharacterButton component)
        {

            Image buttonImage = component.imCharacter;


            addedCharacter = !addedCharacter;

            if (addedCharacter)
            {

                if (!addedCharacters.Contains(buttonIndex))
                {
                    addedCharacters.Add(buttonIndex);
                    characterWeights[buttonIndex] = 1f;



                    buttonImage.color = Color.green;
                }
            }
            else
            {

                if (addedCharacters.Contains(buttonIndex))
                {
                    addedCharacters.Remove(buttonIndex);
                    characterWeights.Remove(buttonIndex);



                    buttonImage.color = Color.red;
                }
            }
        }
        public List<int> GetRandomCharacters()
        {
            return GetRandomCharacters(numberOfStocks, allowRepeats.Value, useKarmicRandom.Value);
        }


        public List<int> GetRandomCharacters(int count, bool allowRepeats, bool useKarmicRandom)
        {
            bool originalAllowRepeats = allowRepeats;
            if (addedCharacters.Count == 0)
                return new List<int>();

            if (!allowRepeats && addedCharacters.Count < count)
            {
                allowRepeats = true;
            }
            List<int> selectedCharacters = new List<int>();

            if (useKarmicRandom)
            {
                Dictionary<int, float> availableCharacters = new Dictionary<int, float>(characterWeights);

                while (selectedCharacters.Count < count)
                {
                    if (availableCharacters.Count == 0)
                        availableCharacters = new Dictionary<int, float>(characterWeights);

                    int chosenCharacter = WeightedRandomSelection(availableCharacters);
                    selectedCharacters.Add(chosenCharacter);

                    if (!allowRepeats)
                        availableCharacters.Remove(chosenCharacter);

                    ReduceWeight(chosenCharacter);
                }
            }
            else
            {
                List<int> tempList = new List<int>(addedCharacters);

                while (selectedCharacters.Count < count)
                {
                    if (tempList.Count == 0)
                        tempList = new List<int>(addedCharacters);

                    int randomIndex = UnityEngine.Random.Range(0, tempList.Count);
                    selectedCharacters.Add(tempList[randomIndex]);

                    if (!allowRepeats)
                        tempList.RemoveAt(randomIndex);
                }
            }


            Debug.Log("Selected characters: ");
            foreach (var character in selectedCharacters)
            {
                Debug.Log("Character ID: " + character);
            }

            foreach (var kvp in characterWeights)
            {
                Debug.Log($"Character ID: {kvp.Key}, Weight: {kvp.Value}");
            }

            allowRepeats = originalAllowRepeats;
            return selectedCharacters;
        }

        private int WeightedRandomSelection(Dictionary<int, float> availableCharacters)
        {
            float totalWeight = availableCharacters.Values.Sum();
            float roll = UnityEngine.Random.Range(0f, totalWeight);

            float cumulative = 0;
            foreach (var kvp in availableCharacters)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                    return kvp.Key;
            }

            return availableCharacters.Keys.First();
        }

        private void ReduceWeight(int character)
        {
            if (characterWeights.ContainsKey(character))
            {
                characterWeights[character] *= decayFactor;

                Debug.Log($"Character {character} weight reduced to: {characterWeights[character]}");

                if (characterWeights[character] < minWeight)
                    characterWeights[character] = minWeight;
            }
        }
        public void ResetWeights()
        {
            foreach (var key in characterWeights.Keys.ToList())
            {
                characterWeights[key] = 1f;
            }
            Debug.Log("Character weights have been reset to 1.");
        }
    }
}
