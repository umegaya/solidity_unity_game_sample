using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UniRx;

namespace Game {
public class Main : MonoBehaviour {
    static public RPC.RPCMgr RPCMgr {
        get { return RPC.RPCMgr.instance; }
    }
    static public ViewModel.ViewModelMgr ViewModelMgr {
        get { return ViewModel.ViewModelMgr.instance; }
    }
    static public UI.UIMgr UIMgr {
        get { return UI.UIMgr.instance; }
    }

    void Awake() {
        ViewModelMgr.subject_.
            Where(ev => ev.Type == ViewModel.ViewModelMgr.EventType.Ready).
            Subscribe(ev => OnViewModelReady(ev));
    }

    void OnViewModelReady(ViewModel.ViewModelMgr.Event ev) {
        Debug.Assert(ev.Type == ViewModel.ViewModelMgr.EventType.Ready);
        UIMgr.Open("Top");
    }
}
}