using System.Collections.Generic;
using UnityEngine;
using Core.Interface.WeaponUI;
using Core.Utils;
using UnityEngine.UI;
using Manager;

namespace Game.Shared.UI.WeaponUI
{
    public class WeaponUI : MonoBehaviour, IWeaponUIObserver
    {
        public List<Sprite> weaponIcons = new List<Sprite>();

        Text ammoAmount;
        Image weaponIcon;

        // Start is called before the first frame update
        void Start()
        {
          
            GameManager.weaponUIObservers.Add(this);
            ammoAmount = GetComponentInChildren<Text>();
            weaponIcon = GetComponent<Image>();
        }

        public void OnNotify(Constant.WeaponNotificationType type, int currentAmmo, int index)
        {   
            if (type.Equals(Constant.WeaponNotificationType.WEAPON_CHANGE))
            {
                weaponIcon.sprite = weaponIcons[index];
                ammoAmount.text = currentAmmo.ToString();
            } else  if (type.Equals(Constant.WeaponNotificationType.WEAPON_AMMO_UPDATE))
            {
                ammoAmount.text = currentAmmo.ToString();
               
            }
        }
    }
}