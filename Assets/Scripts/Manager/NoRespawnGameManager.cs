using Core.Event;
using Core.Interface.PlayerInfoUI;
using Core.Interface.WeaponUI;
using Core.Model;
using Core.Utils;
using ExitGames.Client.Photon;
using Game.Shared.Gameplay;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Manager
{
    /// <summary>
    /// Manages the No Respawn game mode.
    /// </summary>
}
public class NoRespawnGameManager : MonoBehaviourPunCallbacks, IInRoomCallbacks, IOnEventCallback
{
    public static List<IPlayerInfoObserver> playerInfoObservers = new List<IPlayerInfoObserver>();
    public static List<IPlayerInfoSubject> playerInfoSubjects = new List<IPlayerInfoSubject>();

    public static List<IWeaponUIObserver> weaponUIObservers = new List<IWeaponUIObserver>();
    public static List<IWeaponUISubject> weaponUISubject = new List<IWeaponUISubject>();

    [SerializeField] private int maxPlayersInRoom = 2;
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
        scorePanelUI.SetActive(false);
        inGamePanelUI.SetActive(false);
        colorSelectorPanelUI.SetActive(false);
        waitingPanelUI.SetActive(false);

    }

    private void Update()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties != null &&
            PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
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
                        GameObject obj = PhotonNetwork.InstantiateRoomObject(weapon, pointToBeSpawnIn.transform.position, Quaternion.identity, 0);
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
        colorSelectorPanelUI.SetActive(true);
        CreatePlayerEvent.onColorChoosed += CreateCharacter;

    }

    /// <summary>
    /// send an event to display enter and exit button to enter or leave the arena
    /// </summary>
    private void SendTheGameIsReadyEvent()
    {
        object[] content = new object[] { true };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(Constant.PunEventCode.theGameIsReadyEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        Debug.Log("SendTheGameIsReadyEvent is send with the code " + Constant.PunEventCode.theGameIsReadyEventCode);
        gameHaveStarted = true;
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
        this.characters.Add(new Character(nickname, color.ToString(), 100));
        Debug.Log("create new character " + characters.Count);

        // Met à jour l'état du joueur comme prêt
        UpdatePlayerReadyState(true);

        // Cache le panneau de sélection de couleur
        colorSelectorPanelUI.SetActive(false);

        // Vérifie si tous les joueurs sont prêts
        //   CheckAllPlayersReady();
        waitingPanelUI.SetActive(true);
        UpdateRoomReadyPlayer();
    }

    /// <summary>
    /// function to increase the number of ready players and set it in the room custom properties
    /// </summary>
    private void UpdateRoomReadyPlayer()
    {

        Hashtable props = PhotonNetwork.CurrentRoom.CustomProperties;
        int currentReady = 0;
        if (props != null && props.ContainsKey("readyPlayers"))
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
                PhotonNetwork.CurrentRoom.MaxPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                Debug.Log("OnRoomPropertiesUpdate :readyPlayers " + (int)PhotonNetwork.CurrentRoom.CustomProperties["readyPlayers"]);

                SendTheGameIsReadyEvent();
            }
        }
        else
        {
           UpdateScorePanel();
           if(propertiesThatChanged.ContainsKey("killedPlayers"))
           {
               int killedPlayers = (int)propertiesThatChanged["killedPlayers"];
               if (killedPlayers == 1 )
               {
                   Debug.Log("End of the game");
                   
               }
            }
        }
    }

    private void UpdateScorePanel()
    {
        //before instantiating the playerScore, we need to remove all the scorePanelUI's childrens
        foreach (Transform child in scorePanelUI.transform)
        {
            Destroy(child.gameObject);
        }

        int index = 0;
        //foreach (DictionaryEntry entry in PhotonNetwork.CurrentRoom.CustomProperties)
        //{
        //    if (!entry.Key.Equals("readyPlayers") || !entry.Key.Equals("killedPlayers"))
        //    {
        //        InstantiateScorePanelForeachPlayer((string)entry.Key, (string)entry.Value, index);
        //        index++;
        //    }
        //}
        Debug.Log("OnRoomPropertiesUpdate : other property changed");
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
            GameObject obj = PhotonNetwork.Instantiate("Ybot", spawnPoints[characters.IndexOf(character)].transform.position, Quaternion.identity, 0, data);
        }

        inGamePanelUI.SetActive(true);
    }

    /// <summary>
    /// It trigger when an event is received
    /// it's executed only by the master client, and update the score of the killed player
    /// </summary>
    /// <param name="photonEvent"></param>
    public void OnEvent(EventData photonEvent)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            byte eventCode = photonEvent.Code;
            if (eventCode == Constant.PunEventCode.playerHaveBeenKilledEventCode)
            {
                object[] data = (object[])photonEvent.CustomData;
                string nickname = (string)data[0];
                int score = (int)data[1];
                string killedNickname = (string)data[2];

                ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.CurrentRoom.CustomProperties;
                if (properties != null)
                {
                    if (properties.ContainsKey("killedPlayers"))
                    {
                        properties["killedPlayers"] = (int)properties["killedPlayers"] - 1;
                    }
                    else
                    {
                        properties.Add("killedPlayers", (int)properties["killedPlayers"] - 1);
                    }
                    PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
                }
            }
        }
    }
    /// <summary>
    /// Instantiate a score panel for each player with their nickname and score.
    /// </summary>
    /// <param name="nickname"></param>
    /// <param name="score"></param>
    public void InstantiateScorePanelForeachPlayer(string nickname, string score, int index)
    {
        float yOffset = -30f; // négatif = vers le bas

        GameObject toInstantiate = Instantiate(playerScore, scorePanelUI.transform);

        Text nicknameText = toInstantiate.transform.GetChild(0).GetComponent<Text>();
        Text scoreText = toInstantiate.transform.GetChild(1).GetComponent<Text>();

        if (nicknameText == null || scoreText == null)
        {
            Debug.LogError("Nickname or Score Text component is missing on the score prefab.");
            Destroy(toInstantiate);
            return;
        }

        nicknameText.text = nickname;
        scoreText.text = score;

        RectTransform rt = toInstantiate.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0f, index * yOffset);
    }

    /// <summary>
    /// Calls an RPC to destroy a pickup item across the network using its view ID.
    /// </summary>
    /// <param name="viewId"></param>
    public void DestroyPickup(int viewId)
    {
        photonView.RPC("RPC_DestroyPickup", RpcTarget.MasterClient, viewId);
    }
    /// <summary>
    /// Find the gameObject with its viewId and destroy it across the network.
    /// </summary>
    /// <param name="viewId"></param>
    [PunRPC]
    public void RPC_DestroyPickup(int viewId)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PhotonView pv = PhotonView.Find(viewId);
        if (pv == null) return;
        GameObject toBeDestroyed = pv.gameObject;
        PhotonNetwork.Destroy(toBeDestroyed);
    }
    /// <summary>
    /// Toggles the visibility of the score panel user interface.
    /// </summary>
    /// <remarks>If the score panel is currently visible, this method hides it; if it is hidden, the
    /// method makes it visible. This method is typically used to show or hide the score panel in response to user
    /// actions, such as pressing a button.</remarks>
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

    private void UpdatePlayerReadyState(bool isReady)
    {
        Hashtable props = PhotonNetwork.LocalPlayer.CustomProperties;
        props["isReady"] = isReady;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}
