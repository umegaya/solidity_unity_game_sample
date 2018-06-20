using System.Collections;
using System.Collections.Generic;

using UnityEngine;

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
        RPCMgr.Eth.callback_ += OnEthEvent;
        ViewModelMgr.callback_ += OnViewModelEvent;
    }

    void OnEthEvent(RPC.Eth.Event ev, object arg) {
        if (ev == RPC.Eth.Event.Inititalized) {
            StartCoroutine(ViewModelMgr.InititalizeTask());
        } else if (ev == RPC.Eth.Event.TxEvent) {
            var log = (Eth.Receipt.Log)arg;
            if (log.Name == "Transfer") {
                Debug.Log("TxEvent Happen:" + log.As<Eth.Event.Transfer>().ToString());
            } else if (log.Name == "Approval") {
                Debug.Log("TxEvent Happen:" + log.As<Eth.Event.Approval>().ToString());
            } else if (log.Name == "AddCard") {
                Debug.Log("TxEvent Happen:" + log.As<Eth.Event.AddCard>().ToString());
            } else if (log.Name == "Exchange") {
                Debug.Log("TxEvent Happen:" + log.As<Eth.Event.Exchange>().ToString());
            } else {
                Debug.LogError("invalid event log:" + log.Name);
            }
        }
    }
    void OnViewModelEvent(ViewModel.ViewModelMgr.Event ev, params object[] args) {
        Debug.Log("OnViewModelEvent:" + ev);
        if (ev == ViewModel.ViewModelMgr.Event.Initialized) {
            UIMgr.Open("Top");
        }
    }


}
}