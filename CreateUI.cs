using HarmonyLib;
using LLScreen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SuperRandom
{
    internal class CreateUI
    {
        [HarmonyPatch(typeof(PlayersSelection), nameof(PlayersSelection.Init))]
        [HarmonyPostfix]
        private static void Init(int _playerNr)
        {

            SuperRandom.superRandomTweak.CreateUI();

            if (SuperRandomTweak.sRToggled)
            {
                SuperRandom.superRandomTweak.EnableSuperRandomMode();

            }
        }

        [HarmonyPatch(typeof(ScreenPlayers), nameof(ScreenPlayers.OnOpen))]
        [HarmonyPrefix]
        private static void Prefix_OnOpen(ScreenPlayers __instance)
        {
            SuperRandomTweak.sP = __instance;

        }
    }
}
