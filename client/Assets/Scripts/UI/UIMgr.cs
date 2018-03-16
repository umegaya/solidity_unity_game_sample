using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.UI {
public class UIMgr : MonoBehaviour {
    static public UIMgr instance {
        get; private set;
    }

    public void Awake() {
		instance = this;
    }
}
}
