using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MainSingleton))]
[CanEditMultipleObjects]
public class MainSingletonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MainSingleton script = (MainSingleton)target;


        if (GUILayout.Button("Camera Shake"))
        {
            script.ImpulseSource.GenerateImpulse();
        }

        DrawDefaultInspector();
    }
}
