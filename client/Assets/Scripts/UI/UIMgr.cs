using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Game.UI {
public class UIMgr : MonoBehaviour {
    [System.Serializable] public struct Screen {
        public string name_;
        public GameObject prefab_;
    }

    static public UIMgr instance {
        get; private set;
    }

    public Sprite[] catImages_;
    public Screen[] screens_;
    public GameObject active_;
    

    public void Awake() {
		instance = this;
        active_ = null;
    }

    public void Open(string name) {
        for (int i = 0; i < screens_.Length; i++) {
            if (screens_[i].name_ == name) {
                if (active_ != null) {
                    Destroy(active_);
                    active_ = null;
                }
                var go = Instantiate(screens_[i].prefab_);
                go.transform.SetParent(gameObject.transform, false);
                active_ = go;
                return;
            }
        }
        Debug.LogError("screen not found:" + name);
    }
}
}
