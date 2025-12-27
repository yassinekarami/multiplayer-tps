using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Core.Utils;
using Core.Interface.ScorePanelUI;

namespace Manager
{
    // Manages the score system in the game
    // manages player scores, updates UI, and handles score-related events
    // respawns players and updates scores based on game events
    public class ScoreManager : MonoBehaviour
    {


        // Start is called before the first frame update
        void Start()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
