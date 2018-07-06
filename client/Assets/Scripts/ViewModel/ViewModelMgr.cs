using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using UnityEngine;
using UniRx;

using Game.Eth.Util;

namespace Game.ViewModel {
public class ViewModelMgr : MonoBehaviour, Engine.IFiber {
    public enum EventType {
        Initialized,
        RPCError,
        Ready,
    }
    public struct Event {
        public EventType Type;
        public System.Exception Error;
    }
    static public ViewModelMgr instance {
        get; private set;
    }
    public CSV.Container GameData = null;
    public Inventory Inventory {
        get; private set;
    }
    public BigInteger Balance {
        get; set; 
    }
    public BigInteger TokenBalance {
        get; set;
    }
    public Subject<Event> subject_ = new Subject<Event>();
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
    public void Start() {
        RPCMgr.Eth.subject_.
            Where(ev => ev.Type == RPC.Eth.EventType.TxLog).
            Subscribe(ev => OnEthTxLog(ev));
        RPCMgr.Eth.subject_.
            Where(ev => ev.Type == RPC.Eth.EventType.Inititalized).
            Subscribe(ev => Refresh());
        //waiting both contract and csv initialization
        Observable.
            Zip(subject_.Where(ev => ev.Type == EventType.Initialized), 
                GameData.subject_, (left, right) => { return left; }).
            Subscribe(ev => subject_.OnNext(new Event { Type = EventType.Ready } ));
    }

    void OnEthTxLog(RPC.Eth.Event ev) {
        Debug.Assert(ev.Type == RPC.Eth.EventType.TxLog);
        var log = ev.Log;
        if (log.Name == "Transfer") {
            Debug.Log("TxEvent Happen:" + log.As<Eth.Event.Transfer>().ToString());
        } else if (log.Name == "Approval") {
            Debug.Log("TxEvent Happen:" + log.As<Eth.Event.Approval>().ToString());
        } else if (log.Name == "MintCard") {
            Debug.Log("TxEvent Happen:" + log.As<Eth.Event.AddCard>().ToString());
        } else if (log.Name == "Exchange") {
            Debug.Log("TxEvent Happen:" + log.As<Eth.Event.Exchange>().ToString());
        } else {
            Debug.LogError("invalid event log:" + log.Name);
        }
    }

    //idempotentially refresh game status
    public void Refresh() {
        Main.FiberMgr.Start(this);
    }
 
    public IEnumerator RunAsFiber() {
        yield return UpdateBalance();
        var myaddr = RPCMgr.Account.address_;
        yield return RPCMgr.Eth["Inventory"].Call("getSlotSize", myaddr);
        var r = RPCMgr.Eth.CallResponse;
        if (r.Error != null) {
            yield return Raise("InititalizeTask(getSlotSize)", r.Error);
        } else {
            var rr = r.Result[0].Result;
            var slot_size = (BigInteger)rr;
            if (slot_size <= 0) {
                Debug.Log("create initial deck");
                yield return CreateInitialDeck();
            } else {
                Debug.Log("Inventory getSlotSize:" + slot_size);
                for (int i = 0; i < slot_size; i++) {
                    yield return RPCMgr.Eth["Inventory"].Call("getSlotBytesAndId", myaddr, i);
                    r = RPCMgr.Eth.CallResponse;
                    if (r.Error != null) {
                        yield return Raise("InititalizeTask(getSlotBytesAndId)", r.Error);
                    } else {
                        var id = (System.Numerics.BigInteger)r.Result[0].Result;
                        var card = r.As<Ch.Card>(Ch.Card.Parser);
                        Debug.Log("Inventory.getSlotBytes card[" + id.ToString() + "]:" + card.SpecId);
                        Inventory.AddCard(id, card);
                    }
                }
            }
        }
        subject_.OnNext(new Event { Type = EventType.Initialized } );
    }
    IEnumerator UpdateBalance() {
        var req = RPCMgr.Web.NewReq();
        yield return req.Call("balance", new Dictionary<string, object>{
            {"address", RPCMgr.Account.address_}
        });
        if (req.Error != null) {
            yield return new Engine.FiberManager.Sleep(1.0f);
            yield break;
        }
        var json = req.As<Dictionary<string, object>>();
        BigInteger bi;
        if (!BigInteger.TryParse((string)json["balance"], out bi)) {
            Raise("UpdateBalance", 
                new System.Exception("fail to parse as biginteger:" + (string)json["balance"]));
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
            yield return UpdateBalance();
        } else {
            Raise("CreateInitialDeck", req.Error);
            yield return new Engine.FiberManager.Sleep(1.0f);
        }                    
    }

    System.Exception Raise(string msg, System.Exception e) {
        Debug.Log(msg + ": raises " + e.Message);
        subject_.OnNext(new Event{ Type = EventType.RPCError, Error = e});
        return e;
    }
}
}
