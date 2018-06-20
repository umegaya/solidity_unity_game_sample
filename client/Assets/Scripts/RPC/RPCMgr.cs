using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.RPC {
public class RPCMgr : MonoBehaviour {
    static public RPCMgr instance {
        get; private set;
    }
    [HideInInspector] public Game.Eth.Account Account {
        get; private set;
    }
    [HideInInspector] public Eth Eth {
        get; private set;
    }
    [HideInInspector] public Web Web {
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

        Account = gameObject.GetComponent<Game.Eth.Account>();
        Eth = gameObject.GetComponent<Eth>();
        Web = gameObject.GetComponent<Web>();
    }
}
}
