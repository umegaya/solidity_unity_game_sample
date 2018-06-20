using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game {
public class Main : MonoBehaviour {
    static public Web3.Web3Mgr Web3Mgr {
        get { return Web3.Web3Mgr.instance; }
    }
    static public ViewModel.ViewModelMgr ViewModelMgr {
        get { return ViewModel.ViewModelMgr.instance; }
    }
    static public UI.UIMgr UIMgr {
        get { return UI.UIMgr.instance; }
    }

    void Awake() {
        Web3Mgr.Rpc.callback_ += OnRpcEvent;
        ViewModelMgr.callback_ += OnViewModelEvent;
    }

    void OnRpcEvent(RPC.Web3.Event ev, object arg) {
        if (ev == RPC.Web3.Event.Inititalized) {
            StartCoroutine(ViewModelMgr.InititalizeTask());
        } else if (ev == RPC.Web3.Event.TxEvent) {
            var log = (Web3.Receipt.Log)arg;
            if (log.Name == "Transfer") {
                Debug.Log("TxEvent Happen:" + log.As<Web3.Event.Transfer>().ToString());
            } else if (log.Name == "Approval") {
                Debug.Log("TxEvent Happen:" + log.As<Web3.Event.Approval>().ToString());
            } else if (log.Name == "AddCat") {
                Debug.Log("TxEvent Happen:" + log.As<Web3.Event.AddCat>().ToString());
            } else if (log.Name == "Exchange") {
                Debug.Log("TxEvent Happen:" + log.As<Web3.Event.Exchange>().ToString());
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