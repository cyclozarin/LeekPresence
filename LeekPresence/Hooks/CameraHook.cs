using System;
using UnityEngine;

namespace LeekPresence.Hooks
{
    internal class CameraHook
    {
        internal static bool Recording = false;

        internal static int FilmLeftInSeconds = 0;

        internal static int FilmLeftInPercentage = 0;

        internal static float CameraStatusUpdateInterval = 1;

        internal static float CameraStatusUpdateTimer = 1;

        internal static void Init()
        {
            On.VideoCamera.Update += MMHook_Prefix_CameraUpdate;
        }

        private static void MMHook_Prefix_CameraUpdate(On.VideoCamera.orig_Update orig, VideoCamera self)
        {
            Recording = self.m_recorderInfoEntry.isRecording;

            if (self.HasFilmLeft && self.m_recorderInfoEntry.isRecording)
            {
                if (CameraStatusUpdateTimer > 0)
                {
                    CameraStatusUpdateTimer -= Time.deltaTime;
                }
                else
                {
                    FilmLeftInSeconds = Mathf.RoundToInt(self.m_recorderInfoEntry.timeLeft);
                    FilmLeftInPercentage = Mathf.RoundToInt(self.m_recorderInfoEntry.GetPercentage() * 100);
                    RichPresenceHandler.DirtyDiscord();
                    CameraStatusUpdateTimer = CameraStatusUpdateInterval;
                }
            }

            orig(self);
        }
    }
}
