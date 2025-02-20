using BepInEx.Logging;
using GameplayEntities;
using HarmonyLib;
using LLBML.Players;
using LLBML.Utils;
using LLHandlers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperRandomLLB
{
    public static class AddPlayersToWorldPatch
    {
        private static ManualLogSource Logger => SuperRandom.SuperRandom.Logger;


        [HarmonyPatch(typeof(PlayerHandler), nameof(PlayerHandler.AddPlayersToWorld))]
        [HarmonyPostfix]
        public static void AddPlayersToWorld_Postfix(PlayerHandler __instance)
        {
            for (int i = 3; i >= 0; i--)
            {
                Player player = Player.GetPlayer(i);
                if (player.IsInMatch)
                {
                    Logger.LogDebug($"[Patch] Checking Player {player.nr}");

                    //player.playerEntity.SetPlayerState(PlayerState.STANDBY, string.Empty, HitPauseState.NONE, HitstunState.NONE);
                    //SuperRandom.Logger.LogDebug($"[Patch] Ensured main entity for Player {player.nr} is enabled.");


                    PlayerEntity extraEntity1 = CreateExtraEntity(__instance, player, Character.COP);
                    extraEntity1.SetPlayerState(PlayerState.DEAD);
                    PlayerEntity extraEntity2 = CreateExtraEntity(__instance, player, Character.ROBOT);
                    extraEntity2.SetPlayerState(PlayerState.DEAD);

                    __instance.world.AddEntity(extraEntity1);
                    __instance.world.AddEntity(extraEntity2);

                    var newEntities = new List<PlayerEntity>()
                    {
                        player.playerEntity,
                        extraEntity1,
                        extraEntity2
                    };

                    SuperRandom.SuperRandom.playerEntities[player.nr] = newEntities;

                    SuperRandom.SuperRandom.Logger.LogDebug($"[Patch] Added extra entities for Player {i}");
                }
            }
        }


        [HarmonyPatch(typeof(GetHitPlayerEntity), nameof(GetHitPlayerEntity.GetKilled))]
        [HarmonyPostfix]
        public static void GetKilled_PostFix(PlayerEntity __instance)
        {
            /*Player player = __instance.player;

            Logger.LogInfo($"Player {player.nr} died");

            PlayerEntity oldEntity = player.playerEntity;

            var newPlayerEntity =
                SuperRandom.SuperRandom.playerEntities[player.nr][ControlledRandom.Get(0, 0, SuperRandom.SuperRandom.playerEntities[player.nr].Count)];

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
            }
        }

        private static void ResetNextCharacter(PlayerEntity currentEntity)
        {
            try
            {
                SuperRandom.SuperRandom.Logger.LogInfo($"ResetNextCharacter called for entity: {currentEntity?.GetType().Name ?? "null"}");

                if (currentEntity == null)
                {
                    SuperRandom.SuperRandom.Logger.LogError("CurrentEntity is null");
                    return;
                }

                Player player = currentEntity.player;
                if (player == null)
                {
                    SuperRandom.SuperRandom.Logger.LogError("Player is null");
                    return;
                }

                int playerNr = player.nr;
                SuperRandom.SuperRandom.Logger.LogInfo($"Player number: {playerNr}");

                var entityList = SuperRandom.SuperRandom.playerEntities[playerNr];
                SuperRandom.SuperRandom.Logger.LogInfo($"Entity list count: {entityList.Count}");

                var newPlayerEntity =
                SuperRandom.SuperRandom.playerEntities[player.nr][ControlledRandom.Get(0, 0, SuperRandom.SuperRandom.playerEntities[player.nr].Count)];
                if (newPlayerEntity == null)
                {
                    SuperRandom.SuperRandom.Logger.LogError("Next entity is null");
                    return;
                }

                SuperRandom.SuperRandom.Logger.LogInfo($"Next entity type: {newPlayerEntity.GetType().Name}");

                
                newPlayerEntity.playerData.stocks = currentEntity.playerData.stocks;

                
                
                player.Character = newPlayerEntity.character;
                player.playerEntity = newPlayerEntity;
                PlayerHandler.instance.playerHandlerData.playerData[playerNr] = newPlayerEntity.playerData;

                SuperRandom.SuperRandom.Logger.LogInfo($"Reset values for next character of Player {playerNr}");
            }
            catch (Exception ex)
            {
                SuperRandom.SuperRandom.Logger.LogError($"Exception in ResetNextCharacter: {ex.Message}\n{ex.StackTrace}");
            }
        }



        private static PlayerEntity CreateExtraEntity(PlayerHandler instance, Player player, Character characterType)
        {
            GameObject gameObject = new GameObject();
            PlayerEntity playerEntity;

            switch (characterType)
            {
                case Character.COP:
                    playerEntity = gameObject.AddComponent<CopPlayerModel>();
                    break;
                case Character.ROBOT:
                    playerEntity = gameObject.AddComponent<RobotPlayerModel>();
                    break;
                default:
                    playerEntity = gameObject.AddComponent<KidPlayerModel>();
                    break;
            }

            playerEntity.character = characterType;
            playerEntity.variant = player.variant;
            playerEntity.player = player;
            playerEntity.playerIndex = player.nr;
            instance.playerHandlerData.playerData[player.nr].team = player.Team;



            var newPlayerData = new PlayerData(6);
            newPlayerData.Load(instance.playerHandlerData.playerData[player.nr]);
            playerEntity.Init(newPlayerData);

            playerEntity.tf.parent = instance.holder;
            gameObject.name = $"Extra_{characterType}_{playerEntity.playerIndex}";




            SuperRandom.SuperRandom.Logger.LogDebug($"[Patch] Created and disabled extra entity: {gameObject.name}");

            playerEntity.UpdateUnityTransform();

            return playerEntity;
        }
    }
}






