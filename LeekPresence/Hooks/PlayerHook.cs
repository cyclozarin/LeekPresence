using UnityEngine;

namespace LeekPresence.Hooks
{
    internal class PlayerHook
    {
        internal static float StatusUpdateInterval = 10;

        internal static float StatusUpdateTimer = 10;

        internal static void Init()
        {
            On.Player.RPCA_PlayerDie += MMHook_Postfix_Die;
            On.Player.CallHeal += MMHook_Postfix_Heal;
            On.Player.RPCA_PlayerRevive += MMHook_Postfix_Revive;
            On.Player.TakeDamage += MMHook_Postfix_Damage;
            On.Player.PlayerData.UpdateValues += MMHook_Postfix_UpdateStatus;
        }

        // hooks to update player status according to if they die, being healed, damaged or revived
        private static void MMHook_Postfix_UpdateStatus(On.Player.PlayerData.orig_UpdateValues orig, Player.PlayerData self)
        {
            orig(self);
            if (self.player.IsLocal)
            {
                if (StatusUpdateTimer > 0)
                {
                    StatusUpdateTimer -= Time.deltaTime;
                }
                else
                {
                    RichPresenceHandler.DirtyDiscord();
                    StatusUpdateTimer = StatusUpdateInterval;
                }
            }
        }

        private static void MMHook_Postfix_Damage(On.Player.orig_TakeDamage orig, Player self, float damage)
        {
            orig(self, damage);
            if (self.IsLocal)
                RichPresenceHandler.DirtyDiscord();
        }

        private static void MMHook_Postfix_Revive(On.Player.orig_RPCA_PlayerRevive orig, Player self)
        {
            orig(self);
            if (self.IsLocal)
                RichPresenceHandler.DirtyDiscord();
        }

        private static bool MMHook_Postfix_Heal(On.Player.orig_CallHeal orig, Player self, float healAmount)
        {
            bool _orig = orig(self, healAmount);

            if (self.IsLocal)
                RichPresenceHandler.DirtyDiscord();

            return _orig;
        }

        private static void MMHook_Postfix_Die(On.Player.orig_RPCA_PlayerDie orig, Player self)
        {
            orig(self);
            if (self.IsLocal)
                RichPresenceHandler.DirtyDiscord();
        }
    }
}
