using Core.Interface.PlayerInfoUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Interface.PlayerInfoUI
{
    /// <summary>
    /// Defines a contract for a subject that manages player information observers and notifies them of player data
    /// changes.
    /// </summary>
    /// <remarks>Implementations of this interface allow observers to subscribe to and receive updates when
    /// player information, such as health, changes. This interface follows the observer design pattern, enabling
    /// decoupled notification of interested parties. Observers must implement the IPlayerInfoObserver interface to
    /// receive updates.</remarks>
    public interface IPlayerInfoSubject
    {
        /// <summary>
        /// Registers an observer to receive notifications about player information updates.
        /// </summary>
        /// <param name="observer">An object that implements the IPlayerInfoObserver interface and will be notified when player information
        /// changes. Cannot be null.</param>
        public void RegisterObserver(IPlayerInfoObserver observer);

        /// <summary>
        /// Registers a collection of observers to receive player information updates.
        /// </summary>
        /// <param name="obsList">A list of observers that implement the IPlayerInfoObserver interface. Cannot be null. Each observer in the
        /// list will be notified of player information changes.</param>
        public void RegisterObservers(List<IPlayerInfoObserver> obsList);
        /// <summary>
        /// Unsubscribes the specified observer from receiving player information updates.
        /// </summary>
        /// <param name="observer">The observer to remove from the notification list. Cannot be null.</param>
        public void RemoveObserver(IPlayerInfoObserver observer);
        /// <summary>
        /// Notifies all registered observers of a change in the specified player's health.
        /// </summary>
        /// <param name="nickName">The nickname of the player whose health has changed. Cannot be null or empty.</param>
        /// <param name="currentHealth">The player's current health value after the change.</param>
        public void NotifyObserversToModifyTheHealthBar(string nickName, float currentHealth);
        
    }
}
