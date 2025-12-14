using Core.Model;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Core.Utils;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Game.Shared.Gameplay;

namespace Game.Shared.UI
{
    public class PlayersInfoPanel : MonoBehaviour, IOnEventCallback
    {
        public GameObject playerInfo;
        GameObject[] players;
        List<Character> playerCharater = new List<Character>();
        // Start is called before the first frame update

        private void Awake()
        {
            PhotonPeer.RegisterType(typeof(Character),(byte)100, Character.Serialize, Character.Deserialize);
        }

        /// <summary>
        /// callBack to remove event
        /// </summary>
        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }
        void Start()
        {
            //characters = new List<Character>();
            //characters.Add(new Character("toto", Color.red, 100));
            //characters.Add(new Character("titi", Color.blue, 100));
            //InstiateInfoForeachPlayer(characters);


        }
        private void OnEnable()
        {
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
      //      PhotonNetwork.RaiseEvent(Constant.PunEventCode.setUpPlayerInfoPanelEventCode, players[0].GetComponent<PlayerStates>().character, raiseEventOptions, SendOptions.SendReliable);
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.setUpPlayerInfoPanelEventCode, content.ToArray(), raiseEventOptions, SendOptions.SendReliable);
        }
        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// photon event handler
        /// </summary>
        /// <param name="photonEvent"></param>
        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            if (eventCode == Constant.PunEventCode.setUpPlayerInfoPanelEventCode)
            {
                object[] customData = (object[])photonEvent.CustomData;
              
                List<Character> characters = new List<Character>();
                foreach (object data in customData)
                {
                    characters.Add((Character)data);
                }
             

                 InstiateInfoForeachPlayer(characters);


            }

        }


        /// <summary>
        /// instanatiate nickname and healthbar for each character present in the room
        /// </summary>
        /// <param name="characters"></param>
        public void InstiateInfoForeachPlayer(List<Character> characters)
        {
            float yOffset = -30; // distance between each item
            int index = 0;

            foreach (Core.Model.Character character in characters)
            {
                // playerInfo.GetComponentInChildren<Text>().text =
                playerInfo.GetComponentInChildren<Text>().text = character.nickname;
                Debug.Log("character color " + character.color+ " color " + ColorUtils.ResolveColorFromString(character.color));
                playerInfo.GetComponentInChildren<Image>().color = ColorUtils.ParseRGBA(character.color);
                GameObject toInstantiate = Instantiate(playerInfo, gameObject.transform); ;
          


                // Apply vertical offset
                toInstantiate.GetComponent<RectTransform>().anchoredPosition +=
                    new Vector2(0, index * yOffset);

                index++;
            }
        }
    }
}

