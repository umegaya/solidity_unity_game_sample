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
    void Nop(Event ev, object arg) {}

    public TextAsset contract_abi_;
    public string contract_address_;
    public OnEventDelegate callback_;

    Contract api_;
    EthGetBalanceUnityRequest get_balance_;
    EthBlockNumberUnityRequest block_number_;
    EthCallUnityRequest call_;
    TransactionSignedUnityRequest send_;
    
    public void Start() {
        Manager.instance.Account.callback_ += OnAccountInitEvent;
        callback_ += Nop;
    }

    void InitRPC() {
        api_ = new Contract(null, contract_abi_.text, contract_address_);
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
			callback (Nethereum.Util.UnitConversion.Convert.FromWei(balance, 18));
		} else {
			throw new System.InvalidOperationException ("Get balance request failed");
		}
	}
    public IEnumerator GetSelfBalance(System.Action<decimal> callback) {
        return GetBalance(Manager.instance.Account.address_, callback);
    }

    public IEnumerator Call(string func, params object[] args) {
        var fn = api_.GetFunction(func);
        yield return call_.SendRequest(fn.CreateCallInput(args), 
            Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest());
    }
    public EthCallUnityRequest CallResult {
        get { return call_; }
    }

    public IEnumerator Send(string func, double gas, double value_wei, params object[] args) {
        var fn = api_.GetFunction(func);
        yield return send_.SignAndSendTransaction(
            fn.CreateTransactionInput(Manager.instance.Account.address_, 
                new HexBigInteger(new BigInteger(gas)), 
                new HexBigInteger(new BigInteger(value_wei)), args));
    }
    public TransactionSignedUnityRequest SendResult {
        get { return send_; }
    }
}
}
