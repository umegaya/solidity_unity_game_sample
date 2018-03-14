using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game {
public class Main : MonoBehaviour {
    static public Web3.Manager Web3Mgr {
        get { return Web3.Manager.instance; }
    }
    static public UI.Manager UIMgr {
        get { return UI.Manager.instance; }
    }

    void Start() {
        Web3Mgr.Rpc.callback_ += OnWalletInititalized;
    }

    void OnWalletInititalized(Web3.RPC.Event ev, object arg) {
        //dispatch rpc to get current inventory status
        //for test, we first try createInitialCat
        StartCoroutine(OnWalletInititalizedTask());
    }

    IEnumerator OnWalletInititalizedTask() {
        yield return Web3Mgr.Rpc.Send("createInitialCat", 4000000, 1e18, 0, "testcat", false);
        var r = Web3Mgr.Rpc.SendResult;
        if (r.Exception == null) {
            yield return Web3Mgr.Rpc.GetSelfBalance((balance) => {
                Debug.Log("new balance:" + balance);
            });
        } else {
            Debug.LogError("CreateInitialCat fails:" + r.Exception.Message);
        }
    }
}
}