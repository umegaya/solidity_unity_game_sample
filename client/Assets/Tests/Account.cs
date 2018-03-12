using System.Collections;
using System.Collections.Generic;
// Here we import the Netherum.JsonRpc methods and classes.
using Nethereum.JsonRpc.UnityClient;
using UnityEngine;

public class Account : MonoBehaviour {

	// Use this for initialization
	void Start () {
		// At the start of the script we are going to call getAccountBalance()
		// with the address we want to check, and a callback to know when the request has finished.
		StartCoroutine(getAccountBalance("0x627306090abab3a6e1400e9345bc60c78a8bef57", (balance) => {
			// When the callback is called, we are just going print the balance of the account
			Debug.Log("Eth Account Balance:" + balance);
		}));
		
	}

	// We create the function which will check the balance of the address and return a callback with a decimal variable
	public static IEnumerator getAccountBalance (string address, System.Action<decimal> callback) {
		// Now we define a new EthGetBalanceUnityRequest and send it the testnet url where we are going to
		// check the address, in this case "https://kovan.infura.io".
		// (we get EthGetBalanceUnityRequest from the Netherum lib imported at the start)
		var getBalanceRequest = new EthGetBalanceUnityRequest ("http://localhost:9545");
		// Then we call the method SendRequest() from the getBalanceRequest we created
		// with the address and the newest created block.
		yield return getBalanceRequest.SendRequest(address, Nethereum.RPC.Eth.DTOs.BlockParameter.CreateLatest ());
		
		// Now we check if the request has an exception
		if (getBalanceRequest.Exception == null) {
			// We define balance and assign the value that the getBalanceRequest gave us.
			var balance = getBalanceRequest.Result.Value;
			// Finally we execute the callback and we use the Netherum.Util.UnitConversion
			// to convert the balance from WEI to ETHER (that has 18 decimal places)
			callback (Nethereum.Util.UnitConversion.Convert.FromWei(balance, 18));
		} else {
			// If there was an error we just throw an exception.
			throw new System.InvalidOperationException ("Get balance request failed");
		}

	}
}