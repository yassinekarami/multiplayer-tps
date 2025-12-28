using Core.Interface.PlayerInfoUI;
using Core.Model;
using Core.Utils;
using ExitGames.Client.Photon;
using Manager;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Shared.Gameplay
{
    public class PlayerStates : MonoBehaviour, IPlayerInfoSubject, IPunInstantiateMagicCallback
    {
        public Character character;


        private bool isDead = false;
        private PlayerControls playerControls;


        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            object[] instantiationData = info.photonView.InstantiationData;
            character = new Character((string)instantiationData[1], (string)instantiationData[0], 100);
            gameObject.GetComponentInChildren<Renderer>().material.color = ColorUtils.ParseRGBA(character.color);
            playerControls = GetComponent<PlayerControls>();
            GameManager.playerInfoSubjects.Add(this);
        }

        [PunRPC]
        public void ResetPlayer(Vector3 spawnPoint)
        {
            this.character.currentHealth = this.character.maxHealth;
            GetComponent<Transform>().position = spawnPoint;
            this.NotifyObservers(character.nickname, character.currentHealth);
            isDead = false;
            playerControls.enabled = true;
            playerControls.animator.Rebind();
        }

        public bool decreaseHealth(int amount)
        {
            isDead = this.character.decreaseHealth(amount);

            this.NotifyObservers(character.nickname, character.currentHealth);
            return isDead;
        }

        [PunRPC]
        public void RPC_DecreaseHealth(int damage, string attackerNickname)
        {
            if (GetComponent<PhotonView>().IsMine)
            {
                bool isDead = this.decreaseHealth(damage);
                if (isDead)
                {
                    playerControls.animator.SetTrigger("isDead");
                    playerControls.enabled = false;
                    Debug.Log($"{attackerNickname} a tué {this.character.nickname}");
                    int score = character.score + 1;

                    object[] content = new object[] { character.nickname, character.score + 1, gameObject.name };
                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                    PhotonNetwork.RaiseEvent(Constant.PunEventCode.playerHaveBeenKilledEventCode, content, raiseEventOptions, SendOptions.SendReliable);
                }
                else
                {
                    playerControls.animator.Play("Rib Hit");
                }
            }
                

            
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

