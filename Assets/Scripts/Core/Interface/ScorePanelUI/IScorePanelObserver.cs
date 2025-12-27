using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Interface.ScorePanelUI
{
    public interface IScorePanelObserver
    {
        void OnScoreUpdated(int newScore);
    }
}
