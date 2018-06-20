using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Web3 {
public class Web3Mgr : MonoBehaviour {
    static public Web3Mgr instance {
        get; private set;
    }
    [HideInInspector] public Account Account {
        get; private set;
    }
    [HideInInspector] public RPC Rpc {
        get; private set;
    }

    public void Awake() {
		if (instance == null) {
			instance = this;
		} else if (this != instance) {
			DestroyImmediate(gameObject);
			return;
		}
		DontDestroyOnLoad(gameObject);

        Account = gameObject.GetComponent<Account>();
        Rpc = gameObject.GetComponent<RPC>();
    }
}
}
