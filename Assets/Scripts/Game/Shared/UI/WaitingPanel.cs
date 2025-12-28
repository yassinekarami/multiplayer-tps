using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using Core.Utils;

public class WaitingPanel : MonoBehaviour, IOnEventCallback
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
        if (eventCode == Constant.PunEventCode.updateWaitingPanelUIEventCode)
        {
            object[] data = (object[])photonEvent.CustomData;
            string newText = (string)data[0];
            waitingForPlayerText.text = newText;
        }
        else if (eventCode == Constant.PunEventCode.theGameIsReadyEventCode)
        {
            waitingBgButtons.SetActive(true);
        }
    }
}
