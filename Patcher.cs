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

    public static class AddPlayersToWorldPatch
    {


        [HarmonyPatch(typeof(PlayerHandler), nameof(PlayerHandler.AddPlayersToWorld))]
        [HarmonyPostfix]
        public static void AddPlayersToWorld_Postfix(PlayerHandler __instance)
        {
            if (SuperRandom.sRToggled == true)
            {

                SuperRandom.Logger.LogInfo("AddPlayersToWorld Running");
                Player.ForAllInMatch((Player player) =>
                {
                    if (SuperRandom.randomCharacters[player.nr].Count != 0)
                    {
                        var newEntities = new List<PlayerEntity>()
                        {

                        };
                        foreach (var character in SuperRandom.randomCharacters[player.nr])
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

                });
            }

        }
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
            if (SuperRandom.sRToggled)
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
                SuperRandom.Logger.LogInfo($"playerEntities list is null");
                return;
            }

            var entityList = SuperRandom.playerEntities[playerNr];
            if (entityList.Count <= 1)
            {
                return;
            }

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



        private static PlayerEntity CreateExtraEntity(PlayerHandler instance, Player player, Character character)
        {



            PlayerEntity playerEntity = CharacterToModel(character, player.nr);

            playerEntity.character = character;
            playerEntity.variant = player.variant;
            playerEntity.player = player;
            playerEntity.playerIndex = player.nr;
            instance.playerHandlerData.playerData[player.nr].team = player.Team;


            var newPlayerData = new PlayerData(6);
            newPlayerData.Load(instance.playerHandlerData.playerData[player.nr]);
            playerEntity.Init(newPlayerData);

            playerEntity.tf.parent = instance.holder;





            SuperRandom.Logger.LogDebug($"[Patch] Created and disabled extra entity: {player.name}");

            playerEntity.UpdateUnityTransform();

            return playerEntity;
        }

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






