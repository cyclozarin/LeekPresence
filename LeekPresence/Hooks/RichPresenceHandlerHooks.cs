using Discord;
using Photon.Pun;
using Steamworks;
using System;
using System.Linq;
using UnityEngine;

namespace LeekPresence.Hooks
{
    public class RichPresenceHandlerHooks
    {
        private static bool _failedQuota = false; // seemingly SurfaceNetworkHandler.Instance.m_FailedQuota doesnt work

        private static string _failedQuotaString = null!;

        internal static void Init()
        {
            On.RichPresenceHandler.Initialize += MMHook_InitRichPresence;
            On.RichPresenceHandler.UpdateDiscord += MMHook_UpdateDiscord;

            On.SurfaceNetworkHandler.RPC_QuotaFailed += (orig, self) =>
            {
                _failedQuota = true;
                _failedQuotaString = $"Failed quota {SurfaceNetworkHandler.RoomStats.CurrentRun + 1} ({RichPresenceHandler._currentQuota}) on day {RichPresenceHandler._currentDay}"; // if we won't cache string on quota fail we will encounter NullReferenceException
                RichPresenceHandler.DirtyDiscord();
                orig(self);
            };
        }

        private static void MMHook_InitRichPresence(On.RichPresenceHandler.orig_Initialize orig)
        {
            try
            {
                RichPresenceHandler._discord = new Discord.Discord(1278755625123184681L, (ulong)CreateFlags.NoRequireDiscord);
            }
            catch (Exception arg)
            {
                LeekPresence.Logger.LogError($"Presence++ failed to initialize Discord Rich Presence: {arg}");
            }

            ActivityManager _activityManager = RichPresenceHandler._discord.GetActivityManager();
            _activityManager.RegisterSteam(2881650); // registering cw via its steam id
            _activityManager.OnActivityJoin += secret =>
            {
                if (ulong.TryParse(secret, out ulong _rawLobbyId))
                    MainMenuHandler.SteamLobbyHandler.TryToJoinLobby((CSteamID)_rawLobbyId);
                else
                    LeekPresence.Logger.LogInfo("Cannot accept Discord game invite via Presence++; lobby id to join in may be wrong");
                LeekPresence.Logger.LogInfo("Accepted Discord game invite via Presence++");
            };
        }

        private static void MMHook_UpdateDiscord(On.RichPresenceHandler.orig_UpdateDiscord orig)
        {
            if (RichPresenceHandler._discord != null)
            {
                ActivityManager _activityManager = RichPresenceHandler._discord.GetActivityManager();

                var _activity = new Activity
                {
                    State = GetStateString(),
                    Details = GetDetailsString(),
                    Assets = new()
                    {
                        LargeImage = GetLargeIconString(),
                        LargeText = GetLargeIconText(),
                        SmallImage = GetSmallIconString(),
                        SmallText = GetSmallIconText()
                    }
                };

                if (RichPresenceHandler._currentPlayersInGroup > 0)
                {
                    _activity.Party = new ActivityParty
                    {
                        Id = RichPresenceHandler._currentGroup,
                        Size = new PartySize
                        {
                            CurrentSize = RichPresenceHandler._currentPlayersInGroup,
                            MaxSize = PhotonNetwork.CurrentRoom.MaxPlayers
                        },
                        Privacy = ActivityPartyPrivacy.Public,
                    };

                    if (!SurfaceNetworkHandler.HasStarted)
                        _activity.Secrets.Join = MainMenuHandler.SteamLobbyHandler.m_CurrentLobby.ToString();
                }

                if (!_failedQuota && !CameraHook.Recording && PlayerHook.StatusUpdateTimer != 0)
                    _activity.Timestamps.Start = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                _activityManager.UpdateActivity(_activity, delegate (Result result)
                {
                    if (result != Result.Ok)
                        LeekPresence.Logger.LogError($"Something went wrong while updating Discord Rich Presence; error code is {result}");
                });
            }
        }

        private static string GetStateString()
        {
            if (_failedQuota)
            {
                return string.Empty;
            }
            switch (RichPresenceHandler._currentState)
            {
                case RichPresenceState.Status_PlayingWithFriends:
                    return "In friends lobby";
                case RichPresenceState.Status_PlayingWithRandoms:
                    return "In random lobby";
                case RichPresenceState.Status_InFactory:
                case RichPresenceState.Status_InShip:
                    return "In the Old World";
            }
            if (RichPresenceHandler._currentState == RichPresenceState.Status_AtHouse && TimeOfDayHandler.TimeOfDay == TimeOfDay.Morning)
            {
                return "At the surface";
            }
            else if (RichPresenceHandler._currentState == RichPresenceState.Status_AtHouse && TimeOfDayHandler.TimeOfDay == TimeOfDay.Evening)
            {
                return "Back from the Old World";
            }

            return string.Empty;
        }

