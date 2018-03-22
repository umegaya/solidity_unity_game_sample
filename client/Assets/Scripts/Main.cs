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
        Web3Mgr.Rpc.callback_ += OnRpcInititalized;
    }

    void OnRpcInititalized(Web3.RPC.Event ev) {
        if (ev == Web3.RPC.Event.Inititalized) {
            StartCoroutine(ViewModelMgr.InititalizeTask());
        } else if (ev == Web3.RPC.Event.TxEvent) {
            Debug.Log("TxEvent Happen");
        }
    }


}
}