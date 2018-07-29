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
            public BaseRequest(ContractWrapper c, string func) { 
                cw_ = c; 
                Function = Contract.GetFunction(func);
            }
            private ContractWrapper cw_;
            public Contract Contract { get { return cw_.c_; } }
            public Eth Owner { get { return cw_.owner_; } }
            public double DefaultGas { get { return Owner.default_gas_; }}
            public System.Exception Error { get; set; }
            public Function Function { get; set; }
        }
        public class CallRequest : BaseRequest {
            EthCallUnityRequest req_;
            public List<ParameterOutput> Result { 
                get {
                    if (ResultCache_ == null) {
                        ResultCache_ = Function.DecodeResponse(req_.Result);
                    }
                    return ResultCache_;
                }
            }
            protected List<ParameterOutput> ResultCache_;

            public CallRequest(ContractWrapper cw, string func) : base(cw, func) {
                req_ = new EthCallUnityRequest(RPCMgr.instance.Account.chain_url_);
            }

            public M AsMsg<M>(Google.Protobuf.MessageParser<M> p, int at) where M : Google.Protobuf.IMessage<M> {
                return p.ParseFrom((byte[])Result[at].Result);
            }
            public M[] AsMsgs<M>(Google.Protobuf.MessageParser<M> p, int at) where M : Google.Protobuf.IMessage<M> {
                var bs = (List<byte[]>)Result[at].Result;
                M[] rets = new M[bs.Count];
                for (int i = 0; i < bs.Count; i++) {
                    rets[i] = p.ParseFrom(bs[i]);
                }
                return rets;
            }
            public T As<T>(int at) {
                return (T)Result[at].Result;
            }
            public T AsDTO<T>() where T : new() {
                return Function.DecodeDTOTypeOutput<T>(req_.Result);
            }

            public IEnumerator Exec(params object[] args) { 
                return Exec3(DefaultGas, 0, args); }
            public IEnumerator Exec2(double value_wei, params object[] args) { 
                return Exec3(DefaultGas, value_wei, args); }
            public IEnumerator Exec3(double gas, double value_wei, params object[] args) {
                yield return req_.SendRequest(
                    Function.CreateCallInput(RPCMgr.instance.Account.address_,
                        new HexBigInteger(new BigInteger(gas)), 
                        new HexBigInteger(new BigInteger(value_wei)), 
                        args), 
                    Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
            }
        }

        public class SendRequest : BaseRequest {
            TransactionSignedUnityRequest req_;
            public Nethereum.RPC.Eth.DTOs.TransactionReceipt Result { get; set; }

            public SendRequest(ContractWrapper cw, string func) : base(cw, func) {
                req_ = new TransactionSignedUnityRequest(RPCMgr.instance.Account.chain_url_,
                    RPCMgr.instance.Account.PrivateKey,
                    RPCMgr.instance.Account.address_);
            }
            public IEnumerator Exec(params object[] args) { 
                return Exec3(DefaultGas, 0, args); 
            }
            public IEnumerator Exec2(double value_wei, params object[] args) { 
                return Exec3(DefaultGas, value_wei, args); 
            }
            public IEnumerator Exec3(double gas, double value_wei, params object[] args) {
                Debug.Log("gas/value = " + gas + "|" + value_wei);
                yield return req_.SignAndSendTransaction(
                    Function.CreateTransactionInput(RPCMgr.instance.Account.address_, 
                        new HexBigInteger(new BigInteger(gas)), 
                        new HexBigInteger(new BigInteger(value_wei)), args));
                var receipt_waiter = new TransactionReceiptPollingRequest(RPCMgr.instance.Account.chain_url_);
                yield return receipt_waiter.PollForReceipt(req_.Result, 60);
                if (receipt_waiter.Exception != null) {
                    Error = receipt_waiter.Exception;
                    yield break;
                }
                Result = receipt_waiter.Result;
            }
        }


        public Contract c_;
        Eth owner_;

        public ContractWrapper(Eth owner, string abi, string addr) {
            owner_ = owner;
            c_ = new Contract(null, abi, addr);
        }
        public CallRequest Call(string fn) { return new CallRequest(this, fn); }
        public SendRequest Send(string fn) { return new SendRequest(this, fn); }

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
