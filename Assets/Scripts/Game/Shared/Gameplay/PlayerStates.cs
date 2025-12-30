using Core.Interface.PlayerInfoUI;
using Core.Model;
using Core.Utils;
using ExitGames.Client.Photon;
using Manager;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Shared.Gameplay
{
    public class PlayerStates : MonoBehaviour, IPlayerInfoSubject, IPunInstantiateMagicCallback
    {
        public Character character;

        private PlayerControls playerControls;
        public PhotonView photonView;

        /// <summary>
        /// This method is called when the Photon network instantiates this object.
        /// </summary>
        /// <param name="info"></param>

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instantiationData = info.photonView.InstantiationData;
            character = new Character((string)instantiationData[1], (string)instantiationData[0], 100);
            gameObject.name = character.nickname;
            gameObject.GetComponentInChildren<Renderer>().material.color = ColorUtils.ParseRGBA(character.color);
            playerControls = GetComponent<PlayerControls>();
            GameManager.playerInfoSubjects.Add(this);

            photonView = GetComponent<PhotonView>();
        }


        /// <summary>
        /// Decreases the player's health by the specified amount and notifies observers of the change.
        /// if the health drops to zero or below, marks the player as dead.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool decreaseHealth(int amount)
        {
            character.isDead = this.character.decreaseHealth(amount);

            this.NotifyObserversToModifyTheHealthBar(character.nickname, character.currentHealth);
            return character.isDead;
        }

        /// <summary>
        /// RPC method to reset the player's state, including health and position.
        /// </summary>
        /// <param name="spawnPoint"></param>
        [PunRPC]
        public void RPC_ResetPlayer(Vector3 spawnPoint)
        {
            this.character.currentHealth = this.character.maxHealth;
            GetComponent<Transform>().position = spawnPoint;
            this.NotifyObserversToModifyTheHealthBar(character.nickname, character.currentHealth);
            character.isDead = false;
            playerControls.enabled = true;
            playerControls.animator.Rebind();
        }

        /// <summary>
        /// RPC method to decrease the player's health by a specified damage amount.
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="killedPlayerNickname"></param>
        [PunRPC]
        public void RPC_DecreaseHealth(int damage, string killedPlayerNickname)
        {
            Debug.Log($"RPC_DecreaseHealth called on {character.nickname} for {damage} damage by {killedPlayerNickname}");
            if (!photonView.IsMine) return;
            if (!character.isDead)
            {
                character.isDead = this.decreaseHealth(damage);
                if (character.isDead)
                {
                    playerControls.animator.SetTrigger("isDead");
                    playerControls.enabled = false;
                    Debug.Log($"{this.character.nickname} a tué {killedPlayerNickname}");

                    int score = character.increaseScoreByValue(1);

                    object[] content = new object[] { killedPlayerNickname, score, gameObject.name };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(Constant.PunEventCode.playerHaveBeenKilledEventCode, content, raiseEventOptions, SendOptions.SendReliable);
                }
                else
                {
                    StartCoroutine(PlayHitAnimation());
                }
            }
        }

        /// <summary>
        /// Plays the hit animation for the player.
        /// and after a delay, resets the hit trigger .
        /// </summary>
        /// <returns></returns>
        IEnumerator PlayHitAnimation()         {
            playerControls.animator.SetTrigger("isHit");
            yield return new WaitForSeconds(0.5f);
            playerControls.animator.ResetTrigger("isHit");
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
        public void NotifyObserversToModifyTheHealthBar(string nickName, float currentHealth)
        {
            foreach (var obs in GameManager.playerInfoObservers)
            {
                obs.OnNotifyToModifyTheHealthBar(nickName, currentHealth);
            }
        }
    }
}

