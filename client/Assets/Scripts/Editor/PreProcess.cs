using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

[InitializeOnLoadAttribute]
class MyCustomBuildProcessor
{
    static MyCustomBuildProcessor() {
        EditorApplication.playModeStateChanged += Callback;
    }

    static void Callback(PlayModeStateChange state) {
        if (state == PlayModeStateChange.EnteredEditMode) {
            string err;
            if (false == "make abi code".Sh(out err)) {
                Debug.Log("make abi error:" + err);
            } else {
                AssetDatabase.Refresh();
            }
        }
    }
}
