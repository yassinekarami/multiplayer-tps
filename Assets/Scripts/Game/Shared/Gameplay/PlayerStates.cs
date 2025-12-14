using UnityEngine;
using Core.Model;
using Photon.Pun;
using Core.Utils;
namespace Game.Shared.Gameplay
{
    public class PlayerStates : MonoBehaviour, IPunInstantiateMagicCallback
    {
        public Character character;

        private void Start()
        {
         //   PlayerGotShotEvent.onPlayerShot += decreaseHealth;
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {

            object[] instantiationData = info.photonView.InstantiationData;
            character = new Character((string)instantiationData[1], (string)instantiationData[0], 100);
            gameObject.GetComponentInChildren<Renderer>().material.color = ColorUtils.ParseRGBA(character.color);
        

        }


        public void decreaseHealth(int amount)
        {
            Debug.Log(gameObject.name +" has been shot");
            this.character.decreaseHealth(amount);
            Debug.Log("remaining health " + this.character.currentHealth);
        }
    }
}

