using Manager;
using Photon.Pun;
using UnityEngine;


namespace Game.Shared.Gameplay
{
    public class WeaponPickup : MonoBehaviourPun
    {
        GameManager gameManager;

        public void Start()
        {
            gameManager = GameObject.Find("Manager").GetComponent<GameManager>();
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player picked up weapon!");
                // Add weapon to player's inventory logic here
                if (other.gameObject.GetComponent<WeaponHandler>() != null)
                {
                    other.gameObject.GetComponent<WeaponHandler>().increaseAmmo(gameObject.name);
                }
                gameManager.DestroyPickup(photonView.ViewID);
            }
        }
    }
}
