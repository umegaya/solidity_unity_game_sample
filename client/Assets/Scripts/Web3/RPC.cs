using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Contracts;

using UnityEngine;

namespace Game.Web3 {
public class RPC : MonoBehaviour {
    public enum Event {
        Inititalized,
        TxEvent,
    };
    public delegate void OnEventDelegate(Event ev, object arg);
    public class Target {
        public Contract c_;
        RPC owner_;

        public Target(RPC owner, string abi, string addr) {
            owner_ = owner;
            c_ = new Contract(null, abi, addr);
        }
        public IEnumerator Call(string func, params object[] args) { return Call(func, owner_.default_gas_, 0, args); }
        public IEnumerator Call2(string func, double value_wei, params object[] args) { return Call(func, owner_.default_gas_, value_wei, args); }
        public IEnumerator Call3(string func, double gas, double value_wei, params object[] args) {
            var fn = c_.GetFunction(func);
            yield return owner_.call_.SendRequest(
                fn.CreateCallInput(Manager.instance.Account.address_,
                    new HexBigInteger(new BigInteger(gas)), 
                    new HexBigInteger(new BigInteger(value_wei)), 
                    args), 
                Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
        }
        public IEnumerator Send(string func, params object[] args) { return Send(func, owner_.default_gas_, 0, args); }
        public IEnumerator Send2(string func, double value_wei, params object[] args) { return Send(func, owner_.default_gas_, value_wei, args); }
        public IEnumerator Send3(string func, double gas, double value_wei, params object[] args) {
            var fn = c_.GetFunction(func);
            yield return owner_.send_.SignAndSendTransaction(
                fn.CreateTransactionInput(Manager.instance.Account.address_, 
                    new HexBigInteger(new BigInteger(gas)), 
                    new HexBigInteger(new BigInteger(value_wei)), args));
        }
    }
    [System.Serializable] public struct TargetEntry {
        public string label_, address_;
        public TextAsset abi_;
    }

    public List<TargetEntry> target_entries_ = new List<TargetEntry>();
    public OnEventDelegate callback_;
    public double default_gas_ = 4000000;

    Dictionary<string, Target> targets_;
    EthGetBalanceUnityRequest get_balance_;
    EthBlockNumberUnityRequest block_number_;
    EthCallUnityRequest call_;
    TransactionSignedUnityRequest send_;
    
    public void Awake() {
        Manager.instance.Account.callback_ += OnAccountInitEvent;
    }

    void InitRPC() {
        targets_ = new Dictionary<string, Target>();
        foreach (var e in target_entries_) {
            targets_[e.label_] = new Target(this, e.abi_.text, e.address_);
        }
        var url = Manager.instance.Account.chain_url_;
        get_balance_ = new EthGetBalanceUnityRequest(url);
        block_number_ = new EthBlockNumberUnityRequest(url);
        call_ = new EthCallUnityRequest(url);
        send_ = new TransactionSignedUnityRequest(url, 
                    Manager.instance.Account.PrivateKey,
                    Manager.instance.Account.address_);
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
            // At the start of the script we are going to call getAccountBalance()
            // with the address we want to check, and a callback to know when the request has finished.
            StartCoroutine(GetSelfBalance((balance) => {
                Debug.Log("Eth Account Balance:" + balance);
                callback_(Event.Inititalized, balance);
            }));		            
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
        return GetBalance(Manager.instance.Account.address_, callback);
    }

    public Target this[string key] {
        get {
            Target t;
            return targets_.TryGetValue(key, out t) ? t : null;
        }
    }
    public EthCallUnityRequest CallResult {
        get { return call_; }
    }
    public TransactionSignedUnityRequest SendResult {
        get { return send_; }
    }

}
}
