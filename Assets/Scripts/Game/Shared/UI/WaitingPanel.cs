using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Core.Utils;

public class WaitingPanel : MonoBehaviourPunCallbacks, IOnEventCallback
{

    [SerializeField]  Text waitingForPlayerText;
    [SerializeField]  GameObject waitingBgButtons;
   
    /// <summary>
    /// awake method
    /// </summary>
    void Awake()
    {
        waitingForPlayerText = GameObject.Find("Joined").GetComponent<Text>();
        waitingBgButtons = GameObject.Find("WaitingBgButtons");
    }

    private void Start()
    {
        waitingBgButtons.SetActive(false);
    }

    /// <summary>
    /// callBack to register event
    /// </summary>
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties["waitingMessage"] != null)
        {
            waitingForPlayerText.text = PhotonNetwork.CurrentRoom.CustomProperties["waitingMessage"].ToString();
        }

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
        if (eventCode == Constant.PunEventCode.theGameIsReadyEventCode)
        {
            waitingForPlayerText.text = "";
            waitingBgButtons.SetActive(true);
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        string waitingText = (string)propertiesThatChanged["waitingMessage"];
        waitingForPlayerText.text = waitingText;
    }

   
}
