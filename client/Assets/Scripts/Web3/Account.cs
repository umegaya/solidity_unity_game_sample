using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Nethereum.KeyStore;

using UnityEngine;

namespace Game.Web3 {
public class Account : MonoBehaviour {
	//definitions
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
	public enum InitEvent {
		Start,
		EndSuccess,
		EndFailure,
	}
	public delegate void InitCallback(InitEvent ev);

	//variable
	public const string KEY_PREFIX = "neko";
	public string password_;
	public string address_;
	public string chain_url_ = "http://localhost:9545";
	public bool remove_wallet_ = false;
	public InitCallback callback_;


	AccountInitializer worker_;
	Nethereum.Signer.EthECKey key_ = null;

	//attr
	public string PrivateKey {
		get { return key_.GetPrivateKey(); }
	}

	// Use this for initialization
	void Start () {
		#if UNITY_EDITOR
		if (remove_wallet_) {
			Debug.Log("remove wallet data. this is unrecoverable.");
			PlayerPrefs.SetString(KEY_PREFIX + "_encrypted_key_store", "");
		}
		#endif
		var ks = PlayerPrefs.GetString(KEY_PREFIX + "_encrypted_key_store", "");
		callback_(InitEvent.Start);
		worker_ = new AccountInitializer(this, ks);
		worker_.Start();
	}

	void Update() {
		if (worker_ != null) {
			Thread.MemoryBarrier();
			if (worker_.result_ != 0) {
				if (worker_.result_ < 0) {
					callback_(InitEvent.EndFailure);
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
					callback_(InitEvent.EndSuccess);
					Debug.Log("wallet address:" + address_ + " pkey:" + key_.GetPrivateKey());
				}
				worker_ = null;
			}
		}
	}

    static Nethereum.Signer.EthECKey CreateAccount(string password, out string encryptedKeyStore)
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
}