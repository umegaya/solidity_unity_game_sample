using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;

using UnityEngine;
using UniRx;

using Newtonsoft.Json;

using Game.Eth;
using Game.Eth.Util;

namespace Game.RPC {
public class Eth : MonoBehaviour {
    public enum EventType {
        Inititalized,
        TxLog,
    };
    public struct Event {
        public EventType Type;
        public Receipt.Log Log;
    }
    public class ContractWrapper {
        public class BaseRequest {
            public BaseRequest(ContractWrapper c) { cw_ = c; }
            private ContractWrapper cw_;
            public Contract Contract { get { return cw_.c_; } }
            public Eth Owner { get { return cw_.owner_; } }
            public double DefaultGas { get { return Owner.default_gas_; }}
            public System.Exception Error { get; set; }
        }
        public class CallRequest : BaseRequest {
            EthCallUnityRequest req_;
            public List<ParameterOutput> Result { get; set; }

            public CallRequest(ContractWrapper cw) : base(cw) {
                req_ = new EthCallUnityRequest(RPCMgr.instance.Account.chain_url_);
            }

            public M As<M>(Google.Protobuf.MessageParser<M> p, int startIndex) where M : Google.Protobuf.IMessage<M> {
                return p.ParseFrom((byte[])Result[startIndex].Result);
            }
            public M[] AsArray<M>(Google.Protobuf.MessageParser<M> p, int startIndex) where M : Google.Protobuf.IMessage<M> {
                var bs = (List<byte[]>)Result[startIndex].Result;
                M[] rets = new M[bs.Count];
                for (int i = 0; i < bs.Count; i++) {
                    rets[i] = p.ParseFrom(bs[i]);
                }
                return rets;
            }
            public T As<T>(int startIndex) {
                return (T)Result[startIndex].Result;
            }

            public IEnumerator Exec(string func, params object[] args) { 
                return Exec3(func, DefaultGas, 0, args); }
            public IEnumerator Exec2(string func, double value_wei, params object[] args) { 
                return Exec3(func, DefaultGas, value_wei, args); }
            public IEnumerator Exec3(string func, double gas, double value_wei, params object[] args) {
                var fn = Contract.GetFunction(func);
                yield return req_.SendRequest(
                    fn.CreateCallInput(RPCMgr.instance.Account.address_,
                        new HexBigInteger(new BigInteger(gas)), 
                        new HexBigInteger(new BigInteger(value_wei)), 
                        args), 
                    Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
                ParseResponse(fn, req_);
            }
            protected void ParseResponse(Function fn, UnityRequest<string> req) {
                var r = this;
                if (req.Exception != null) {
                    r.Error = req.Exception;
                    r.Result = null;
                } else {
                    try {
                        r.Result = fn.DecodeResponse(req.Result);
                        r.Error = null;
                    } catch (System.Exception ex) {
                        r.Error = ex;
                        r.Result = null;
                    }
                }
            }
        }

        public class SendRequest : BaseRequest {
            TransactionSignedUnityRequest req_;
            public Receipt Result { get; set; }

