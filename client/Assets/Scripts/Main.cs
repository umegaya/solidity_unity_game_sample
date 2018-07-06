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
    static Engine.FiberManager instance_ = null;
    static public Engine.FiberManager FiberMgr {
        get { 
            if (instance_ == null) {
                instance_ = new Engine.FiberManager();
            }
            return instance_;
        }
    }

    void Awake() {
        FiberMgr.Logger = Debug.Log;
        ViewModelMgr.subject_.
            Where(ev => ev.Type == ViewModel.ViewModelMgr.EventType.Ready).
            Subscribe(ev => OnViewModelReady(ev));
        FiberMgr.error_stream_. 
            Subscribe(f => OnFiberError(f.Item1, f.Item2));
    }

    void Update() {
        FiberMgr.Poll();
    }

    void OnViewModelReady(ViewModel.ViewModelMgr.Event ev) {
        Debug.Assert(ev.Type == ViewModel.ViewModelMgr.EventType.Ready);
        UIMgr.Open("Top");
    }

    void OnFiberError(System.Exception e, System.Func<IEnumerator> f) {
        var go = UIMgr.PushDialog("FiberErrorDialog");
        go.GetComponent<UI.FiberErrorDialog>().raise_ = f;
        go.GetComponent<UI.FiberErrorDialog>().error_ = e;
        go.GetComponent<UI.FiberErrorDialog>().behavior_ = UI.FiberErrorDialog.CancelBehavior.Abort;
    }
}
}