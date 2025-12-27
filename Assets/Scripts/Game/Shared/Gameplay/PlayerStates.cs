using Core.Interface.PlayerInfoUI;
using Core.Model;
using Core.Utils;
using Manager;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Shared.Gameplay
{
    public class PlayerStates : MonoBehaviour, IPlayerInfoSubject, IPunInstantiateMagicCallback
    {
        public Character character;
        private bool isDead = false;
  

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {

            object[] instantiationData = info.photonView.InstantiationData;
            character = new Character((string)instantiationData[1], (string)instantiationData[0], 100);
            gameObject.GetComponentInChildren<Renderer>().material.color = ColorUtils.ParseRGBA(character.color);
            GameManager.playerInfoSubjects.Add(this);
        }


        public bool decreaseHealth(int amount)
        {
            isDead = this.character.decreaseHealth(amount);
            this.NotifyObservers(character.nickname, character.currentHealth);
            return isDead;
        }

        public void RegisterObserver(IPlayerInfoObserver observer)
        {
            GameManager.playerInfoObservers.Add(observer);
        }

        public void RegisterObservers(List<IPlayerInfoObserver> obsList)
        {
            GameManager.playerInfoObservers.AddRange(obsList);
        }
        public void RemoveObserver(IPlayerInfoObserver observer)
        {
            GameManager.playerInfoObservers.Remove(observer);
        }
        public void NotifyObservers(string nickName, float currentHealth)
        {
            foreach (var obs in GameManager.playerInfoObservers)
            {
                obs.OnNotify(nickName, currentHealth);
            }
        }
    }
}

