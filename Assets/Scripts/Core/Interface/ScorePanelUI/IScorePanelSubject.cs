using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Interface.ScorePanelUI
{
    public interface IScorePanelSubject
    {
        void RegisterObserver(IScorePanelObserver observer);
        void RemoveObserver(IScorePanelObserver observer);
        void NotifyObservers();
    }
}

