using System.Collections;
using System.Collections.Generic;

using Nethereum.Hex.HexTypes;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.JsonRpc.UnityClient;
using Nethereum.Contracts;

using UnityEngine;

namespace Game.Web3 {
public class RPC : MonoBehaviour {
    public TextAsset contract_abi_;
    public string contract_address_;
    Contract api_;
    EthGetBalanceUnityRequest get_balance_;
    EthBlockNumberUnityRequest block_number_;
    EthCallUnityRequest call_;
    TransactionSignedUnityRequest send_;
    
    public void Start() {
        Manager.instance.Account.callback_ += OnAccountInitEvent;
    }

    void InitRPC() {
        api_ = new Contract(null, contract_abi_.text, contract_address_);
        var url = Manager.instance.Account.chain_address_;
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
            StartCoroutine(GetAccountBalance(
                Manager.instance.Account.address_, 
                Manager.instance.Account.chain_address_, (balance) => {
                Debug.Log("Eth Account Balance:" + balance);
            }));		            
            break;
        }
    }

	public IEnumerator GetAccountBalance(string address, string chain_address, System.Action<decimal> callback) {
		yield return get_balance_.SendRequest(address, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest ());
		if (get_balance_.Exception == null) {
			var balance = get_balance_.Result.Value;
			callback (Nethereum.Util.UnitConversion.Convert.FromWei(balance, 18));
		} else {
			throw new System.InvalidOperationException ("Get balance request failed");
		}
	}


}
}