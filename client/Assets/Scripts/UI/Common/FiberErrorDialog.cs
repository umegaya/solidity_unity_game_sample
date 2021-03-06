using System.Numerics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using uGUI = UnityEngine.UI;

namespace Game.UI {
class FiberErrorDialog : ErrorDialog {
    public enum CancelBehavior {
        GoTop,
        Abort,
    }
    public System.Func<IEnumerator> raise_;
    public System.Exception error_;
    public CancelBehavior behavior_;
    override protected void UpdateView() {
        text_.text = error_.Message;
    }
    override protected void OnOk() {
        Main.FiberMgr.Start(raise_);
        Main.UIMgr.PopDialog(gameObject);
    }
    override protected void OnCancel() {
        Main.FiberMgr.Stop(raise_);
        Main.UIMgr.PopDialog(gameObject);
        switch (behavior_) {
        case CancelBehavior.GoTop:
            UIMgr.instance.Open("Top");
            break;
        case CancelBehavior.Abort:
            Application.Quit();
            break;
        }
    }
}
}
