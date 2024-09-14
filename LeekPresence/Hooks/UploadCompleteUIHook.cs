using System;

namespace LeekPresence.Hooks
{
    internal class UploadCompleteUIHook
    {
        internal static string ViewsString = string.Empty;

        internal static void Init()
        {
            On.UploadCompleteUI.PlayVideo += MMHook_Postfix_PlayVideo;
            On.UploadCompleteUI.DisplayComment += MMHook_Postfix_DisplayComment;
        }

        private static void MMHook_Postfix_DisplayComment(On.UploadCompleteUI.orig_DisplayComment orig, UploadCompleteUI self, Comment comment)
        {
            orig(self, comment);
            ViewsString = self.m_ViewsText.ToLower();
            RichPresenceHandler.DirtyDiscord();
        }

        private static void MMHook_Postfix_PlayVideo(On.UploadCompleteUI.orig_PlayVideo orig, UploadCompleteUI self, IPlayableVideo playableVideo, int views, Comment[] comments, Action onPlayed)
        {
            orig(self, playableVideo, views, comments, onPlayed);
            ViewsString = self.m_ViewsText.ToLower();
            RichPresenceHandler.DirtyDiscord();
        }
    }
}
