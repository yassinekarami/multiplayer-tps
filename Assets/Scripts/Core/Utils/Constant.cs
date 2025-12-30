
using UnityEditor.Rendering;

namespace Core.Utils
{
    /// <summary>
    /// class of constant
    /// </summary>
    public static class Constant
    {
        public static class Tag
        {
            public const string SPAWN_POINT = "SpawnPoint";
            public const string PLAYER = "Player";
            public const string WEAPON_SPAWN_POINT = "WeaponSpawnPoint";
        }
        /// <summary>
        /// class for all pun event code
        /// </summary>
        public static class PunEventCode
        {
            public const byte updateWaitingPanelUIEventCode = 1; 
            public const byte theGameIsReadyEventCode = 2;
            public const byte setUpPlayerInfoPanelEventCode = 3;
            public const byte colorHasBeenChooseEventCode = 4;
            public const byte updatePlayerHealthUIEventCode = 6;
            public const byte playerHaveBeenKilledEventCode = 7;
        }

        public enum WeaponNotificationType
        {
            WEAPON_CHANGE,
            WEAPON_AMMO_UPDATE
        }

    }

}
