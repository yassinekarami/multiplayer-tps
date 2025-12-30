using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Core.Event;
using Core.Utils;
using ExitGames.Client.Photon;
using Photon.Realtime;

namespace Game.KillCount.UI
{

    /// <summary>
    /// class for color selector panel
    /// it containts method for choosing color
    /// </summary>
    public class ColorSelector : MonoBehaviour, IOnEventCallback
    {

        /// <summary>
        /// photonview instance attached to the colorSelectorPanel
        /// </summary>
        private PhotonView photonView;

        private bool hasSelectedColor = false;

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void Start()
        {
            photonView = GetComponent<PhotonView>();
        }

        /// <summary>
        /// red color button is clicked
        /// </summary>
        public void RedColorChoosed()
        {
            if (hasSelectedColor) return; // Empêche une double sélection

            hasSelectedColor = true; // Marque le joueur comme ayant sélectionné une couleur
            object[] content = new object[] { "RedButton", PhotonNetwork.LocalPlayer.NickName };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.colorHasBeenChooseEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// green color button is clicked
        /// </summary>
        public void GreenColorChoosed()
        {

            if (hasSelectedColor) return; // Empêche une double sélection

            hasSelectedColor = true; // Marque le joueur comme ayant sélectionné une couleur
            object[] content = new object[] { "GreenButton", PhotonNetwork.LocalPlayer.NickName };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.colorHasBeenChooseEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// orange color button is clicked
        /// </summary>
        public void OrangeColorChoosed()
        {
            if (hasSelectedColor) return; // Empêche une double sélection

            hasSelectedColor = true; // Marque le joueur comme ayant sélectionné une couleur
            object[] content = new object[] { "OrangeButton", PhotonNetwork.LocalPlayer.NickName };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.colorHasBeenChooseEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// purple color button is clicked
        /// </summary>
        public void PurpleColorChoosed()
        {
            if (hasSelectedColor) return; // Empêche une double sélection

            hasSelectedColor = true; // Marque le joueur comme ayant sélectionné une couleur
            object[] content = new object[] { "PurpleButton", PhotonNetwork.LocalPlayer.NickName };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.colorHasBeenChooseEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// blue color is clicked
        /// </summary>
        public void BlueColorChoosed()
        {
            if (hasSelectedColor) return; // Empêche une double sélection

            hasSelectedColor = true; // Marque le joueur comme ayant sélectionné une couleur
            object[] content = new object[] { "BlueButton", PhotonNetwork.LocalPlayer.NickName };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.colorHasBeenChooseEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }

        /// <summary>
        /// grey color is clicked
        /// </summary>
        public void GreyColorChoosed()
        {
            if (hasSelectedColor) return; // Empêche une double sélection

            hasSelectedColor = true; // Marque le joueur comme ayant sélectionné une couleur
            object[] content = new object[] { "GreyButton", PhotonNetwork.LocalPlayer.NickName };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(Constant.PunEventCode.colorHasBeenChooseEventCode, content, raiseEventOptions, SendOptions.SendReliable);
        }

        public void OnEvent(EventData photonEvent)
        {
            byte eventCode = photonEvent.Code;

            if (eventCode == Constant.PunEventCode.colorHasBeenChooseEventCode)
            {
                object[] data = (object[])photonEvent.CustomData;
                string button = (string)data[0];
                string playerName = (string)data[1];

                // Synchronise l'état des boutons pour tous les clients
                photonView.RPC("RPC_SetButtonInteractable", RpcTarget.AllBuffered, button, false);

                // Exécute la logique uniquement pour le joueur local
                if (playerName == PhotonNetwork.LocalPlayer.NickName)
                {
                    CreatePlayerEvent.onColorChoosed?.Invoke(playerName, ColorUtils.ResolveColorFromString(button));
                }
            }
        }


        /// <summary>
        /// Synchronizes the interactable state of a UI button across all clients in the networked session.
        /// </summary>
        /// <remarks>This method is intended to be called remotely via Photon Unity Networking (PUN) to
        /// ensure consistent UI state across all players. If the specified button is not found or does not have a
        /// Button component, no action is taken.</remarks>
        /// <param name="buttonName">The name of the button GameObject to update. Must correspond to an existing GameObject with a Button
        /// component.</param>
        /// <param name="isInteractable">A value indicating whether the button should be interactable. Set to <see langword="true"/> to enable
        /// interaction; otherwise, <see langword="false"/>.</param>
        [PunRPC]
        public void RPC_SetButtonInteractable(string buttonName, bool isInteractable)
        {
            GameObject buttonGameObject = GameObject.Find(buttonName);
            if (buttonGameObject != null && buttonGameObject.GetComponent<Button>())
            {
                buttonGameObject.GetComponent<Button>().interactable = isInteractable;
            }
        }
    }

}
