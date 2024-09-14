global using Plugin = LeekPresence.LeekPresence;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LeekPresence.Hooks;
using System.Linq;
using UnityEngine.SceneManagement;

namespace LeekPresence
{
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, true)]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class LeekPresence : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;

        internal static ConfigEntry<long> DiscordAppID { get; private set; } = null!;

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;

            HookAll();

            DiscordAppID = Config.Bind("General", "Discord App ID", 1278755625123184681L, "Determines Discord App ID for mod to use");

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION} by {MyPluginInfo.PLUGIN_GUID.Split("")[0]} has loaded!");
        }

        internal static void HookAll()
        {
            Logger.LogDebug("Hooking...");

            RichPresenceHandlerHooks.Init();
            CameraHook.Init();
            PlayerHook.Init();
            UploadCompleteUIHook.Init();

            Logger.LogDebug("Finished hooking!");
        }

        internal static string GetCurrentMap()
        {
            return SceneManager.GetActiveScene().name.Replace("Scene", "");
        }

        internal static bool ViralityLoaded()
        {
            return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("MaxWasUnavailable.Virality");
        }

        internal static bool ViralityLateJoinEnabled()
        {
            BepInEx.Bootstrap.Chainloader.PluginInfos.First((x) => x.Key == "MaxWasUnavailable.Virality").Value.Instance.Config.TryGetEntry<bool>("General", "AllowLateJoin", out var _allowLateJoin);
            return _allowLateJoin.Value;
        }

        internal static bool InTheOldWorld()
        {
            return RichPresenceHandler._currentState == RichPresenceState.Status_InFactory || RichPresenceHandler._currentState == RichPresenceState.Status_InShip;
        }
    }
}
