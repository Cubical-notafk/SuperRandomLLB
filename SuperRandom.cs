using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameplayEntities;
using HarmonyLib;
using LLBML.GameEvents;
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
        public static List<Character>[] randomCharacters = new List<Character>[4];

        public GameObject characterButton;

        public static List<PlayerEntity>[] playerEntities = new List<PlayerEntity>[4];

        public void Awake()
        {

            Logger = base.Logger;
            Instance = this;
            ConfigInit();
            harmony.PatchAll(typeof(AddPlayersToWorldPatch));
            LobbyEvents.OnStageSelectOpen += (o, a) =>
            {
                if (sRToggled == true)
                {

                    SetNumberOfStocks();
                    Player.ForAllInMatch((Player player) =>
                    {
                        if (player.nr == -1)
                        {
                            player.nr = 0;
                        }
                        randomCharacters[player.nr] = GetRandomCharacters(player.nr);
                    });
                    Logger.LogInfo("randomCharacters has been set");

                }

            };
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


        public void FixedUpdate()
        {
            Player.ForAllInMatch((Player player) =>
            {
                if (playerEntities[player.nr] != null)
                {

                    if (GameStates.IsInMatch() && Input.GetKeyDown(KeyCode.Q))
                    {
                        Logger.LogInfo("Swapping character");
                        PlayerEntity oldEntity = player.playerEntity;

                        var newPlayerEntity =
                            playerEntities[player.nr][ControlledRandom.Get(0, 0, playerEntities[player.nr].Count)];

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
            

            if (GameStates.IsInLobby())
            {
                if (!uiCreated)
                {
                    sP ??= FindObjectOfType<ScreenPlayers>(); 

                    if (sP != null)
                    {
                        CreateUI();
                        uiCreated = true;

                        ApplyUIState(); 

                        if (sRToggled)
                        {
                            EnableSuperRandomMode();
                            
                        }
                    }
                }
            }
            else if (uiCreated)
            {
                DestroyUI();
            }
        }
        private void ApplyUIState()
        {
            if (allowRepeatsButton != null)
            {
                allowRepeatsButton.SetText(allowRepeats ? "Allow Repeats: ON" : "Allow Repeats: OFF");
            }
        }

        private void EnableSuperRandomMode()
        {

            if (superRandomButton != null)
            {
                superRandomButton.SetText("Super Random !!!");
            }

            if (emptyParent == null)
            {
                emptyParent = new GameObject("SuperRandomParent", typeof(RectTransform));
               
            }

            var parent = sP.btOptions.transform.parent;
            emptyParent.transform.SetParent(parent.transform);
            emptyParent.SetActive(true);
            emptyParent.transform.localScale = Vector3.one;

            
            Player.ForAll((Player player) =>
            {
                Debug.Log($"OverlayButtonsON called for Player {player.nr}");
                OverlayButtonsON(player.nr);
                GameStates.Send(Msg.SEL_CHAR, player.nr, (int)Character.RANDOM);
            });

            

            if (resetWeightButton.Value)
            {
                ResetWeights();
                resetWeightButton.Value = false;
            }
        }


        #region UIMaker
        public void CreateUI()
        {
            if (superRandomButton == null || !superRandomButton.gameObject.activeInHierarchy)
            {

                if (superRandomButton != null)
                {
                    Destroy(superRandomButton.gameObject);
                }



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


        }


        private void DestroyUI()
        {
            if (superRandomButton != null)
            {
                Destroy(superRandomButton.gameObject);
                superRandomButton = null;
            }

            if (allowRepeatsButton != null)
            {
                Destroy(allowRepeatsButton.gameObject);
                allowRepeatsButton = null;
            }

            sP = null;
            uiCreated = false;
        }
        #endregion



        private bool sRToggled = false;

        public void HandleSuperRandomButtonClick(int playerNr)
        {
            if (playerNr == -1)
            {
                playerNr = 0;
            }
            Logger.LogInfo($"Player {playerNr} clicked");

            sRToggled = !sRToggled;


            if (sRToggled)
            {
                if (emptyParent == null)
                {
                    emptyParent = new GameObject();

                }
                var parent = sP.btOptions.transform.parent;
                emptyParent.transform.SetParent(parent.transform);
                emptyParent.SetActive(true);
                emptyParent.transform.localScale = Vector3.one;
                superRandomButton.SetText("Super Random !!!");
                Debug.Log("Super Random is ON");
                OverlayButtonsON(playerNr);

                GameStates.Send(Msg.SEL_CHAR, playerNr, (int)Character.RANDOM);

            }
            else
            {
                emptyParent.SetActive(false);
                superRandomButton.SetText("Super Random ???");
                Debug.Log("Super Random is OFF");
                OverlayButtonsOFF();
            }
        }
        public GameObject emptyParent;

        public void OverlayButtonsON(int playerNr)
        {
            Debug.Log($"OverlayButtonsON called for Player {playerNr}");
            if (playerNr == -1)
            {
                playerNr = 0;
            }
            Player player = Player.GetPlayer(playerNr);
            Character[] orderedCharacters = new Character[]
            {
            Character.GRAF,
            Character.ELECTRO,
            Character.PONG,
            Character.CROC,
            Character.BOOM,
            Character.ROBOT,
            player.GetRandomCharacter(),
            Character.KID,
            Character.CANDY,
            Character.SKATE,
            Character.BOSS,
            Character.COP,
            Character.BAG

            };

            List<Character> _characters = new List<Character>
            {
            Character.GRAF,
            Character.ELECTRO,
            Character.PONG,
            Character.CROC,
            Character.BOOM,
            Character.ROBOT,
            Character.RANDOM,
            Character.KID,
            Character.CANDY,
            Character.SKATE,
            Character.BOSS,
            Character.COP,
            Character.BAG
            };

            float buttonOffset = 64f;
            float leftShift = -384.5f;
            for (int i = 0; i < orderedCharacters.Length; i++)
            {
                Character character = orderedCharacters[i];



                characterButton = Instantiate(sP.pfCharacterButton, emptyParent.transform);
                characterButton.transform.localPosition = new Vector3(i * buttonOffset + leftShift, -255f, 0f);
                characterButton.name = "CharacterButton_" + character;

                PlayersCharacterButton component = characterButton.GetComponent<PlayersCharacterButton>();

                component.Init(_characters[i]);
                Image buttonImage = component.imCharacter;

                Image bgImage = component.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();


                buttonImage.color = Color.Lerp(Color.white, Color.black, 0.75f);
                bgImage.color = Color.red;

                component.btCharacter.onClick = new LLClickable.ControlDelegate((pnr) => HandleOverlay(playerNr, character, component));

                if (addedCharacters[playerNr].Contains(character))
                {
                    buttonImage.color = Color.white;
                    bgImage.color = Color.green;
                }

            }
        }

        public void OverlayButtonsOFF()
        {
            Debug.Log("Hiding all character buttons.");
            foreach (Transform child in sP.btOptions.transform.parent)
            {

                if (child.name.StartsWith("CharacterButton"))
                {
                    Debug.Log($"Hiding {child.name}");
                    child.gameObject.SetActive(false);
                }
            }
        }

        public static List<Character>[] addedCharacters = new List<Character>[4]
        {
            new List<Character>(),
            new List<Character>(),
            new List<Character>(),
            new List<Character>()
        };
        private Dictionary<Character, float> characterWeights = new Dictionary<Character, float>();


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
                if (GameSettings.current.points >= 12)
                {
                    numberOfStocks = 12;
                }
                else if (GameSettings.current.points <= 12)
                {
                    numberOfStocks = GameSettings.current.points;
                }
            }




        }
        public void SetAllowRepeats(int playerNr)
        {
            allowRepeats = !allowRepeats;
            if (allowRepeats)
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

        public void HandleOverlay(int playerNr, Character character, PlayersCharacterButton component)
        {
            if (playerNr == -1)
            {
                playerNr = 0;
            }
            Image buttonImage = component.imCharacter;
            Image bgImage = component.gameObject.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();

            if (!addedCharacters[playerNr].Contains(character))
            {
                addedCharacters[playerNr].Add(character);
                characterWeights[character] = 1f;
                buttonImage.color = Color.white;
                bgImage.color = Color.green;

            }
            else
            {
                addedCharacters[playerNr].Remove(character);
                characterWeights.Remove(character);
                buttonImage.color = Color.Lerp(Color.white, Color.black, 0.75f);
                bgImage.color = Color.red;
            }



        }

        public List<Character> GetRandomCharacters(int playerNr)
        {
            return GetRandomCharacters(numberOfStocks, allowRepeats, useKarmicRandom.Value, playerNr);
        }

        public List<Character> GetRandomCharacters(int count, bool allowRepeats, bool useKarmicRandom, int playerNR)
        {
            bool originalAllowRepeats = allowRepeats;
            if (addedCharacters[playerNR].Count == 0)
                return new List<Character>();

            if (!allowRepeats && addedCharacters[playerNR].Count < count)
            {
                allowRepeats = true;
            }
            List<Character> selectedCharacters = new List<Character>();

            if (useKarmicRandom)
            {
                Dictionary<Character, float> availableCharacters = new Dictionary<Character, float>(characterWeights);

                while (selectedCharacters.Count < count)
                {
                    if (availableCharacters.Count == 0)
                        break;  // Prevents re-adding characters when allowRepeats is false

                    Character chosenCharacter = WeightedRandomSelection(availableCharacters);
                    selectedCharacters.Add(chosenCharacter);

                    if (!allowRepeats)
                    {
                        availableCharacters.Remove(chosenCharacter);
                    }

                    if (allowRepeats) // Only reduce weight if repeats are allowed
                    {
                        ReduceWeight(chosenCharacter);
                    }
                }
            }
            else
            {
                List<Character> tempList = new List<Character>(addedCharacters[playerNR]);

                while (selectedCharacters.Count < count)
                {
                    if (tempList.Count == 0)
                        break;  // Prevents re-adding characters when allowRepeats is false

                    int randomIndex = UnityEngine.Random.Range(0, tempList.Count);
                    selectedCharacters.Add(tempList[randomIndex]);

                    if (!allowRepeats)
                        tempList.RemoveAt(randomIndex);
                }
            }

            Debug.Log("Selected characters: ");
            foreach (var character in selectedCharacters)
            {
                Debug.Log("Character: " + character);
            }

            allowRepeats = originalAllowRepeats;
            return selectedCharacters;
        }


        private Character WeightedRandomSelection(Dictionary<Character, float> availableCharacters)
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

        private void ReduceWeight(Character character)
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
