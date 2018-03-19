using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Game.Web3Util;

namespace Game.ViewModel {
public class ViewModelMgr : MonoBehaviour {
    static public ViewModelMgr instance {
        get; private set;
    }
    public Inventory Inventory {
        get; private set;
    }
    public decimal Balance {
        get; set; 
    }
    public Web3.Web3Mgr Web3Mgr {
        get { return Main.Web3Mgr; }
    }

    public void Awake() {
		if (instance == null) {
			instance = this;
		} else if (this != instance) {
			DestroyImmediate(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);

        Inventory = new Inventory();
        Balance = 0;
    }

    public IEnumerator InititalizeTask() {
        var myaddr = Web3Mgr.Account.address_;
        while (true) {
            yield return Web3Mgr.Rpc["Inventory"].Call("getSlotSize", myaddr);
            var r = Web3Mgr.Rpc.Response;
            if (r.Error != null) {
                Debug.LogError("Inventory.getSlotSize fails:" + r.Error.Message);    
                break;            
            } else {
                var rr = r.Result[0].Result;
                var slot_size = (System.Numerics.BigInteger)rr;
                if (slot_size <= 0) {
                    yield return StartCoroutine(CreateInitialCat());
                } else {
                    Debug.Log("Inventory getSlotSize:" + slot_size);
                    for (int i = 0; i < slot_size; i++) {
                        yield return Web3Mgr.Rpc["Inventory"].Call("getSlotBytes", myaddr, i);
                        r = Web3Mgr.Rpc.Response;
                        if (r.Error != null) {
                            Debug.LogError("Inventory.getSlotBytes fails:" + r.Error.Message);
                        } else {
                            var cat = r.As<Neko.Cat>(Neko.Cat.Parser);
                            Debug.Log("Inventory.getSlotBytes cat:" + cat.Name);
                        }
                    }
                    break;
                }
            }
        }
    }
    IEnumerator UpdateBalance() {
        yield return Web3Mgr.Rpc.GetSelfBalance((balance) => {
            Debug.Log("new balance:" + balance);
            Balance = balance;
        });
    }
    IEnumerator CreateInitialCat() {
        yield return Web3Mgr.Rpc["World"].Send2("createInitialCat", 1e18, 0, "testcat", false);
        var r = Web3Mgr.Rpc.Response;
        if (r.Error == null) {
            yield return StartCoroutine(UpdateBalance());
        } else {
            Debug.LogError("World.createInitialCat fails:" + r.Error.Message);
        }                    
    }
}
}
