using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LLBML.GameEvents;
using LLBT;
using LLBT.Tweaks;

namespace SuperRandom
{
    [BepInPlugin("us.Cubical.plugins.llb.SuperRandom", "SuperRandom", "1.0.0.0")]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]
    public class SuperRandom : BaseUnityPlugin
    {
        public static SuperRandom Instance { get; private set; } = null;
        public static ManualLogSource Log { get; private set; } = null;

        Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);

        public SuperRandomTweak superRandomTweak;

        public void Awake()
        {
            Instance = this;
            Log = this.Logger;

            superRandomTweak = new SuperRandomTweak();
            LLBTweaker.AddTweak(superRandomTweak);
            Logger.LogDebug("Added SuperRandom Tweak");


            SuperRandom.Instance.superRandomTweak.ConfigInit();
            
            
            Networking.Init();
        }
    }
}


