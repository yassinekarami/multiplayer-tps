using Core.Event;
using Core.Interface.PlayerInfoUI;
using Core.Interface.ScorePanelUI;
using Core.Interface.WeaponUI;
using Core.Model;
using Core.Utils;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;


namespace Manager
{
    public class GameManager : MonoBehaviourPunCallbacks, IInRoomCallbacks, IOnEventCallback
    {
        public static List<IPlayerInfoObserver> playerInfoObservers = new List<IPlayerInfoObserver>();
        public static List<IPlayerInfoSubject> playerInfoSubjects = new List<IPlayerInfoSubject>();

        public static List<IWeaponUIObserver> weaponUIObservers = new List<IWeaponUIObserver>();
        public static List<IWeaponUISubject> weaponUISubject = new List<IWeaponUISubject>();

        [SerializeField] private int maxPlayersInRoom = 20;
        public List<Character> characters = new List<Character>();


        [SerializeField][Tooltip("out game panel")] GameObject colorSelectorPanelUI = null;
        [SerializeField][Tooltip("out game panel")] GameObject waitingPanelUI = null;


        [SerializeField][Tooltip("in game panel")] GameObject inGamePanelUI = null;
        [SerializeField][Tooltip("in game panel")] static GameObject scorePanelUI = null;
        [SerializeField] GameObject level = null;
        public GameObject playerScore;

        List<GameObject> spawnPoints = new List<GameObject>();
        [SerializeField] List<GameObject> weaponSpawnPoints = new List<GameObject>();

        List<string> availableWeapons = new List<string>();

        private ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable();
        private float weaponSpawnTimer = 20;
        private bool gameHaveStarted = false;


        private void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.AddCallbackTarget(this);
            colorSelectorPanelUI = GameObject.Find("ColorSelectorPanel");
            waitingPanelUI = GameObject.Find("WaitingBG");
            inGamePanelUI = GameObject.Find("InGamePanel");
            scorePanelUI = GameObject.Find("ScorePanel");
            level = GameObject.Find("Level");

            spawnPoints.AddRange(GameObject.FindGameObjectsWithTag(Constant.Tag.SPAWN_POINT));
            weaponSpawnPoints.AddRange(GameObject.FindGameObjectsWithTag(Constant.Tag.WEAPON_SPAWN_POINT));
            availableWeapons.AddRange(new[] { "spawn/RL0N-25_low", "spawn/Sci-Fi Gun", "spawn/Bio Integrity Gun" });
        }

        private void Start()
        {
            customProperties["readyPlayers"] = 0;
            //level.SetActive(false);
            inGamePanelUI.SetActive(false);
            colorSelectorPanelUI.SetActive(true);
            waitingPanelUI.SetActive(false);

            PhotonNetwork.JoinRandomOrCreateRoom(null,roomOptions: setUpRoomOptions());
        }

        private void Update()
        {
            if(PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties != null &&
                (int)PhotonNetwork.CurrentRoom.CustomProperties["readyPlayers"] == maxPlayersInRoom)
            {
                weaponSpawnTimer -= Time.deltaTime;
                if (weaponSpawnTimer <= 0f)
                {
                    Queue<GameObject> queue = new Queue<GameObject>(weaponSpawnPoints);
                    foreach (string weapon in availableWeapons)
                    {
                        GameObject pointToBeSpawnIn = queue.Dequeue();
                        if (pointToBeSpawnIn.transform.childCount == 0)
                        {
                            GameObject obj = PhotonNetwork.Instantiate(weapon, pointToBeSpawnIn.transform.position, Quaternion.identity, 0);
                            obj.transform.parent = pointToBeSpawnIn.transform;
                        }
                    }
                    weaponSpawnTimer = 20f;
                }
            }
            
        }

