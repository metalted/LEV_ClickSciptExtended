using BepInEx;
using HarmonyLib;

namespace LEV_ClickSciptExtended
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGUID = "com.metalted.zeepkist.levclickscriptx";
        public const string pluginName = "LEV_ClickScriptX";
        public const string pluginVersion = "1.0";

        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony harmony = new Harmony(pluginGUID);
            harmony.PatchAll();
        }
    }        
}
