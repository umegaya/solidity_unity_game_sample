using System.Numerics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using uGUI = UnityEngine.UI;

namespace Game.UI {
class ErrorDialog : MonoBehaviour {
    public enum Caption {
        Ok,
        Cancel,
    }
    public uGUI.Text text_;
    public uGUI.Button[] buttons_;

    void Start() {
        buttons_[(int)Caption.Ok].GetComponent<uGUI.Button>().onClick.AddListener(OnOk);
        buttons_[(int)Caption.Cancel].GetComponent<uGUI.Button>().onClick.AddListener(OnCancel);
        UpdateView();
    }
    virtual protected void UpdateView() {}
    virtual protected void OnOk() {}
    virtual protected void OnCancel() {}
}
}
