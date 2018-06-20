using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEngine;

using Game.Web3Util;

namespace Game.ViewModel {
public class ViewModelMgr : MonoBehaviour {
    public enum Event {
        Initialized,
    }
    public delegate void OnViewModelChange(Event ev, params object[] args);
    static public ViewModelMgr instance {
        get; private set;
    }
    public Inventory Inventory {
        get; private set;
    }
    public decimal Balance {
        get; set; 
    }
    public BigInteger TokenBalance {
        get; set;
    }
    public OnViewModelChange callback_;
    public Web3.Web3Mgr Web3Mgr {
        get { return Main.Web3Mgr; }
    }

    public void Awake() {
		if (instance == null) {
			instance = this;
		} else if (this != instance) {
			DestroyImmediate(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);

        Inventory = new Inventory();
        Balance = 0;
    }

    public IEnumerator InititalizeTask() {
        yield return StartCoroutine(UpdateBalance());
        var myaddr = Web3Mgr.Account.address_;
        while (true) {
            yield return Web3Mgr.Rpc["Inventory"].Call("getSlotSize", myaddr);
            var r = Web3Mgr.Rpc.CallResponse;
            if (r.Error != null) {
                Debug.LogError("Inventory.getSlotSize fails:" + r.Error.Message);    
                break;            
            } else {
                var rr = r.Result[0].Result;
                var slot_size = (BigInteger)rr;
                if (slot_size <= 0) {
                    Debug.Log("create initial cat");
                    yield return StartCoroutine(CreateInitialCat());
                } else {
                    Debug.Log("Inventory getSlotSize:" + slot_size);
                    for (int i = 0; i < slot_size; i++) {
                        yield return Web3Mgr.Rpc["Inventory"].Call("getSlotBytesAndId", myaddr, i);
                        r = Web3Mgr.Rpc.CallResponse;
                        if (r.Error != null) {
                            Debug.LogError("Inventory.getSlotBytes fails:" + r.Error.Message);
                        } else {
                            var id = (System.Numerics.BigInteger)r.Result[0].Result;
                            var card = r.As<Ch.Card>(Ch.Card.Parser);
                            Debug.Log("Inventory.getSlotBytes cat[" + id.ToString() + "]:" + card.Name);
                            Inventory.AddCard(id, card);
                        }
                    }
                    break;
                }
            }
        }
        callback_(Event.Initialized);
    }
    IEnumerator UpdateBalance() {
        yield return Web3Mgr.Rpc.GetSelfBalance((balance) => {
            Balance = balance;
        });
        yield return Web3Mgr.Rpc["Moritapo"].Call("balanceOf", Web3Mgr.Account.address_);
        TokenBalance = (BigInteger)Web3Mgr.Rpc.CallResponse.Result[0].Result;
        Debug.Log("new balance:" + TokenBalance + "(" + Balance + ")");
    }
    IEnumerator CreateInitialCat() {
        yield return Web3Mgr.Rpc["World"].Send2("payForInitialCat", 1e18, 0, "testcat");
        Debug.Log("create initial cat: created");
        var r = Web3Mgr.Rpc.SendResponse;
        if (r.Error == null) {
            yield return StartCoroutine(UpdateBalance());
        } else {
            Debug.LogError("World.createInitialCat fails:" + r.Error.Message + "|" + r.Error.InnerException.Message);
        }                    
    }
}
}
