using System.Reflection;
using SuperRandom;

#region Assembly attributes
[assembly: AssemblyVersion(PluginInfos.PLUGIN_VERSION)]
[assembly: AssemblyTitle(PluginInfos.PLUGIN_NAME + " (" + PluginInfos.PLUGIN_ID + ")")]
[assembly: AssemblyProduct(PluginInfos.PLUGIN_NAME)]
#endregion

namespace SuperRandom
{
    public static class PluginInfos
    {
        public const string PLUGIN_NAME = "SuperRandom";
        public const string PLUGIN_ID = "us.Cubical.plugins.llb.SuperRandom";
        public const string PLUGIN_VERSION = "1.0.0.0";
    }
}

