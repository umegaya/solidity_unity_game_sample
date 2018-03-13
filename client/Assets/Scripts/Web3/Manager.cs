using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Web3 {
public class Manager : MonoBehaviour {
    static public Manager instance {
        get; private set;
    }
    public Account Account {
        get; private set;
    }
    public RPC Rpc {
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