        /// <summary>
        /// when a player join the room he create his character
        /// </summary>
        public override void OnJoinedRoom()
        {
            Debug.Log("Properties = " + PhotonNetwork.CurrentRoom.MaxPlayers);
            // joined a room successfully
            Debug.Log("joined room" + PhotonNetwork.CurrentRoom + " current player " + PhotonNetwork.CurrentRoom.PlayerCount);
            CreatePlayerEvent.onColorChoosed += CreateCharacter;
      
            if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayersInRoom)
            {
      
                Debug.Log("the game start , number of players " + PhotonNetwork.CurrentRoom.PlayerCount);

            }
        }

        /// <summary>
        /// send an event to display enter and exit button to enter or leave the arena
        /// </summary>
        private void SendTheGameIsReadyEvent()
        {
            object[] content = new object[] { true };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.theGameIsReadyEventCode, content,  raiseEventOptions, SendOptions.SendReliable);
            Debug.Log("SendTheGameIsReadyEvent is send with the code " + Constant.PunEventCode.theGameIsReadyEventCode);
            gameHaveStarted = true;
        }

        /// <summary>
        /// if the player cannot join a room , he create his own room
        /// </summary>
        /// <param name="returnCode"></param>
        /// <param name="message"></param>
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("joining room has failed, creating a new room");
            PhotonNetwork.CreateRoom(UnityEngine.Random.Range(0, 1000).ToString(), this.setUpRoomOptions());
        }

        /// <summary>
        /// set up room options with custom properties and max player in the room
        /// </summary>
        /// <returns></returns>
        private RoomOptions setUpRoomOptions()
        {
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = maxPlayersInRoom;

            // initial room properties
            Hashtable props = new Hashtable();
            props["readyPlayers"] = 0;

            // tell PUN which properties we want to sync
            roomOptions.CustomRoomProperties = props;

            return roomOptions; 
        }
        /// <summary>
        /// when the room is created
        /// </summary>
        public override void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom -> room exists now");

            ExitGames.Client.Photon.Hashtable props = new Hashtable();
            props["readyPlayers"] = 0;
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            Debug.Log("Room properties set: " + PhotonNetwork.CurrentRoom.CustomProperties["readyPlayers"]);
        }

        /// <summary>
        /// create an instance of character
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="color"></param>
        private void CreateCharacter(string nickname, Color color)
        {
            this.characters.Add(new Character(nickname, color.ToString(), 100)); ;
            Debug.Log("create new character "+characters.Count);
            colorSelectorPanelUI.SetActive(false);
            waitingPanelUI.SetActive(true);
            waitingPanelUI.GetComponent<WaitingPanel>().UpdateText(PhotonNetwork.CountOfPlayers.ToString(), maxPlayersInRoom.ToString());
            UpdateRoomReadyPlayer();
        }

        /// <summary>
        /// function to increase the number of ready players and set it in the room custom properties
        /// </summary>
        private void UpdateRoomReadyPlayer()
        {

            Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
            int currentReady = 0;
            if (props != null && props.ContainsKey("readyPlayers") )
            {
                currentReady = (int)props["readyPlayers"];
            }

            Hashtable updatedProps = new ExitGames.Client.Photon.Hashtable();
            updatedProps["readyPlayers"] = currentReady + 1;

            PhotonNetwork.CurrentRoom.SetCustomProperties(updatedProps);
        }

        /// <summary>
        /// detect room properties updated
        /// </summary>
        /// <param name="propertiesThatChanged"></param>
        public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
        {
            if (!gameHaveStarted)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties != null &&
                (int)PhotonNetwork.CurrentRoom.CustomProperties["readyPlayers"] == maxPlayersInRoom)
                {
                    Debug.Log("OnRoomPropertiesUpdate :readyPlayers " + (int)PhotonNetwork.CurrentRoom.CustomProperties["readyPlayers"]);

                    SendTheGameIsReadyEvent();
                }
            }
            else
            {
                //before instantiating the playerScore, we need to remove all the scorePanelUI's childrens
                foreach (Transform child in scorePanelUI.transform)
                {
                    Destroy(child.gameObject);
                }
                float yOffset = -30; // distance between each item
                int index = 0;
                foreach (DictionaryEntry entry in PhotonNetwork.CurrentRoom.CustomProperties)
                {
                    if (!entry.Key.Equals("readyPlayers"))
                    {
                        playerScore.transform.GetChild(0).GetComponent<Text>().text = (string)entry.Key;
     
                        playerScore.transform.GetChild(1).GetComponent<Text>().text = (string)entry.Value;

                        // Apply vertical offset
                        playerScore.GetComponent<RectTransform>().anchoredPosition +=
                            new Vector2(0, index * yOffset);

                        GameObject obj = PhotonNetwork.Instantiate("playerScore", scorePanelUI.transform.position, Quaternion.identity, 0);
                        obj.transform.parent = scorePanelUI.transform;

                        index++;
                    } 
                }
              

              

                Debug.Log("OnRoomPropertiesUpdate : other property changed");
            }
           
        }
        /// <summary>
        /// enable the level and instantiate the character through the network
        /// </summary>
        public void EnableLevel()
        {
            level.SetActive(true);
            waitingPanelUI.SetActive(false);

            foreach (Character character in characters)
            {
                object[] data = new object[]
                {
                    character.color.ToString(), character.nickname
                };
                Queue<GameObject> queue = new Queue<GameObject>(spawnPoints);
                GameObject obj = PhotonNetwork.Instantiate("Ybot", queue.Dequeue().transform.position, Quaternion.identity, 0, data);
            }
     //       Player[] players = PhotonNetwork.PlayerList;
     //       InstantiateScorePanelForeachPlayer(players);
            inGamePanelUI.SetActive(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="photonEvent"></param>
        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;
            if (eventCode == Constant.PunEventCode.roomPropertiesHaveToBeUpdatedEventCode)
            {
                object[] data = (object[])photonEvent.CustomData;
                string nickname = (string)data[0];
                int score = (int)data[1];
                ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
                if (properties != null)
                {
                    if (properties.ContainsKey(nickname))
                    {
                        Debug.Log("Score Updated: " + properties[nickname]);
                        properties[nickname] = score.ToString();
                    }
                    else
                    {
                        properties.Add(nickname, score.ToString());
                    }
                    PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
                }
            }
            //if (eventCode == Constant.PunEventCode.playerPropertiesUpdateEventCode)
            //{
            //    Debug.Log("playerPropertiesUpdateEventCode received");

            //    if (photonEvent.CustomData is object[] data && data.Length > 0)
            //    {
            //        Debug.Log(data);
            //        Debug.Log(data[0]);

            //        if (data[0] is Dictionary<object, object> dictionary)
            //        {
            //            foreach (var key in dictionary.Keys)
            //            {
            //                Debug.Log($"Key: {key}, Value: {dictionary[key]}");
            //                   // Appeler la méthode pour instancier le panneau de score
            //                InstantiateScorePanelForeachPlayer((string)key, (string)dictionary[key]);
            //            }

            //        }
            //        else
            //        {
            //            Debug.LogError("data[0] is not a Dictionary<object, object>");
            //        }
            //    }
            //    else
            //    {
            //        Debug.LogError("photonEvent.CustomData is not a valid object array or is empty.");
            //    }
            //}
        }

        /// <summary>
        /// instantiate nickname and health bar for each character present in the room
        /// </summary>
        /// <param name="characters"></param>
        public void InstantiateScorePanelForeachPlayer(string nickname, string score)
        {
            float yOffset = -30; // distance entre chaque élément
            int index = 0;

            Debug.Log($"Key: {nickname}, Value: {score}");

            // Vérifie si les composants nécessaires existent avant de les utiliser
            Text nicknameText = playerScore.transform.GetChild(0).GetComponent<Text>();
            Text scoreText = playerScore.transform.GetChild(1).GetComponent<Text>();

            if (nicknameText != null && scoreText != null)
            {
                GameObject toInstantiate = Instantiate(playerScore, gameObject.transform);
                toInstantiate.transform.parent = scorePanelUI.transform;

                // Applique un décalage vertical
                toInstantiate.GetComponent<RectTransform>().anchoredPosition +=
                    new Vector2(0, index * yOffset);

                index++;
            }
            else
            {
                Debug.LogError("Nickname or Score Text component is missing on the score GameObject.");
            }
        }

        public static void ToggleScorePanel()
        {
            if (scorePanelUI.activeSelf)
            {
                scorePanelUI.SetActive(false);
            }
            else
            {
                scorePanelUI.SetActive(true);
            }
        }
    }
}

