using GameplayEntities;
using HarmonyLib;
using JetBrains.Annotations;
using LLBML.Players;
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

            SuperRandom.playerEntities[player.nr].RemoveAt(0);
        }


        [HarmonyPatch(typeof(GetHitPlayerEntity), nameof(GetHitPlayerEntity.GetKilled))]
        [HarmonyPostfix]
        public static void GetKilled_PostFix(PlayerEntity __instance)
        {
            /*Player player = __instance.player;

            

            PlayerEntity oldEntity = player.playerEntity;

            var newPlayerEntity =
                SuperRandom.playerEntities[player.nr][ControlledRandom.Get(0, 0, SuperRandom.playerEntities[player.nr].Count)];

            newPlayerEntity.playerData.stocks = __instance.playerData.stocks;


            player.Character = newPlayerEntity.character;



            PlayerHandler.instance.playerHandlerData.playerData[player.nr] = newPlayerEntity.playerData;
            player.playerEntity = newPlayerEntity;*/


        }

        [HarmonyPatch(typeof(PlayerEntity), "SetPlayerState")]
        [HarmonyPostfix]
        public static void Postfix(PlayerEntity __instance, PlayerState __0)
        {
            if (__0 == PlayerState.DEAD)
            {
                ResetNextCharacter(__instance);
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

            var newPlayerEntity = entityList[0];

            newPlayerEntity.playerData.stocks = currentEntity.playerData.stocks;


            player.Character = newPlayerEntity.character;
            player.playerEntity = newPlayerEntity;

            PlayerHandler.instance.playerHandlerData.playerData[playerNr] = newPlayerEntity.playerData;

            entityList.RemoveAt(0);

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
                    AddCandyAnims();
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
        public static void AddCandyAnims()
        {

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






