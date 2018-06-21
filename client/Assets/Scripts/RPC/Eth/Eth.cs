using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Contracts;

using UnityEngine;

using Newtonsoft.Json;

using Game.Eth;
using Game.Eth.Util;

namespace Game.RPC {
public class Eth : MonoBehaviour {
    public enum Event {
        Inititalized,
        TxEvent,
    };
    public delegate void OnEventDelegate(Event ev, object arg);
    public class ContractWrapper {
        public class CallResponse {
            public List<ParameterOutput> Result { get; set; }
            public System.Exception Error { get; set; }

            public M As<M>(Google.Protobuf.MessageParser<M> p, int startIndex = 1) where M : Google.Protobuf.IMessage<M> {
                var len = (int)(System.Numerics.BigInteger)Result[startIndex + 1].Result;
                var bs = new byte[len];
                var ls = (List<object>)Result[startIndex].Result;
                for (int j = 0; j < len; j++) {
                    bs[j] = (byte)ls[j];
                }
                return p.ParseFrom(bs);
            }
        }

        public class SendResponse {
            public Receipt Result { get; set; }
            public System.Exception Error { get; set; }
        }

        public Contract c_;
        Eth owner_;

        public ContractWrapper(Eth owner, string abi, string addr) {
            owner_ = owner;
            c_ = new Contract(null, abi, addr);
        }
        public IEnumerator Call(string func, params object[] args) { return Call3(func, owner_.default_gas_, 0, args); }
        public IEnumerator Call2(string func, double value_wei, params object[] args) { return Call3(func, owner_.default_gas_, value_wei, args); }
        public IEnumerator Call3(string func, double gas, double value_wei, params object[] args) {
            var fn = c_.GetFunction(func);
            yield return owner_.call_.SendRequest(
                fn.CreateCallInput(RPCMgr.instance.Account.address_,
                    new HexBigInteger(new BigInteger(gas)), 
                    new HexBigInteger(new BigInteger(value_wei)), 
                    args), 
                Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
            ParseCallResponse(fn, owner_.call_);
        }
        public IEnumerator Send(string func, params object[] args) { return Send3(func, owner_.default_gas_, 0, args); }
        public IEnumerator Send2(string func, double value_wei, params object[] args) { return Send3(func, owner_.default_gas_, value_wei, args); }
        public IEnumerator Send3(string func, double gas, double value_wei, params object[] args) {
            Debug.Log("gas/value = " + gas + "|" + value_wei);
            var fn = c_.GetFunction(func);
            yield return owner_.send_.SignAndSendTransaction(
                fn.CreateTransactionInput(RPCMgr.instance.Account.address_, 
                    new HexBigInteger(new BigInteger(gas)), 
                    new HexBigInteger(new BigInteger(value_wei)), args));
            int retry = 0;
            do {
                if (retry > 0) {
                    if (retry > 100) {
                        break;
                    }
                    Debug.Log("retry get receipt:" + retry);
                    yield return new WaitForSeconds(0.5f);
                }
                yield return owner_.get_receipt_.SendRequest(
                    owner_.send_.Result
                );
                retry++;
            } while (ParseSendResponse(fn, owner_.get_receipt_) == 0);
        }

        public void ParseCallResponse(Function fn, UnityRequest<string> req) {
            var r = owner_.call_resp_;
            if (req.Exception != null) {
                r.Error = req.Exception;
                r.Result = null;
            } else {
                r.Error = null;
                r.Result = fn.DecodeResponse(req.Result);
            }
        }
        public int ParseSendResponse(Function fn, UnityRequest<Dictionary<string, object>> req) {
            try {
                var r = owner_.send_resp_;
                if (req.Exception != null) {
                    r.Error = req.Exception;
                    r.Result = null;
                    Debug.Log("parseSendResposne request error:" + r.Error.Message);
                    return -1;
                } else if (req.Result == null) {
                    Debug.Log("tx not returns");
                    return 0;
                } else {
                    var txr = new Receipt(req.Result, c_);
                    txr.Dump();
                    r.Error = null;
                    r.Result = txr;
                    for (int i = 0; i < txr.Logs.Count; i++) {
                        owner_.callback_(Event.TxEvent, new Receipt.Log(txr.Logs[i], c_));
                    }
                    return 1;
                }
            } catch (System.Exception ex) {
                Debug.Log("parseSendResposne error:" + ex.StackTrace);
                return 0;
            }
        }
    }
    [System.Serializable] public struct ContractEntry {
        public string label;
        public string address;
    }
    public TextAsset addresses_;

