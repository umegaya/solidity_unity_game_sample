using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEngine;

using Game.Eth.Util;

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
    public BigInteger Balance {
        get; set; 
    }
    public BigInteger TokenBalance {
        get; set;
    }
    public OnViewModelChange callback_;
    public RPC.RPCMgr RPCMgr {
        get { return Main.RPCMgr; }
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
        var myaddr = RPCMgr.Account.address_;
        while (true) {
            yield return RPCMgr.Eth["Inventory"].Call("getSlotSize", myaddr);
            var r = RPCMgr.Eth.CallResponse;
            if (r.Error != null) {
                Debug.LogError("Inventory.getSlotSize fails:" + r.Error.Message);    
                break;            
            } else {
                var rr = r.Result[0].Result;
                var slot_size = (BigInteger)rr;
                if (slot_size <= 0) {
                    Debug.Log("create initial deck");
                    yield return StartCoroutine(CreateInitialDeck());
                } else {
                    Debug.Log("Inventory getSlotSize:" + slot_size);
                    for (int i = 0; i < slot_size; i++) {
                        yield return RPCMgr.Eth["Inventory"].Call("getSlotBytesAndId", myaddr, i);
                        r = RPCMgr.Eth.CallResponse;
                        if (r.Error != null) {
                            Debug.LogError("Inventory.getSlotBytes fails:" + r.Error.Message);
                        } else {
                            var id = (System.Numerics.BigInteger)r.Result[0].Result;
                            var card = r.As<Ch.Card>(Ch.Card.Parser);
                            Debug.Log("Inventory.getSlotBytes card[" + id.ToString() + "]:" + card.Hp.ToNumber());
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
        var req = RPCMgr.Web.NewReq();
        yield return req.Call("balance", new Dictionary<string, object>{
            {"address", RPCMgr.Account.address_}
        });
        if (req.Error != null) {
            yield return new WaitForSeconds(1.0f);
            yield break;
        }
        var json = req.As<Dictionary<string, object>>();
        BigInteger bi;
        if (!BigInteger.TryParse((string)json["balance"], out bi)) {
            Debug.Log("fail to parse as biginteger:" + (string)json["balance"]);
            yield break;
        }
        Balance = bi;
        yield return RPCMgr.Eth["Moritapo"].Call("balanceOf", RPCMgr.Account.address_);
        TokenBalance = (BigInteger)RPCMgr.Eth.CallResponse.Result[0].Result;
        Debug.Log("new balance:" + TokenBalance + "(" + Balance + ")");
    }
    IEnumerator CreateInitialDeck(uint selected_idx = 0) {
        var req = RPCMgr.Web.NewReq();
        yield return req.Call("new_account", new Dictionary<string, object>{
            {"address", RPCMgr.Account.address_},
            {"selected_idx", selected_idx},
            {"iap_tx", new Dictionary<string, object> {
                {"id", RPCMgr.Account.address_}, //temporary uses own address
                {"coin_amount", 10000},
            }},
        });
        if (req.Error == null) {
            Debug.Log("create account: created");
            yield return StartCoroutine(UpdateBalance());
        } else {
            Debug.LogError("World.createInitialCard fails:" + req.Error.Message);
            yield return new WaitForSeconds(1.0f);
        }                    
    }
}
}
