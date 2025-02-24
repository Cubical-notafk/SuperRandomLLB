using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameplayEntities;
using HarmonyLib;
using LLBML.Players;
using LLBML.Settings;
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
using UnityEngine.Experimental.VFX;
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
        
        public ConfigEntry<bool> useKarmicRandom;
        public ConfigEntry<bool> resetWeightButton;

        public LLButton superRandomButton;

        public LLButton allowRepeatsButton;


        public ScreenPlayers sP;

        public PlayersCharacterButton pCB;
      

        public GameObject characterButton;

        public void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Instance = this;
            ConfigInit();
            harmony.PatchAll(typeof(AddPlayersToWorldPatch));
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
            
            useKarmicRandom = Config.Bind<bool>("Toggles", "UseKarmicRandom", true);
            resetWeightButton = Config.Bind<bool>("Toggles", "ResetKarmaWeights", false);

        }


        public static List<PlayerEntity>[] playerEntities = new List<PlayerEntity>[4];
        public void FixedUpdate()
        {
            Player.ForAllInMatch((Player player) => {
                if (playerEntities[player.nr] != null)
                {

                    if (GameStates.IsInMatch() && Input.GetKeyDown(KeyCode.Q))
                    {
                        Logger.LogInfo("Swapping character");
                        PlayerEntity oldEntity = player.playerEntity;
                        
                        var newPlayerEntity =
                            playerEntities[player.nr][ControlledRandom.Get(0, 0,playerEntities[player.nr].Count)];

                        player.playerEntity.SetPlayerState(PlayerState.DISABLED);

                        player.Character = newPlayerEntity.character;
                        

                        PlayerHandler.instance.playerHandlerData.playerData[player.nr] = newPlayerEntity.playerData;
                        player.playerEntity = newPlayerEntity;
                        player.playerEntity.Spawn();
                    }
                }
                
            });
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

                if (allowRepeats)
                {
                    allowRepeatsButton.SetText("Allow Repeats: ON");
                }
                else
                {
                    allowRepeatsButton.SetText("Allow Repeats: OFF");
                }
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
            Player.ForAllInMatch((Player player) => {
                if (playerEntities[player.nr] != null)
                {

                    if (GameStates.IsInMatch())
                    {
                        
                    }
                }

            });

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

            allowRepeatsButton = Instantiate(sP.btOptions, sP.btOptions.transform.parent);
            allowRepeatsButton.name = "btAR";
            allowRepeatsButton.SetText("Allow Repeats: OFF");

            allowRepeatsButton.onClick = new LLClickable.ControlDelegate(SetAllowRepeats);


            RectTransform buttonRect1 = allowRepeatsButton.GetComponent<RectTransform>();

            buttonRect1.anchoredPosition = referenceRect.anchoredPosition + new Vector2(830, 0);


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

                GameStates.Send(Msg.SEL_CHAR, playerNr, (int)Character.RANDOM);

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
            float buttonOffset = 64f;
            float leftShift = -384.5f;
            for (int i = 0; i < 13; i++)
            {
                characterButton = Instantiate(sP.pfCharacterButton, sP.btOptions.transform.parent);

                characterButton.transform.localPosition = new Vector3(i * buttonOffset + leftShift, -255f, 0f);
                
                characterButton.name = "CharacterButton_" + i;



                PlayersCharacterButton component = characterButton.GetComponent<PlayersCharacterButton>();

                int buttonIndex = i;

                Image buttonImage = component.imCharacter;

                buttonImage.color = Color.red;

                component.btCharacter.onClick = new LLClickable.ControlDelegate((playerNr) => HandleOverlay(playerNr, buttonIndex, component));

                if (addedCharacters.Contains(buttonIndex))
                {
                    buttonImage.color = Color.green;
                }
               
            }
        }

        public void OverlayButtonsOFF()
        {
            foreach (Transform child in sP.btOptions.transform.parent)
            {
                
                if (child.name.StartsWith("CharacterButton"))
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        public static List<int> addedCharacters = new List<int>();
        private Dictionary<int, float> characterWeights = new Dictionary<int, float>();
        

        private const float minWeight = 0.1f;
        private const float maxWeight = 5f;
        private const float decayFactor = 0.5f;
        public static bool allowRepeats;
        public static int numberOfStocks = 5;



        public static void SetNumberOfStocks()
        {

            if (!JOMBNFKIHIC.GIGAKBJGFDI.KOBEJOIALMO)
            {
                numberOfStocks = GameSettings.current.stocks;

            }
            else
            {
                if(GameSettings.current.points >= 13)
                {
                    numberOfStocks = 13;
                }
                else if(GameSettings.current.points <= 13)
                {
                    numberOfStocks = GameSettings.current.points;
                }
            }


            

        }
        public void SetAllowRepeats(int playerNr)
        {
            allowRepeats = !allowRepeats;
            if(allowRepeats)
            {
                allowRepeatsButton.SetText("Allow Repeats: ON");
            }
            else 
            {
                allowRepeatsButton.SetText("Allow Repeats: OFF");
            }
        }
        public void SetUseKarmicRandom(bool value)
        {
            useKarmicRandom.Value = value;
        }

        public void HandleOverlay(int playerNr, int buttonIndex, PlayersCharacterButton component)
        {

            Image buttonImage = component.imCharacter;


            if (!addedCharacters.Contains(buttonIndex))
            {
                addedCharacters.Add(buttonIndex);
                characterWeights[buttonIndex] = 1f;
                buttonImage.color = Color.green;
            }
            else 
            {
                addedCharacters.Remove(buttonIndex);
                characterWeights.Remove(buttonIndex);
                buttonImage.color = Color.red;
            }
            

            
        }

        public List<int> GetRandomCharacters()
        {
            return GetRandomCharacters(numberOfStocks, allowRepeats, useKarmicRandom.Value);
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
