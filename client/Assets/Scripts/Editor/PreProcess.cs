using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

[InitializeOnLoadAttribute]
class MyCustomBuildProcessor
{
    static MyCustomBuildProcessor() {
        EditorApplication.playModeStateChanged += Callback;
    }

    [MenuItem("Assets/Create Proto and Contract ABI")]
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
