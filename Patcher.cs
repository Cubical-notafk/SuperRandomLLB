using GameplayEntities;
using HarmonyLib;
using LLBML.Players;
using LLBML.Utils;
using LLHandlers;
using LLScreen;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRandom
{

    public static class AddPlayersToWorldPatch
    {


        [HarmonyPatch(typeof(PlayerHandler), nameof(PlayerHandler.AddPlayersToWorld))]
        [HarmonyPostfix]
        public static void AddPlayersToWorld_Postfix(PlayerHandler __instance)
        {
            SuperRandom.SetNumberOfStocks();
            var randomCharacters = SuperRandom.Instance.GetRandomCharacters();
            Player player = Player.GetPlayer(0);


            if (player.IsInMatch)
            {
                var newEntities = new List<PlayerEntity>()
                {

                };

                foreach (var character in randomCharacters)
                {
                    PlayerEntity extraCharacter = CreateExtraEntity(__instance, player, character);
                    extraCharacter.SetPlayerState(PlayerState.STANDBY);
                    __instance.world.AddEntity(extraCharacter);
                    newEntities.Add(extraCharacter);
                }
                SuperRandom.playerEntities[player.nr] = newEntities;

                PlayerEntity playerEntity = player.playerEntity;
                SetFirstCharacter(playerEntity);



            }

        }
        private static void SetFirstCharacter(PlayerEntity __0)
        {

            Player player = __0.player;
            int playerNr = player.nr;

            var firstInList = SuperRandom.playerEntities[player.nr][0];

            player.Character = firstInList.character;

            player.playerEntity = firstInList;
            PlayerHandler.instance.playerHandlerData.playerData[playerNr] = firstInList.playerData;

            SuperRandom.Logger.LogInfo("Set first player");


        }




        [HarmonyPatch(typeof(PlayerEntity), "SetPlayerState")]
        [HarmonyPostfix]
        public static void Postfix(PlayerEntity __instance, PlayerState __0)
        {
            if (__0 == PlayerState.DEAD && !JOMBNFKIHIC.GIGAKBJGFDI.KOBEJOIALMO)
            {
                ResetNextCharacter(__instance);
                ResetCorpse(__instance);
                ResetHud(__instance);
            }
            else if (__0 == PlayerState.DEAD && JOMBNFKIHIC.GIGAKBJGFDI.KOBEJOIALMO == true)
            {
                ResetNextCharacterPointsVersion(__instance);
                ResetCorpse(__instance);
                ResetHud(__instance);
            }
        }
        private static void ResetHud(PlayerEntity __instance)
        {
            var playerInfo = ScreenGameHud.instance.playerInfos[__instance.playerIndex];
            int count = playerInfo.transform.childCount;
            GameObject.Destroy(playerInfo.transform.GetChild(count - 1).gameObject);
            playerInfo.SetPlayer(__instance.player, 12);
        }

        private static void ResetNextCharacter(PlayerEntity currentEntity)
        {

            SuperRandom.Logger.LogInfo("ResetNextCharacter starting");

            Player player = currentEntity.player;

            int playerNr = player.nr;

            SuperRandom.Logger.LogInfo($"Player +{player.nr} has died");

            if (SuperRandom.playerEntities[player.nr] == null)
            {
                return;
            }

            var entityList = SuperRandom.playerEntities[playerNr];

            var newPlayerEntity = entityList[1];

            newPlayerEntity.playerData.stocks = currentEntity.playerData.stocks;


            player.Character = newPlayerEntity.character;
            player.playerEntity = newPlayerEntity;

            PlayerHandler.instance.playerHandlerData.playerData[playerNr] = newPlayerEntity.playerData;

            entityList.RemoveAt(1);

        }
        private static void ResetNextCharacterPointsVersion(PlayerEntity currentEntity)
        {

            SuperRandom.Logger.LogInfo("ResetNextCharacter starting");

            Player player = currentEntity.player;

            int playerNr = player.nr;

            SuperRandom.Logger.LogInfo($"Player +{player.nr} has died");

            if (SuperRandom.playerEntities[player.nr] == null)
            {
                return;
            }

            var entityList = SuperRandom.playerEntities[playerNr];

            var newPlayerEntity = SuperRandom.playerEntities[player.nr][ControlledRandom.Get(0, 0, SuperRandom.playerEntities[player.nr].Count)];

            newPlayerEntity.playerData.stocks = currentEntity.playerData.stocks;


            player.Character = newPlayerEntity.character;
            player.playerEntity = newPlayerEntity;

            PlayerHandler.instance.playerHandlerData.playerData[playerNr] = newPlayerEntity.playerData;



        }

        private static void ResetCorpse(PlayerEntity player)
        {
            CorpseEntity corpse = World.instance.itemHandler.corpseItems[player.playerIndex];
            corpse.visualTable.Remove("main");
            GameObject.Destroy(corpse.transform.GetChild(0).gameObject);
            corpse.SetCharacter(player.character, player.variant);

        }



        private static PlayerEntity CreateExtraEntity(PlayerHandler instance, Player player, int character)
        {

            Character characters = IndextoCharacter(player, character);

            PlayerEntity playerEntity = CharacterToModel(characters, player.nr);





            playerEntity.character = characters;
            playerEntity.variant = player.variant;
            playerEntity.player = player;
            playerEntity.playerIndex = player.nr;
            instance.playerHandlerData.playerData[player.nr].team = player.Team;

            foreach (var ball in BallHandler.instance.balls)
            {

                if (playerEntity.character == Character.CANDY)
                {
                    AddCandyAnims(ball);
                }
                if (playerEntity.character == Character.SKATE)
                {

                    AddJetBubble(ball);
                }
            }

            var newPlayerData = new PlayerData(6);
            newPlayerData.Load(instance.playerHandlerData.playerData[player.nr]);
            playerEntity.Init(newPlayerData);

            playerEntity.tf.parent = instance.holder;





            SuperRandom.Logger.LogDebug($"[Patch] Created and disabled extra entity: {player.name}");

            playerEntity.UpdateUnityTransform();

            return playerEntity;
        }
        public static void AddCandyAnims(BallEntity ball)
        {
            HashSet<CharacterVariant> candies = new HashSet<CharacterVariant>();

            ALDOKEMAOMB.ICOCPAFKCCE(delegate (ALDOKEMAOMB player)
            {
                if (player.LALEEFJMMLH == Character.CANDY)
                {
                    candies.Add(player.AIINAIDBHJI);
                }

                ball.candyballs.Clear();
                ball.hatTf = new Transform[candies.Count];
                int num = 0;
                foreach (CharacterVariant characterVariant in candies)
                {
                    string text = "candyBall" + (int)characterVariant + "Visual";
                    AOOJOMIECLD aoojomiecld = JPLELOFJOOH.NEBGBODHHCG(Character.CANDY, characterVariant);
                    DLC dlc = EPCDKLCABNC.LEMKFOAAMKA(Character.CANDY, characterVariant);
                    string text2 = aoojomiecld.KGFMPDNFIEC()[0];
                    AOOJOMIECLD modelValues;
                    if (dlc == DLC.CANDY_SATURN)
                    {
                        modelValues = AOOJOMIECLD.HCFBCKBLLAH(dlc, "candyBallSaturn", new string[]
                        {
                        text2
                        }, 1f, 0, FKBHNEMDBMK.NMJDMHNMDNJ);
                    }
                    else if (characterVariant == CharacterVariant.MODEL_ALT || characterVariant == CharacterVariant.MODEL_ALT2)
                    {
                        modelValues = AOOJOMIECLD.HCFBCKBLLAH(Character.CANDY, "candyBallStrait", new string[]
                        {
                        text2
                        }, 1f, 0, FKBHNEMDBMK.NMJDMHNMDNJ);
                    }
                    else
                    {
                        modelValues = AOOJOMIECLD.HCFBCKBLLAH(Character.CANDY, "candyBall", new string[]
                        {
                        text2
                        }, 1f, 0, FKBHNEMDBMK.NMJDMHNMDNJ);
                    }
                    if (dlc != DLC.NONE)
                    {
                        modelValues.EDKLFODCINA = new Bundle(dlc);
                    }
                    ball.SetVisualModel(text, modelValues, true, true);
                    ball.GetVisual(text).flipMode = FlipMode.NOT_AUTO;
                    ball.candyballs.Add(text);
                    Transform transform = ball.GetVisual(text).gameObject.transform;
                    ball.hatTf[num] = transform.Find("centerhead/hat001");
                    num++;
                }
            });
        }
        public static void AddJetBubble(BallEntity ball)
        {
            ball.SetVisualSprite("bubbleVisual", "bubble", false, false, new JKMAAHELEMF(256, 256), default(JKMAAHELEMF), 0f, true, 0f, Layer.GAMEPLAY, default(Color32));
            ball.SetScale(0.7f, "bubbleVisual");
            ball.GetVisual("bubbleVisual").flipMode = FlipMode.NOT_AUTO;
            ball.GetVisual("bubbleVisual").SetAllMaterialsRenderQueue(1998);
        }

        private static Character IndextoCharacter(Player player, int index)
        {

            switch (index)
            {
                case 0:


                    return Character.GRAF;

                case 1:

                    return Character.ELECTRO;

                case 2:

                    return Character.PONG;

                case 3:

                    return Character.CROC;

                case 4:

                    return Character.BOOM;

                case 5:

                    return Character.ROBOT;

                case 6:

                    return player.GetRandomCharacter();

                case 7:

                    return Character.KID;

                case 8:

                    return Character.CANDY;

                case 9:

                    return Character.SKATE;

                case 10:

                    return Character.BOSS;

                case 11:

                    return Character.COP;

                case 12:

                    return Character.BAG;

                default:

                    return Character.KID;

            }
        }

        private static PlayerEntity CharacterToModel(Character character, int playerIndex)
        {
            GameObject gameObject = new GameObject();
            PlayerEntity playerEntity;
            gameObject.name = $"Extra_{character}_{playerIndex}";
            switch (character)
            {
                case Character.KID:
                    return playerEntity = gameObject.AddComponent<KidPlayerModel>();

                case Character.ROBOT:
                    return playerEntity = gameObject.AddComponent<RobotPlayerModel>();

                case Character.CANDY:
                    return playerEntity = gameObject.AddComponent<CandyPlayerModel>();

                case Character.BOOM:
                    return playerEntity = gameObject.AddComponent<BoomPlayerModel>();

                case Character.CROC:
                    return playerEntity = gameObject.AddComponent<CrocPlayerModel>();

                case Character.PONG:
                    return playerEntity = gameObject.AddComponent<PongPlayerModel>();

                case Character.BOSS:
                    return playerEntity = gameObject.AddComponent<BossPlayerModel>();

                case Character.COP:
                    return playerEntity = gameObject.AddComponent<CopPlayerModel>();

                case Character.ELECTRO:
                    return playerEntity = gameObject.AddComponent<ElectroPlayerModel>();

                case Character.SKATE:
                    return playerEntity = gameObject.AddComponent<SkatePlayerModel>();

                case Character.GRAF:
                    return playerEntity = gameObject.AddComponent<GrafPlayerModel>();

                case Character.BAG:
                    return playerEntity = gameObject.AddComponent<BagPlayerModel>();

                default:
                    return playerEntity = gameObject.AddComponent<KidPlayerModel>();

            }

        }
    }
}




