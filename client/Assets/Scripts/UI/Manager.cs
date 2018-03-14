using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.UI {
public class Manager : MonoBehaviour {
    static public Manager instance {
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
    }


}
}
