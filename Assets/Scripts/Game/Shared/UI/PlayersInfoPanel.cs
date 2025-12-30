using Core.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Core.Utils;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Game.Shared.Gameplay;

using System.Linq;
using Core.Interface.PlayerInfoUI;
using Manager;

namespace Game.Shared.UI
{
    public class PlayersInfoPanel : MonoBehaviour, IOnEventCallback, IPlayerInfoObserver
    {
        public GameObject playerInfo;
        GameObject[] players;
        Dictionary<string, GameObject> playerInfoDictionary = new Dictionary<string, GameObject>();



        // Start is called before the first frame update

        /// <summary>
        /// Initializes the registration of the Character type with Photon for custom serialization.
        /// </summary>
        /// <remarks>This method is called by Unity when the script instance is being loaded. It ensures
        /// that the Character type can be correctly serialized and deserialized by Photon networking. This registration
        /// should occur before any network operations involving Character objects.</remarks>
        private void Awake()
        {
            PhotonPeer.RegisterType(typeof(Character),(byte)100, Character.Serialize, Character.Deserialize);
            
        }


        /// <summary>
        /// Registers the current object as a Photon callback target and broadcasts player character information to all
        /// clients when the component is enabled.
        /// </summary>
        /// <remarks>This method is typically called by Unity when the component becomes enabled. It
        /// ensures that the object receives Photon callback events and synchronizes player character data across the
        /// network. This method should not be called directly.</remarks>
        private void OnEnable()
        {
            GameManager.playerInfoObservers.Add(this);
            PhotonNetwork.AddCallbackTarget(this);
            players = GameObject.FindGameObjectsWithTag(Constant.Tag.PLAYER);
            List<object> content = new List<object>();
            foreach (GameObject player in players)
            {
                if (player != null && player.GetComponent<PlayerStates>())
                {
                    Debug.Log("OnEnable "+ player.GetComponent<PlayerStates>().character);
                    content.Add(player.GetComponent<PlayerStates>().character);
                }
            }

            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.setUpPlayerInfoPanelEventCode, content.ToArray(), raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// callBack to remove event
        /// </summary>
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }


        /// <summary>
        /// photon event handler
        /// </summary>
        /// <param name="photonEvent"></param>
        public void OnEvent(EventData photonEvent)
        {

            byte eventCode = photonEvent.Code;
            Debug.Log("onEvent received code " + eventCode);
            if (eventCode == Constant.PunEventCode.setUpPlayerInfoPanelEventCode)
            {
                object[] customData = (object[])photonEvent.CustomData;
              
                List<Character> characters = new List<Character>();
                foreach (object data in customData)
                {
                    characters.Add((Character)data);
                }
             
                InstantiateInfoForeachPlayer(characters);
            }
            else if (eventCode == Constant.PunEventCode.updatePlayerHealthUIEventCode)
            {
                object[] data = (object[])photonEvent.CustomData;
                string nickName = (string)data[0];
                float currentHealth = (float)data[1];

                var playerInfoEntry = playerInfoDictionary.FirstOrDefault(entry => entry.Key.Equals(nickName));
                if (playerInfoEntry.Key != null)
                {
                    playerInfoEntry.Value.GetComponentInChildren<Image>().fillAmount = currentHealth/100;
                }
                else
                {
                    Debug.LogWarning($"PlayerInfo  with nickName {nickName} not found in the dictionary.");
                }
            }

        }


        /// <summary>
        /// instantiate nickname and health bar for each character present in the room
        /// </summary>
        /// <param name="characters"></param>
        public void InstantiateInfoForeachPlayer(List<Character> characters)
        {
            float yOffset = -30; // distance between each item
            int index = 0;

            foreach (Core.Model.Character character in characters)
            {
                playerInfo.GetComponentInChildren<Text>().text = character.nickname;
                playerInfo.GetComponentInChildren<Image>().color = ColorUtils.ParseRGBA(character.color);
                GameObject toInstantiate = Instantiate(playerInfo, gameObject.transform);
                if (!playerInfoDictionary.ContainsKey(character.nickname))
                {
                    playerInfoDictionary.Add(character.nickname, toInstantiate);
                }
         

                // Apply vertical offset
                toInstantiate.GetComponent<RectTransform>().anchoredPosition +=
                    new Vector2(0, index * yOffset);

                index++;
            }
        }

        /// <summary>
        /// Handles notification events. Intended to be overridden in a derived class to provide custom notification
        /// handling logic.
        /// </summary>
        /// <exception cref="System.NotImplementedException">Thrown if the method is called on a base class instance where it has not been implemented.</exception>
        public void OnNotifyToModifyTheHealthBar(string nickName, float currentHealth)
        {
            Debug.Log("Player info panel notified of a change. event is sent with code "+ Constant.PunEventCode.updatePlayerHealthUIEventCode);
            object[] content = new object[] { nickName, currentHealth };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.updatePlayerHealthUIEventCode, content.ToArray(), raiseEventOptions, SendOptions.SendReliable);
        }
    }
}

