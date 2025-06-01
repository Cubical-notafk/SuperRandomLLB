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

        private static void ResetHud(PlayerEntity __instance)
        {
            SuperRandom.Log.LogDebug("Resetting Hud starting");
            var playerInfo = ScreenGameHud.instance.playerInfos[__instance.playerIndex];
            int count = playerInfo.transform.childCount;
            
            for (int i = count - 1; i >= 0; i--)
            {
                var child = playerInfo.transform.GetChild(i);
                if (child.name.Contains("Head"))
                {
                    GameObject.Destroy(child.gameObject);
                    break;
                }
            }
            playerInfo.SetPlayer(__instance.player, 12);
        }

        
        public static PlayerEntity ResetNextCharacter(PlayerEntity currentEntity)
        {

            if (currentEntity == null)
            {
                SuperRandom.Log.LogDebug("ResetNextCharacter: currentEntity is null!");
                return null;
            }


            SuperRandom.Log.LogDebug("ResetNextCharacter starting");

            Player player = currentEntity.player;

            int playerNr = player.nr;

            SuperRandom.Log.LogDebug($"Player {player.nr} has died");

            

            var handler = World.instance.playerHandler;
            if (SuperRandomTweak.randomCharacters[player.nr] == null )
            {
                return currentEntity;
            }
            var entityList = SuperRandomTweak.randomCharacters[player.nr];

            SuperRandom.Log.LogDebug($"current death count : {currentEntity.playerData.deaths}");
            if (entityList == null || entityList.Count == 0)
            {
                SuperRandom.Log.LogDebug($"ResetNextCharacter: entityList for player {player.nr} is null or empty!");
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
            

            if (currentEntity.playerData.deaths >= SuperRandomTweak.numberOfStocks)
            {
                return currentEntity;
            }
            int prevStocks = World.instance.playerHandler.playerHandlerData.playerData[player.playerEntity.playerIndex].stocks;

            var character = Character.NONE;


            if (JOMBNFKIHIC.GIGAKBJGFDI.KOBEJOIALMO && currentEntity.playerData.deaths > 12) //Points is True
            {
                 character = entityList[ControlledRandom.Get(0, 0, SuperRandomTweak.randomCharacters[player.nr].Count)];
            }
            else 
            {
                 character = entityList[currentEntity.playerData.deaths];
            }

            player.Character = character;

            
            PlayerEntity newPlayerEntity = handler.CreatePlayerEntity(player);
            

            newPlayerEntity.playerData.deaths = currentEntity.playerData.deaths;


            SuperRandom.Log.LogDebug($"new death count : {newPlayerEntity.playerData.deaths}");

            player.Character = newPlayerEntity.character;

            SuperRandom.Log.LogDebug($"Setting {player.nr} to {player.Character}");

            newPlayerEntity.playerData.specialAmount = 0;
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
                if (SuperRandomTweak.randomCharacters[player.nr] == null)
                {
                    return;
                }
                if (SuperRandomTweak.randomCharacters[player.nr].Contains(Character.SKATE))
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
                if (SuperRandomTweak.randomCharacters[player.nr] == null)
                {
                    return;
                }
                if (SuperRandomTweak.randomCharacters[player.nr].Contains(Character.COP) && (player.variant == CharacterVariant.MODEL_ALT || player.variant == CharacterVariant.MODEL_ALT2))
                    found = true;
            });
            return found;
        }
        private static bool CopLuchaInList()
        {
            bool found = false;
            Player.ForAllInMatch((Player player) =>
            {
                if (SuperRandomTweak.randomCharacters[player.nr] == null)
                {
                    return;
                }
                if (EPCDKLCABNC.LEMKFOAAMKA(Character.COP, player.variant) == DLC.COP_LUCHA)
                    found = true;
            });
            return found;
        }

        

        [HarmonyPatch(typeof(BallEntity), nameof(BallEntity.AddExtraBallVisuals))]
        [HarmonyTranspiler]
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
                if (SuperRandomTweak.randomCharacters[player.nr] == null)
                {
                    return;
                }
                var playerVariants = player.variant;

                SuperRandom.Log.LogInfo("AddingCandyAnims");
                foreach (var character in SuperRandomTweak.randomCharacters[player.nr])
                {
                    candies.Add(playerVariants);

                }
            });
            return candies;
        }

    }
}






