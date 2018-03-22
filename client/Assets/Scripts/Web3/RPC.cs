using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Contracts;

using UnityEngine;

using Game.Web3Util;

namespace Game.Web3 {
public class RPC : MonoBehaviour {
    public enum Event {
        Inititalized,
        TxEvent,
    };
    public delegate void OnEventDelegate(Event ev);
    public class Target {
        public class CallResponse {
            public List<ParameterOutput> Result { get; set; }
            public System.Exception Error { get; set; }

            public M As<M>(Google.Protobuf.MessageParser<M> p) where M : Google.Protobuf.IMessage<M> {
                var len = (int)(System.Numerics.BigInteger)Result[1].Result;
                var bs = new byte[len];
                var ls = (List<object>)Result[0].Result;
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
        RPC owner_;

        public Target(RPC owner, string abi, string addr) {
            owner_ = owner;
            c_ = new Contract(null, abi, addr);
        }
        public IEnumerator Call(string func, params object[] args) { return Call3(func, owner_.default_gas_, 0, args); }
        public IEnumerator Call2(string func, double value_wei, params object[] args) { return Call3(func, owner_.default_gas_, value_wei, args); }
        public IEnumerator Call3(string func, double gas, double value_wei, params object[] args) {
            var fn = c_.GetFunction(func);
            yield return owner_.call_.SendRequest(
                fn.CreateCallInput(Web3Mgr.instance.Account.address_,
                    new HexBigInteger(new BigInteger(gas)), 
                    new HexBigInteger(new BigInteger(value_wei)), 
                    args), 
                Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
            ParseCallResponse(fn, owner_.call_);
        }
        public IEnumerator Send(string func, params object[] args) { return Send3(func, owner_.default_gas_, 0, args); }
        public IEnumerator Send2(string func, double value_wei, params object[] args) { return Send3(func, owner_.default_gas_, value_wei, args); }
        public IEnumerator Send3(string func, double gas, double value_wei, params object[] args) {
            var fn = c_.GetFunction(func);
            yield return owner_.send_.SignAndSendTransaction(
                fn.CreateTransactionInput(Web3Mgr.instance.Account.address_, 
                    new HexBigInteger(new BigInteger(gas)), 
                    new HexBigInteger(new BigInteger(value_wei)), args));
            yield return owner_.get_receipt_.SendRequest(
                owner_.send_.Result
            );
            ParseSendResponse(fn, owner_.get_receipt_);
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
        public void ParseSendResponse(Function fn, UnityRequest<Dictionary<string, object>> req) {
            try {
                var r = owner_.send_resp_;
                if (req.Exception != null) {
                    r.Error = req.Exception;
                    r.Result = null;
                } else {
                    var txr = new Receipt(req.Result);
                    txr.Dump();
                    r.Error = null;
                    r.Result = txr;
                }
            } catch (System.Exception ex) {
                Debug.Log("parseSendResposne error:" + ex.StackTrace);
            }
        }
    }
    [System.Serializable] public struct TargetEntry {
        public string label_, address_;
        public TextAsset abi_;
    }

    public List<TargetEntry> target_entries_ = new List<TargetEntry>();
    public OnEventDelegate callback_;
    public double default_gas_ = 4000000;

    public Newtonsoft.Json.JsonSerializerSettings settings_ = null;

    Dictionary<string, Target> targets_;
    EthGetBalanceUnityRequest get_balance_;
    EthBlockNumberUnityRequest block_number_;
    EthCallUnityRequest call_;
    TransactionSignedUnityRequest send_;
    GetTransactionReceiptRequest get_receipt_;

    Target.CallResponse call_resp_;
    Target.SendResponse send_resp_;
    
    public void Awake() {
        Web3Mgr.instance.Account.callback_ += OnAccountInitEvent;
        call_resp_ = new Target.CallResponse();
        send_resp_ = new Target.SendResponse();
    }

    void InitRPC() {
        targets_ = new Dictionary<string, Target>();
        foreach (var e in target_entries_) {
            targets_[e.label_] = new Target(this, e.abi_.text, e.address_);
        }
        var url = Web3Mgr.instance.Account.chain_url_;
        get_balance_ = new EthGetBalanceUnityRequest(url, settings_);
        block_number_ = new EthBlockNumberUnityRequest(url);
        call_ = new EthCallUnityRequest(url);
        send_ = new TransactionSignedUnityRequest(url, 
                    Web3Mgr.instance.Account.PrivateKey,
                    Web3Mgr.instance.Account.address_);
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
            InitRPC();
            callback_(Event.Inititalized);
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
        return GetBalance(Web3Mgr.instance.Account.address_, callback);
    }

    public Target this[string key] {
        get {
            Target t;
            return targets_.TryGetValue(key, out t) ? t : null;
        }
    }
    public Target.CallResponse CallResponse {
        get { return call_resp_; }
    }
    public Target.SendResponse SendResponse {
        get { return send_resp_; }
    }
}
}