            public SendRequest(ContractWrapper cw) : base(cw) {
                req_ = new TransactionSignedUnityRequest(RPCMgr.instance.Account.chain_url_,
                    RPCMgr.instance.Account.PrivateKey,
                    RPCMgr.instance.Account.address_);
            }
            public IEnumerator Exec(string func, params object[] args) { 
                return Exec3(func, DefaultGas, 0, args); 
            }
            public IEnumerator Exec2(string func, double value_wei, params object[] args) { 
                return Exec3(func, DefaultGas, value_wei, args); 
            }
            public IEnumerator Exec3(string func, double gas, double value_wei, params object[] args) {
                Debug.Log("gas/value = " + gas + "|" + value_wei);
                var fn = Contract.GetFunction(func);
                yield return req_.SignAndSendTransaction(
                    fn.CreateTransactionInput(RPCMgr.instance.Account.address_, 
                        new HexBigInteger(new BigInteger(gas)), 
                        new HexBigInteger(new BigInteger(value_wei)), args));
                var get_receipt = new GetTransactionReceiptRequest(RPCMgr.instance.Account.chain_url_);
                int retry = 0;
                do {
                    if (retry > 0) {
                        if (retry > 100) {
                            break;
                        }
                        Debug.Log("retry get receipt:" + retry);
                        yield return new Engine.FiberManager.Sleep(0.5f);
                    }
                    yield return get_receipt.SendRequest(
                        req_.Result
                    );
                    retry++;
                } while (ParseResponse(fn, get_receipt) == 0);
            }
            public int ParseResponse(Function fn, UnityRequest<Dictionary<string, object>> req) {
                try {
                    var r = this;
                    if (req.Exception != null) {
                        r.Error = req.Exception;
                        r.Result = null;
                        Debug.Log("parseSendResposne request error:" + r.Error.Message);
                        return -1;
                    } else if (req.Result == null) {
                        Debug.Log("tx not returns");
                        return 0;
                    } else {
                        var txr = new Receipt(req.Result, Contract);
                        txr.Dump();
                        r.Error = null;
                        r.Result = txr;
                        for (int i = 0; i < txr.Logs.Count; i++) {
                            Owner.subject_.OnNext(
                                new Event{ Type = EventType.TxLog, Log = new Receipt.Log(txr.Logs[i], Contract) }
                            );
                        }
                        return 1;
                    }
                } catch (System.Exception ex) {
                    Debug.Log("parseSendResposne error:" + ex.StackTrace);
                    var r = this;
                    r.Error = ex;
                    r.Result = null;
                    return 0;
                }
            }
        }


        public Contract c_;
        Eth owner_;

        public ContractWrapper(Eth owner, string abi, string addr) {
            owner_ = owner;
            c_ = new Contract(null, abi, addr);
        }
        public CallRequest Call() { return new CallRequest(this); }
        public SendRequest Send() { return new SendRequest(this); }

    }
    [System.Serializable] public struct ContractEntry {
        public string label;
        public string address;
    }
    public TextAsset addresses_;

    public List<ContractEntry> contract_entries_;
    public Subject<Event> subject_ = new Subject<Event>();
    public double default_gas_ = 4000000;

    public Newtonsoft.Json.JsonSerializerSettings settings_ = null;

    Dictionary<string, ContractWrapper> contracts_;

    public void Start() {
        RPCMgr.instance.Account.subject_.
            Where(ev => ev.Type == Account.EventType.InitSuccess).
            Subscribe(ev => OnAccountInitSuccess(ev));
    }

    void Initialize() {
        contracts_ = new Dictionary<string, ContractWrapper>();
        contract_entries_ = JsonConvert.DeserializeObject<List<ContractEntry>>(addresses_.text);
        foreach (var e in contract_entries_) {
            Debug.Log("contract:" + e.label + " => " + e.address);
            var text_asset = Resources.Load<TextAsset>("Contracts/" + e.label);
            contracts_[e.label] = new ContractWrapper(this, text_asset.text, e.address);
        }
    }

    void OnAccountInitSuccess(Account.Event ev) {
        Initialize();
        subject_.OnNext(new Event{ Type = EventType.Inititalized });
    }

	public IEnumerator GetBalance(string address, System.Action<BigInteger> callback) {
        var url = RPCMgr.instance.Account.chain_url_;
        var get_balance = new EthGetBalanceUnityRequest(url, settings_);
		yield return get_balance.SendRequest(address, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest ());
		if (get_balance.Exception == null) {
			var balance = get_balance.Result.Value;
			callback(balance);
		} else {
			throw new System.InvalidOperationException ("Get balance request failed");
		}
	}
    public IEnumerator GetSelfBalance(System.Action<BigInteger> callback) {
        return GetBalance(RPCMgr.instance.Account.address_, callback);
    }

    public ContractWrapper this[string key] {
        get {
            ContractWrapper c;
            return contracts_.TryGetValue(key, out c) ? c : null;
        }
    }
}
}
