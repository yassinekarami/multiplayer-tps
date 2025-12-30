using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Interface.PlayerInfoUI
{
    /// <summary>
    /// Defines a contract for receiving notifications about a player's status, including the player's nickname and
    /// current health.
    /// </summary>
    public interface IPlayerInfoObserver
    {
        /// <summary>
        /// Handles a notification event for a player, providing the player's nickname and current health.
        /// </summary>
        /// <param name="nickName">The nickname of the player associated with the notification. Cannot be null.</param>
        /// <param name="currentHealth">The current health value of the player. Typically expected to be a non-negative number.</param>
        public void OnNotifyToModifyTheHealthBar(string nickName, float currentHealth);
    }

}