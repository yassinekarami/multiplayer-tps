using Core.Event;
using Core.Interface.WeaponUI;
using Core.Model;
using Core.Utils;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Manager;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static Core.Utils.Constant;
namespace Game.Shared.Gameplay {

    public class WeaponHandler : MonoBehaviour, IWeaponUISubject
    {
        Dictionary<Weapon, GameObject> weaponsDictionary = new Dictionary<Weapon, GameObject>();

        Weapon firstWeapon = new Weapon(0, "Sci-Fi Gun", 30, true, 10, 0);
        Weapon secondWeapon = new Weapon(1, "RL0N-25_low", 30, false, 20, 1);
        Weapon thirdWeapon = new Weapon(2, "Bio Integrity Gun", 30, false, 30, 2);

        [Header("Weapons GameObjects")]
        public GameObject Sci_Fi_Gun;
        public GameObject RL0N_25_low;
        public GameObject Bio_Integrity_Gun;


        Weapon currentWeapon;
        private Camera playerCamera;
        public GameObject cameraHolder;
        public List<AudioClip> weaponsSound = new List<AudioClip>();
        private AudioSource audioSource;

        public PlayerStates playerStates;
        private void Awake()
        {
            if(GetComponentInParent<PhotonView>() == null || !GetComponentInParent<PhotonView>().IsMine) return;

            GameManager.weaponUISubject.Add(this);
           
            weaponsDictionary.Add(firstWeapon, Sci_Fi_Gun);
            weaponsDictionary.Add(secondWeapon, RL0N_25_low);
            weaponsDictionary.Add(thirdWeapon, Bio_Integrity_Gun);

            foreach (KeyValuePair<Weapon, GameObject> entry in weaponsDictionary)
            {
                entry.Value.SetActive(entry.Key.isActive);
                if (entry.Key.isActive)
                {
                    currentWeapon = entry.Key;
                }
            }

            playerCamera = cameraHolder.GetComponentInChildren<Camera>();
            audioSource = GetComponentInParent<AudioSource>();
        }

        public void Shot()
        {

            if (currentWeapon.currentAmo <= 0)
            {
                Debug.Log("no more amo");
            }
            else
            {
                currentWeapon.decreaseAmo();
                // Notify ammo decrease event
                this.NotifyObservers(WeaponNotificationType.WEAPON_AMMO_UPDATE, currentWeapon.currentAmo, -1);

                audioSource.PlayOneShot(weaponsSound[currentWeapon.weaponSoundIndex]);
                RaycastHit hit;
                Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out hit, 100);
                if (hit.collider != null)
                {
                    Debug.Log(hit.collider);
                 
                    if (hit.collider.gameObject.GetComponentInParent<PlayerStates>() != null)
                    {
                       
                        if (hit.collider.gameObject.GetComponentInParent<PlayerStates>() != null)
                        {
                            PhotonView targetPhotonView = hit.collider.gameObject.GetComponentInParent<PhotonView>();
                            if (targetPhotonView != null)
                            {
                                // Appeler le RPC pour synchroniser la santé
                                targetPhotonView.RPC("RPC_DecreaseHealth", RpcTarget.All, currentWeapon.damage, playerStates.character.nickname);
                            }
                        }
                    }
                }
            }
        }
        

        /// <summary>
        /// Increases the ammunition count for the specified weapon by a fixed amount.
        /// </summary>
        /// <remarks>If the specified weapon is not found in the weapons dictionary, no ammunition is
        /// added and a warning is logged.</remarks>
        /// <param name="weaponName">The name of the weapon for which to increase the ammunition. Must correspond to a weapon present in the
        /// weapons dictionary.</param>
        public void increaseAmmo(string weaponName)
        {
            // Find the weapon in the dictionary by its name
            var weaponEntry = weaponsDictionary.FirstOrDefault(entry => entry.Key.name == weaponName);

            if (weaponEntry.Key != null)
            {
                weaponEntry.Key.increaseAmo(20);
                this.NotifyObservers(WeaponNotificationType.WEAPON_AMMO_UPDATE, currentWeapon.currentAmo, -1);
            }
            else
            {
                Debug.LogWarning($"Weapon with name {weaponName} not found in the dictionary.");
            }
        }

        /// <summary>
        /// Cycles to the next available weapon, updating the active weapon and its associated game object state.
        /// </summary>
        /// <remarks>If the currently active weapon is the last in the collection, this method wraps
        /// around to activate the first weapon. Only one weapon will be active after calling this method. This method
        /// is typically used to allow players to switch weapons in sequence during gameplay.</remarks>
        public void ChangeWeapon()
        {
            Weapon activeWeapon = weaponsDictionary.FirstOrDefault(x => x.Key.isActive == true).Key;
            int activeIndex = activeWeapon.id + 1 >= weaponsDictionary.Keys.Count ? 0 : activeWeapon.id + 1;
           

            foreach (KeyValuePair<Weapon, GameObject> entry in weaponsDictionary)
            {
                if (activeIndex == entry.Key.id)
                {
                    entry.Key.isActive = true;
                    currentWeapon = entry.Key;
                }
                else
                {
                    entry.Key.isActive = false;
                }

                entry.Value.gameObject.SetActive(entry.Key.isActive);
            }
            this.NotifyObservers(WeaponNotificationType.WEAPON_CHANGE, currentWeapon.currentAmo, activeIndex);
        }

        public void RegisterObserver(IWeaponUIObserver observer)
        {
            GameManager.weaponUIObservers.Add(observer);
        }

        public void RemoveObserver(IWeaponUIObserver observer)
        {
            GameManager.weaponUIObservers.Remove(observer);
        }

        public void NotifyObservers(WeaponNotificationType type, int currentAmmo, int index)
        {
            foreach (var obs in GameManager.weaponUIObservers)
            {
                obs.OnNotify(type, currentAmmo, index);
            }
        }
    }

}
