using GameplayEntities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;


namespace SuperRandom
{
    [HarmonyPatch]
    internal class SpawnPlayerPatcher
    {
        [HarmonyTargetMethod]
        private static MethodInfo TargetMethod()
        {
            Type firstInner = typeof(OGONAGCFDPK).GetNestedType("BONHAFGBBNE", BindingFlags.NonPublic);
            return firstInner.GetMethod("JICJCNKMBBN", BindingFlags.Instance | BindingFlags.NonPublic);
        }
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> SpawnPlayers_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher cm = new CodeMatcher(instructions);
            cm.SearchForward(i => i.opcode == OpCodes.Callvirt && (i.operand as MethodBase).Name == "Spawn");
            cm.Insert( 
                Transpilers.EmitDelegate(delegate (PlayerEntity playerEntity)
                {
                    return AddPlayersToWorldPatch.ResetNextCharacter(playerEntity);
                })
                );
            return cm.InstructionEnumeration();
        }

    }

}
