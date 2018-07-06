using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Nethereum.KeyStore;

using UnityEngine;
using UniRx;

namespace Game.Eth {
public class Account : MonoBehaviour, Engine.IFiber, Engine.IYieldable {
	//definitions
	internal class AccountInitializer {
		internal Thread thread_;
		internal Account account_;
		internal string keyStore_ = null;
		internal bool encrypted_;
		internal int result_ = 0;

		internal AccountInitializer(Account a, 
									string keyStore, bool encrypted) {
			account_ = a;
			keyStore_ = keyStore;
			thread_ = new Thread(() => { 
				LoadOrCreateAccount(); 
			});
		}

		internal void Start() { thread_.Start(); }

		void LoadOrCreateAccount() {
			Nethereum.Signer.EthECKey key;
			var password = account_.password_;
			if (string.IsNullOrEmpty(keyStore_)) {
				if (encrypted_ && string.IsNullOrEmpty(password)) {
					Thread.MemoryBarrier();
					result_ = -1;
					return;
				}
				key = CreateAccount(password, encrypted_, out keyStore_);
			} else {
				//generate ecKey from encrypted key store
				if (encrypted_) {
					var service = new Nethereum.KeyStore.KeyStoreService();
					key = new Nethereum.Signer.EthECKey(
						service.DecryptKeyStoreFromJson(password, keyStore_), 
						true);
				} else {
					key = new Nethereum.Signer.EthECKey(keyStore_);
				}
			}
			account_.key_ = key;

			Thread.MemoryBarrier();
			result_ = 1;
		}

	}
	public enum EventType {
		InitStart,
		InitSuccess,
	}
	public struct Event {
		public EventType Type;
	}
	public enum Encyption {
		Unknown,
		On,
		Off,
	}

	//variable
	public const string KEY_PREFIX = "neko";
	public string password_;
	public string address_;
	public string chain_url_ = "http://localhost:9545";
	public bool encyption_ = false;
	public bool remove_wallet_ = false;
	public Subject<Event> subject_ = new Subject<Event>();


	AccountInitializer worker_;
	Nethereum.Signer.EthECKey key_ = null;

	//attr
	public string PrivateKey {
		get { return key_.GetPrivateKey(); }
	}
	public System.Exception Error {
		get; private set;
	}

	// Use this for initialization
	void Start () {
		#if UNITY_EDITOR
		if (remove_wallet_) {
			Debug.Log("remove wallet data. this is unrecoverable.");
			PlayerPrefs.SetString(KEY_PREFIX + "_key_store", "");
		}
		chain_url_ = chain_url_.EvalBackTick();
		#else
		encyption_ = false;
		#endif
		Encyption e = (Encyption)PlayerPrefs.GetInt(
			KEY_PREFIX + "_key_store_encyption", (int)Encyption.Unknown);
		if (e != Encyption.Unknown) {
			encyption_ = (e == Encyption.On);
		}
		Main.FiberMgr.Start(this);
	}

	void Update() {
		if (worker_ != null) {
			Thread.MemoryBarrier();
			if (worker_.result_ != 0) {
				if (worker_.result_ < 0) {
					Error = new System.Exception(
						"AccountInitializer fails with code:" + worker_.result_);
					#if UNITY_EDITOR
					UnityEditor.EditorApplication.isPlaying = false;
					#else
					Application.Quit();
					#endif
				} else {
					var ks = worker_.keyStore_;
					if (!string.IsNullOrEmpty(ks)) {
						PlayerPrefs.SetString(KEY_PREFIX + "_key_store", ks);
						PlayerPrefs.SetInt(KEY_PREFIX + "_key_store_encyption", 
							(int)(encyption_ ? Encyption.On : Encyption.Off));
						PlayerPrefs.Save();					
					}
					//Get the public address (derivied from the public key)
					address_ = key_.GetPublicAddress();
					subject_.OnNext(new Event {Type = EventType.InitSuccess});
					Debug.Log("wallet address:" + address_ + " pkey:" + key_.GetPrivateKey());
				}
				worker_ = null;
			}
		}
	}

    static Nethereum.Signer.EthECKey CreateAccount(
		string password, bool encryption, out string keyStore)
    {
        //Generate a private key pair using SecureRandom
        var key = Nethereum.Signer.EthECKey.GenerateKey();

        //Create a store service, to encrypt and save the file using the web3 standard
		if (encryption) {
			var service = new Nethereum.KeyStore.KeyStoreService();
			keyStore = service.EncryptAndGenerateDefaultKeyStoreAsJson(
				password, key.GetPrivateKeyAsBytes(), key.GetPublicAddress());
			return key;
		} else {
			keyStore = key.GetPrivateKey();
			return key;
		}
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

	//implements IFiber
	public IEnumerator RunAsFiber() {
		Error = null;
		var ks = PlayerPrefs.GetString(KEY_PREFIX + "_key_store", "");
		subject_.OnNext(new Event {Type = EventType.InitStart});
		worker_ = new AccountInitializer(this, ks, encyption_);
		worker_.Start();
		yield return this;
		if (Error != null) {
			yield return Error;
		}
	}

	//implements IYieldable
	public bool YieldDone() {
		return !string.IsNullOrEmpty(address_) || Error != null;
	}
}
}