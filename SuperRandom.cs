using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using LLBML.GameEvents;
using LLBML.Utils;
using LLBT;
using LLBT.Tweaks;
using UnityEngine;

namespace SuperRandom
{
    
    [BepInPlugin("us.Cubical.plugins.llb.SuperRandom", "SuperRandom", "1.0.1")]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]
    public class SuperRandom : BaseUnityPlugin
    {
        public static SuperRandom Instance { get; private set; } = null;
        public static ManualLogSource Log { get; private set; } = null;

        public static Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        public static SuperRandomTweak superRandomTweak;

        public ConfigEntry<bool> superRandomOn;
        public ConfigEntry<bool> useKarmicRandom;
        public ConfigEntry<bool> resetWeightButton;
        public ConfigEntry<KeyCode> SwapCharacterInTrainingKey;
        public ConfigEntry<KeyCode> ResetWeightsKey;

        public void Awake()
        {
            Instance = this;
            Log = this.Logger;
            harmony.PatchAll(typeof(CreateUI));
            superRandomTweak = new SuperRandomTweak();
            LLBTweaker.AddTweak(superRandomTweak);
            Log.LogDebug("Added SuperRandom Tweak");
            Networking.Init();

            superRandomOn = Config.Bind("Toggles", "SuperRandomOn", true);
            useKarmicRandom = Config.Bind<bool>("Toggles", "UseKarmicRandom", true);
            resetWeightButton = Config.Bind<bool>("Toggles", "ResetKarmaWeights", false);
            SwapCharacterInTrainingKey = Config.Bind("Keybinds", "swapCharacterInTrainingKey", KeyCode.Q);
            ResetWeightsKey = Config.Bind("Keybinds", "resetWeightsKey", KeyCode.W);
        }
    }
}