    public List<ContractEntry> contract_entries_;
    public OnEventDelegate callback_;
    public double default_gas_ = 4000000;

    public Newtonsoft.Json.JsonSerializerSettings settings_ = null;

    Dictionary<string, ContractWrapper> contracts_;
    EthGetBalanceUnityRequest get_balance_;
    EthBlockNumberUnityRequest block_number_;
    EthCallUnityRequest call_;
    TransactionSignedUnityRequest send_;
    GetTransactionReceiptRequest get_receipt_;

    ContractWrapper.CallResponse call_resp_;
    ContractWrapper.SendResponse send_resp_;
    
    public void Awake() {
        RPCMgr.instance.Account.callback_ += OnAccountInitEvent;
        call_resp_ = new ContractWrapper.CallResponse();
        send_resp_ = new ContractWrapper.SendResponse();
    }

    void Initialize() {
        contracts_ = new Dictionary<string, ContractWrapper>();
        contract_entries_ = JsonConvert.DeserializeObject<List<ContractEntry>>(addresses_.text);
            Debug.Log("addrs:" + contract_entries_.Count + " => " + addresses_.text);
        foreach (var e in contract_entries_) {
            Debug.Log("contract:" + e.label + " => " + e.address);
            var text_asset = Resources.Load<TextAsset>("Contracts/" + e.label);
            contracts_[e.label] = new ContractWrapper(this, text_asset.text, e.address);
        }
        var url = RPCMgr.instance.Account.chain_url_;
        get_balance_ = new EthGetBalanceUnityRequest(url, settings_);
        block_number_ = new EthBlockNumberUnityRequest(url);
        call_ = new EthCallUnityRequest(url);
        send_ = new TransactionSignedUnityRequest(url, 
                    RPCMgr.instance.Account.PrivateKey,
                    RPCMgr.instance.Account.address_);
        get_receipt_ = new GetTransactionReceiptRequest(url);
    }

    void OnAccountInitEvent(Account.InitEvent ev) {
        Debug.Log("OnAccountInitEvent:" + ev);
        switch (ev) {
        case Account.InitEvent.Start:
            break;
        case Account.InitEvent.EndFailure:
            break;
        case Account.InitEvent.EndSuccess:
            Initialize();
            callback_(Event.Inititalized, null);
            break;
        }
    }

	public IEnumerator GetBalance(string address, System.Action<decimal> callback) {
		yield return get_balance_.SendRequest(address, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest ());
		if (get_balance_.Exception == null) {
			var balance = get_balance_.Result.Value;
			callback(Nethereum.Util.UnitConversion.Convert.FromWei(balance, 18));
		} else {
			throw new System.InvalidOperationException ("Get balance request failed");
		}
	}
    public IEnumerator GetSelfBalance(System.Action<decimal> callback) {
        return GetBalance(RPCMgr.instance.Account.address_, callback);
    }

    public ContractWrapper this[string key] {
        get {
            ContractWrapper c;
            return contracts_.TryGetValue(key, out c) ? c : null;
        }
    }
    public ContractWrapper.CallResponse CallResponse {
        get { return call_resp_; }
    }
    public ContractWrapper.SendResponse SendResponse {
        get { return send_resp_; }
    }
}
}
