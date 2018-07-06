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
    public Screen[] dialogs_;
    public GameObject active_;
    public List<GameObject> dialog_stack_;
    

    public void Awake() {
		instance = this;
        active_ = null;
    }

    public GameObject PushDialog(string name) {
        for (int i = 0; i < dialogs_.Length; i++) {
            if (dialogs_[i].name_ == name) {
                if (active_ != null) {
                    var cg = active_.GetComponent<CanvasGroup>();
                    if (cg != null) {
                        cg.interactable = false;
                    }
                }
                foreach (var s in dialog_stack_) {
                    var cg = s.GetComponent<CanvasGroup>();
                    if (cg != null) {
                        cg.interactable = false;
                    }
                }
                var go = Instantiate(dialogs_[i].prefab_);
                go.transform.SetParent(gameObject.transform, false);
                dialog_stack_.Add(go);
                return go;
            }   
        }
        Debug.LogError("screen not found:" + name);
        return null;
    }

    public void PopDialog(GameObject pop) {
        int last_idx = dialog_stack_.Count - 1;
        int find_idx = -1;
        for (int i = last_idx; i >= 0; i--) {
            var d = dialog_stack_[i];
            if (pop == d) {
                find_idx = i;
                break;
            }
        }
        dialog_stack_.RemoveAt(find_idx);
        Destroy(pop);
        var cg = dialog_stack_[dialog_stack_.Count - 1].GetComponent<CanvasGroup>();
        if (cg != null) {
            if (!cg.interactable) {
                cg.interactable = true;
            }
        }
    }

    public GameObject Open(string name) {
        for (int i = 0; i < screens_.Length; i++) {
            if (screens_[i].name_ == name) {
                if (active_ != null) {
                    Destroy(active_);
                    active_ = null;
                }
                var go = Instantiate(screens_[i].prefab_);
                go.transform.SetParent(gameObject.transform, false);
                active_ = go;
                return go;
            }
        }
        Debug.LogError("screen not found:" + name);
        return null;
    }
}
}
