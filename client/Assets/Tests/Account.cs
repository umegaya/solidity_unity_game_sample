using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Nethereum.JsonRpc.UnityClient;
using Nethereum.KeyStore;

using UnityEngine;

public class Account : MonoBehaviour {
	internal class AccountInitializer {
		internal Thread thread_;
		internal Account account_;
		internal string encryptedKeyStore_ = null;
		internal int result_ = 0;

		internal AccountInitializer(Account a, string encryptedKeyStore) {
			account_ = a;
			encryptedKeyStore_ = encryptedKeyStore;
			thread_ = new Thread(() => { 
				LoadOrCreateAccount(); 
			});
		}

		internal void Start() { thread_.Start(); }

		void LoadOrCreateAccount() {
			Nethereum.Signer.EthECKey key;
			var password = account_.password_;
			if (string.IsNullOrEmpty(encryptedKeyStore_)) {
				if (string.IsNullOrEmpty(password)) {
					Thread.MemoryBarrier();
					result_ = -1;
					return;
				}
				key = CreateAccount(password, out encryptedKeyStore_);
			} else {
				//generate ecKey from encrypted key store
				var service = new Nethereum.KeyStore.KeyStoreService();
				key = new Nethereum.Signer.EthECKey(
					service.DecryptKeyStoreFromJson(password, encryptedKeyStore_), 
					true);
			}
			account_.key_ = key;

			Thread.MemoryBarrier();
			result_ = 1;
		}

	}
	public enum AccountInitEvent {
		Start,
		EndSuccess,
		EndFailure,
	}
	public delegate void AccountInitCallback(AccountInitEvent ev);

	public const string KEY_PREFIX = "neko";
	public string password_;
	public string address_;
	public string chain_address_ = "http://localhost:9545";
	public bool remove_wallet_ = false;
	public AccountInitCallback callback_;


	AccountInitializer worker_;
	Nethereum.Signer.EthECKey key_ = null;

	// Use this for initialization
	void Start () {
		#if UNITY_EDITOR
		if (remove_wallet_) {
			Debug.Log("remove wallet data. this is unrecoverable.");
			PlayerPrefs.SetString(KEY_PREFIX + "_encrypted_key_store", "");
		}
		#endif
		var ks = PlayerPrefs.GetString(KEY_PREFIX + "_encrypted_key_store", "");
		callback_(AccountInitEvent.Start);
		worker_ = new AccountInitializer(this, ks);
		worker_.Start();
	}

	void Update() {
		if (worker_ != null) {
			Thread.MemoryBarrier();
			if (worker_.result_ != 0) {
				if (worker_.result_ < 0) {
					callback_(AccountInitEvent.EndFailure);
					#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
					#else
					Application.Quit();
					#endif
				} else {
					var eks = worker_.encryptedKeyStore_;
					if (!string.IsNullOrEmpty(eks)) {
						PlayerPrefs.SetString(KEY_PREFIX + "_encrypted_key_store", eks);
						PlayerPrefs.Save();					
					}
					//Get the public address (derivied from the public key)
					address_ = key_.GetPublicAddress();
					callback_(AccountInitEvent.EndSuccess);
					Debug.Log("wallet address:" + address_);
					// At the start of the script we are going to call getAccountBalance()
					// with the address we want to check, and a callback to know when the request has finished.
					StartCoroutine(getAccountBalance(address_, chain_address_, (balance) => {
						// When the callback is called, we are just going print the balance of the account
						Debug.Log("Eth Account Balance:" + balance);
					}));		
				}
				worker_ = null;
			}
		}
	}

	// We create the function which will check the balance of the address and return a callback with a decimal variable
	public static IEnumerator getAccountBalance (string address, string chain_address, System.Action<decimal> callback) {
		// Now we define a new EthGetBalanceUnityRequest and send it the testnet url where we are going to
		// check the address, in this case "https://kovan.infura.io".
		// (we get EthGetBalanceUnityRequest from the Netherum lib imported at the start)
		var getBalanceRequest = new EthGetBalanceUnityRequest (chain_address);
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

    static public Nethereum.Signer.EthECKey CreateAccount(string password, out string encryptedKeyStore)
    {
        //Generate a private key pair using SecureRandom
        var key = Nethereum.Signer.EthECKey.GenerateKey();

        //Create a store service, to encrypt and save the file using the web3 standard
        var service = new Nethereum.KeyStore.KeyStoreService();
        encryptedKeyStore = service.EncryptAndGenerateDefaultKeyStoreAsJson(
			password, key.GetPrivateKeyAsBytes(), key.GetPublicAddress());
        return key;
    }

    //original version
    /* public string CreateAccount(string password, string path)
    {
        //Generate a private key pair using SecureRandom
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
        //Get the public address (derivied from the public key)
        var address = ecKey.GetPublicAddress();

        //Create a store service, to encrypt and save the file using the web3 standard
        var service = new KeyStoreService();
        var encryptedKey = service.EncryptAndGenerateDefaultKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), address);
        var fileName = service.GenerateUTCFileName(address);
        //save the File
        using (var newfile = File.CreateText(Path.Combine(path, fileName)))
        {
            newfile.Write(encryptedKey);
            newfile.Flush();
        }

        return fileName;
    }*/
}
