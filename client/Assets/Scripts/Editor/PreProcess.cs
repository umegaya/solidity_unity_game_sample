using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

[InitializeOnLoadAttribute]
public class MyCustomBuildProcessor
{
    static MyCustomBuildProcessor() {
        EditorApplication.playModeStateChanged += Callback;
    }

    public static void Callback(PlayModeStateChange state) {
        if (state == PlayModeStateChange.EnteredEditMode) {
            MyMenuItem.CodeGen();
        }
    }
}

public class MyMenuItem {
    [MenuItem("Assets/CodeGen")]
    public static void CodeGen() {
        string err;
        if (false == "make abi code".Sh(out err)) {
            Debug.Log("make abi error:" + err);
        } else {
            AssetDatabase.Refresh();
        }
    }
}