        private static string GetDetailsString()
        {
            if (_failedQuota)
            {
                return _failedQuotaString;
            }
            switch (RichPresenceHandler._currentState)
            {
                case RichPresenceState.Status_MainMenu:
                    return "In main menu";
                case RichPresenceState.Status_AtHouse:
                case RichPresenceState.Status_InFactory:
                case RichPresenceState.Status_InShip:
                    return $"Day {RichPresenceHandler._currentDay} ({(SurfaceNetworkHandler.RoomStats.GetDaysLeft() == 0 ? "last day" : $"{SurfaceNetworkHandler.RoomStats.GetDaysLeft() + 1} days left")}), {RichPresenceHandler._currentViews}/{RichPresenceHandler._currentQuota}, {SurfaceNetworkHandler.RoomStats.Money}$";
            }

            return string.Empty;
        }

        private static string GetLargeIconString()
        {
            if (_failedQuota)
                return "failedquota";
            switch (RichPresenceHandler._currentState)
            {
                case RichPresenceState.Status_MainMenu:
                    return "mainmenu";
                case RichPresenceState.Status_PlayingWithFriends:
                case RichPresenceState.Status_PlayingWithRandoms:
                    return "lobby";
                case RichPresenceState.Status_InFactory:
                case RichPresenceState.Status_InShip:
                    return LeekPresence.GetCurrentMap().ToLower();
            }
            if (RichPresenceHandler._currentState == RichPresenceState.Status_AtHouse && TimeOfDayHandler.TimeOfDay == TimeOfDay.Morning)
            {
                return "surface";
            }
            else if (RichPresenceHandler._currentState == RichPresenceState.Status_AtHouse && TimeOfDayHandler.TimeOfDay == TimeOfDay.Evening)
            {
                return "mainmenu";
            }

            return string.Empty;
        }

        private static string GetLargeIconText()
        {
            if (_failedQuota)
            {
                _failedQuota = false;
                return "Womp womp";
            }
            else if (LeekPresence.InTheOldWorld())
                return $"Current map: {LeekPresence.GetCurrentMap()}";
            else if (RichPresenceHandler._currentState == RichPresenceState.Status_AtHouse && TimeOfDayHandler.TimeOfDay == TimeOfDay.Morning)
                return "At the surface";
            else if (RichPresenceHandler._currentState == RichPresenceState.Status_AtHouse && TimeOfDayHandler.TimeOfDay == TimeOfDay.Evening)
                return "Back from the Old World";
            switch (RichPresenceHandler._currentState)
            {
                case RichPresenceState.Status_PlayingWithFriends:
                    return "In friends lobby";
                case RichPresenceState.Status_PlayingWithRandoms:
                    return "In random lobby";
            }

            return string.Empty;
        }

        private static string GetSmallIconString()
        {
            if (CameraHook.Recording)
            {
                return "https://contentwarning.wiki.gg/images/thumb/8/80/Camera.png/350px-Camera.png"; // borrowed from cw wiki :3
            }

            if (LeekPresence.InTheOldWorld())
            {
                if (!Player.localPlayer.data.dead)
                    return "alive";
                else if (PlayerHandler.instance.playersAlive.Count == 0 && PlayerHandler.instance.players.Count > 1)
                    return "everyoneisdead";
                else
                    return "dead";
            }

            if (LeekPresence.ViralityLoaded())
                return "virality";
            else
                return "cw";
        }

        private static string GetSmallIconText()
        {
            if (CameraHook.Recording)
                return $"Recording on camera\nFilm left: {CameraHook.FilmLeftInPercentage}% ({CameraHook.FilmLeftInSeconds}s)";

            if (LeekPresence.InTheOldWorld())
            {
                if (!Player.localPlayer.data.dead)
                    return $"Alive - HP: {Mathf.Round(Player.localPlayer.data.health)}, O2: {Mathf.Round(Player.localPlayer.data.OxygenPercentage() * 100)}%";
                else if (PlayerHandler.instance.playersAlive.Count == 0 && PlayerHandler.instance.players.Count > 1)
                    return "Everyone is dead.";
                else
                    return $"Dead - {Mathf.Round(HelperFunctions.FlatDistance(DiveBellParent.instance.transform.GetComponentsInChildren<DivingBell>().First((bell) => bell.enabled).transform.position, Player.localPlayer.refs.ragdoll.GetBodypart(BodypartType.Hip).transform.position))}m away from diving bell";
            }

            var _pluginsCount = BepInEx.Bootstrap.Chainloader.PluginInfos.Count;

            if (LeekPresence.ViralityLoaded())
                return $"{_pluginsCount} mods loaded, Virality enabled";

            return $"{_pluginsCount} mods loaded";
        }
    }
}
