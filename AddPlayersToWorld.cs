using GameplayEntities;
using HarmonyLib;
using LLBML.Players;
using LLBML.Utils;
using LLHandlers;
using LLScreen;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace SuperRandom
{
    [HarmonyPatch]
    public static class AddPlayersToWorldPatch
    {

        [HarmonyPatch(typeof(PlayersSelection), nameof(PlayersSelection.Init))]
        [HarmonyPostfix]
        private static void Init(int _playerNr)
        {
            SuperRandom.Instance.CreateUI();

            if (SuperRandom.sRToggled)
            {
                SuperRandom.Instance.EnableSuperRandomMode();

            }
        }


        [HarmonyPatch(typeof(ScreenPlayers), nameof(ScreenPlayers.OnOpen))]
        [HarmonyPrefix]
        private static void Prefix_OnOpen(ScreenPlayers __instance)
        {
            SuperRandom.sP = __instance;
            
        }
       

        private static void ResetHud(PlayerEntity __instance)
        {
            SuperRandom.Logger.LogInfo("Resetting Hud starting");
            var playerInfo = ScreenGameHud.instance.playerInfos[__instance.playerIndex];
            int count = playerInfo.transform.childCount;
            GameObject.Destroy(playerInfo.transform.GetChild(count - 1).gameObject);
            playerInfo.SetPlayer(__instance.player, 12);
        }

        public static PlayerEntity ResetNextCharacter(PlayerEntity currentEntity)
        {
            if (currentEntity == null)
            {
                SuperRandom.Logger.LogError("ResetNextCharacter: currentEntity is null!");
                return null;
            }


            SuperRandom.Logger.LogInfo("ResetNextCharacter starting");

            Player player = currentEntity.player;

            int playerNr = player.nr;

            SuperRandom.Logger.LogInfo($"Player {player.nr} has died");

            

            var handler = World.instance.playerHandler;
            if (SuperRandom.randomCharacters[player.nr] == null )
            {
                return currentEntity;
            }
            var entityList = SuperRandom.randomCharacters[player.nr];

            SuperRandom.Logger.LogInfo($"current death count : {currentEntity.playerData.deaths}");
            if (entityList == null || entityList.Count == 0)
            {
                SuperRandom.Logger.LogError($"ResetNextCharacter: entityList for player {player.nr} is null or empty!");
                return currentEntity;
            }
            
            if(player.Character == Character.BAG)
            {
                GameObject.Destroy(currentEntity.GetVisual("shadowVisual").gameObject);
            }
            if(player.Character == Character.GRAF)
            {
                GameObject.Destroy(currentEntity.GetVisual("pieceVisual").gameObject);
            }
            

            if (currentEntity.playerData.deaths >= SuperRandom.numberOfStocks)
            {
                return currentEntity;
            }
            int prevStocks = World.instance.playerHandler.playerHandlerData.playerData[player.playerEntity.playerIndex].stocks;

            var character = Character.NONE;


            if (JOMBNFKIHIC.GIGAKBJGFDI.KOBEJOIALMO && currentEntity.playerData.deaths > 12) //Points is True
            {
                 character = entityList[ControlledRandom.Get(0, 0, SuperRandom.randomCharacters[player.nr].Count)];
            }
            else 
            {
                 character = entityList[currentEntity.playerData.deaths];
            }

            player.Character = character;

            
            PlayerEntity newPlayerEntity = handler.CreatePlayerEntity(player);
            

            newPlayerEntity.playerData.deaths = currentEntity.playerData.deaths;


            SuperRandom.Logger.LogInfo($"new death count : {newPlayerEntity.playerData.deaths}");

            player.Character = newPlayerEntity.character;

            SuperRandom.Logger.LogInfo($"Setting {player.nr} to {player.Character}");

            newPlayerEntity.playerData.stocks = prevStocks;
            newPlayerEntity.entityID = player.playerEntity.entityID;

            PlayerHandler.instance.playerHandlerData.playerData[playerNr] = newPlayerEntity.playerData;

            World.instance.entityList[player.playerEntity.entityID -1] = newPlayerEntity;
            var nameofobject = player.playerEntity.gameObject.name;
            
            UnityEngine.Object.Destroy(player.playerEntity.gameObject);
            
            player.playerEntity = newPlayerEntity;

            
            ResetHud(currentEntity);

            return newPlayerEntity;
        }

        private static bool JetInList()
        {
            bool found = false;
            Player.ForAllInMatch((Player player) =>
            {
                if (SuperRandom.randomCharacters[player.nr] == null)
                {
                    return;
                }
                if (SuperRandom.randomCharacters[player.nr].Contains(Character.SKATE))
                {
                    found = true;
                }
            });

            return found;

        }

        private static bool CopDetectiveInList()
        {
            bool found = false;
            Player.ForAllInMatch((Player player) =>
            {
                if (SuperRandom.randomCharacters[player.nr] == null)
                {
                    return;
                }
                if (SuperRandom.randomCharacters[player.nr].Contains(Character.COP) && (player.variant == CharacterVariant.MODEL_ALT || player.variant == CharacterVariant.MODEL_ALT2))
                    found = true;
            });
            return found;
        }
        private static bool CopLuchaInList()
        {
            bool found = false;
            Player.ForAllInMatch((Player player) =>
            {
                if (SuperRandom.randomCharacters[player.nr] == null)
                {
                    return;
                }
                if (EPCDKLCABNC.LEMKFOAAMKA(Character.COP, player.variant) == DLC.COP_LUCHA)
                    found = true;
            });
            return found;
        }

        //DAIO CRIMES
        [HarmonyPatch(typeof(ALDOKEMAOMB), nameof(ALDOKEMAOMB.CHDHDGAHNPB))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ChangeRandomVariant(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher cm = new CodeMatcher(instructions);
            cm.SearchForward(iL => iL.opcode == OpCodes.Stfld && ((FieldInfo)iL.operand).Name == nameof(ALDOKEMAOMB.AIINAIDBHJI))
                .Advance(-3);

            cm.RemoveInstructions(3);

            cm.Insert(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)CharacterVariant.MODEL_ALT3));

            return cm.InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BallEntity), nameof(BallEntity.AddExtraBallVisuals))]
        [HarmonyTranspiler]
        [HarmonyDebug]
        static IEnumerable<CodeInstruction> AddExtraVisual_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
        {
            CodeMatcher cm = new CodeMatcher(instructions, iL);

            cm.SearchForward(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "skate");

            cm.Advance(-1)
                .Set(
                OpCodes.Call, AccessTools.Method(typeof(AddPlayersToWorldPatch), "JetInList")
                );

            cm.SearchForward(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "cop_detective");

            cm.Advance(-1)
                .Set(
                OpCodes.Call, AccessTools.Method(typeof(AddPlayersToWorldPatch), "CopDetectiveInList")
                );

            cm.SearchForward(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "candies");

            cm.Advance(-1)
                .Set(
                OpCodes.Call, AccessTools.Method(typeof(AddPlayersToWorldPatch), "AddCandyAnims")
                );

            cm.SearchForward(i => i.opcode == OpCodes.Stfld && ((FieldInfo)i.operand).Name == "cop_lucha");

            cm.Advance(-1)
                .Set(
                OpCodes.Call, AccessTools.Method(typeof(AddPlayersToWorldPatch), "CopLuchaInList")
                );





            return cm.InstructionEnumeration();
        }

        public static HashSet<CharacterVariant> AddCandyAnims()
        {
            HashSet<CharacterVariant> candies = new HashSet<CharacterVariant>();

            Player.ForAllInMatch((Player player) =>
            {
                if (SuperRandom.randomCharacters[player.nr] == null)
                {
                    return;
                }
                var playerVariants = player.variant;

                SuperRandom.Logger.LogInfo("AddingCandyAnims");
                foreach (var character in SuperRandom.randomCharacters[player.nr])
                {
                    candies.Add(playerVariants);

                }
            });
            return candies;
        }

    }
}






