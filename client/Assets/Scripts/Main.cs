using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Game.Web3Util;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Game {
public class Main : MonoBehaviour {
    static public Web3.Manager Web3Mgr {
        get { return Web3.Manager.instance; }
    }
    static public UI.Manager UIMgr {
        get { return UI.Manager.instance; }
    }

    void Awake() {
        Web3Mgr.Rpc.callback_ += OnWalletInititalized;
    }

    void OnWalletInititalized(Web3.RPC.Event ev, object arg) {
        //dispatch rpc to get current inventory status
        //for test, we first try createInitialCat
        StartCoroutine(OnWalletInititalizedTask());
    }

    IEnumerator OnWalletInititalizedTask() {
        var myaddr = Web3Mgr.Account.address_;
        while (true) {
            yield return Web3Mgr.Rpc["Inventory"].Call("getSlotSize", myaddr);
            var r = Web3Mgr.Rpc.CallResult;
            if (r.Exception != null) {
                Debug.LogError("Inventory.getSlotSize fails:" + r.Exception.Message);    
                break;            
            } else {
                var slot_size = r.Result.AsInt().Value;
                if (slot_size <= 0) {
                    yield return StartCoroutine(CreateInitialCat());
                } else {
                    Debug.Log("Inventory getSlotSize:" + slot_size);
                    for (int i = 0; i < slot_size; i++) {
                        yield return Web3Mgr.Rpc["Inventory"].Call("getSlotBytes", myaddr, i);
                        r = Web3Mgr.Rpc.CallResult;
                        if (r.Exception != null) {
                            Debug.LogError("Inventory.getSlotBytes fails:" + r.Exception.Message);
                        } else {
                            Debug.Log("Inventory.getSlotBytes bytes:" + r.Result);
                        }
                    }
                    break;
                }
            }
        }
    }
    IEnumerator CreateInitialCat() {
        yield return Web3Mgr.Rpc["World"].Send("createInitialCat", 0, "testcat", false);
        var r = Web3Mgr.Rpc.SendResult;
        if (r.Exception == null) {
            yield return Web3Mgr.Rpc.GetSelfBalance((balance) => {
                Debug.Log("new balance:" + balance);
            });
        } else {
            Debug.LogError("World.createInitialCat fails:" + r.Exception.Message);
        }                    
    }
}
}