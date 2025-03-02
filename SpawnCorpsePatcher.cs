using GameplayEntities;
using HarmonyLib;
using LLBML.Players;
using LLHandlers;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace SuperRandom
{
    [HarmonyPatch]
    internal class SpawnCorpsePatcher
    {
        [HarmonyTargetMethod]
        private static MethodInfo TargetMethod()
        {
            return typeof(ItemHandler).GetMethod("SpawnCorpse", BindingFlags.Instance | BindingFlags.Public);
        }

        [HarmonyTranspiler]
        [HarmonyDebug]
        private static IEnumerable<CodeInstruction> SpawnCorpse_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher cm = new CodeMatcher(instructions);


            cm.SearchForward(i => i.opcode == OpCodes.Stloc_0);

            cm.Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                Transpilers.EmitDelegate(delegate (CorpseEntity corpseEntity, ItemHandler itemHandler, int index)
                {
                    return InitializeCorpse(corpseEntity, itemHandler, index);
                }));


            return cm.InstructionEnumeration();
        }



        private static CorpseEntity InitializeCorpse(CorpseEntity corpseEntity, ItemHandler itemHandler, int index)
        {
            var oldID = corpseEntity.entityID;
            GameObject.Destroy(itemHandler.corpseItems[index].gameObject);

            GameObject corpseGo = new GameObject();
            CorpseEntity newCorpse = corpseGo.AddComponent<CorpseEntity>();

            SuperRandom.Logger.LogInfo("Initializing new corpse");
            newCorpse.Init(itemHandler.itemHandlerData.corpseItemsData[index]);

            PlayerEntity playerEntity = Player.GetPlayer(index).playerEntity;
            if (playerEntity != null && playerEntity.character != Character.NONE)
            {
                newCorpse.SetCharacter(playerEntity.character, playerEntity.variant);
            }

            newCorpse.transform.parent = itemHandler.holder;
            newCorpse.entityID = oldID;
            itemHandler.corpseItems[index] = newCorpse;

            itemHandler.world.entityList[oldID - 1] = newCorpse;

            return newCorpse;
        }
    }
}
