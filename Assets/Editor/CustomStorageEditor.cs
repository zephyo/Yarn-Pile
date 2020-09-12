using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CustomStorage))]
[CanEditMultipleObjects]
public class CustomStorageEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CustomStorage script = (CustomStorage)target;


        if (GUILayout.Button("Delete Save Data"))
        {
            script.DeleteSaveData();
        }

        DrawDefaultInspector();
    }
}