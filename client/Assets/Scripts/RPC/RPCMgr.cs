using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.RPC {
public class RPCMgr : MonoBehaviour {
    static public RPCMgr instance {
        get; private set;
    }
    [HideInInspector] public Web3.Account Web3.Account {
        get; private set;
    }
    [HideInInspector] public Web3 Web3 {
        get; private set;
    }
/*    [HideInInspector] public API API {
        get; private set;
    } */

    public void Awake() {
		if (instance == null) {
			instance = this;
		} else if (this != instance) {
			DestroyImmediate(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);

        Account = gameObject.GetComponent<Account>();
        Web3 = gameObject.GetComponent<Web3>();
        //API = gameObject.GetComponent<API>();
    }
}
}
