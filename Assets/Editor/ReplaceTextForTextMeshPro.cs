// Editor script to replace all Text components with TextMeshPro
// By VirtiaTheRed: https://www.reddit.com/r/Unity2D/comments/9etqii/replacing_existing_unityengineuitext_components/e5tqqhb/?utm_source=reddit&utm_medium=web2x&context=3

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System;
using System.Reflection;
using UnityEditor.SceneManagement;

public class ReplaceFont
{
    public Font OriginalFont;
    public TMP_FontAsset TargetFont;
}

public class ReplaceTextForTextMeshPro : EditorWindow
{
    private int currentSize = 0;
    public List<ReplaceFont> ReplaceFonts = new List<ReplaceFont>();
    private List<TextMeshProUGUI> m_needUpdate = new List<TextMeshProUGUI>();

    bool m_supposedToCheckTime = false;
    float m_time = 0.0f;

    [MenuItem("Tools/Text for TextMeshPro Component")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(ReplaceTextForTextMeshPro));
    }

    void OnGUI()
    {
        int newSize = EditorGUILayout.IntField("Number of Font Assets:", currentSize);
        if (newSize != currentSize)
        {
            currentSize = newSize;
            ReplaceFonts = new List<ReplaceFont>();
            for (int i = 0; i < currentSize; i++)
            {
                ReplaceFonts.Add(new ReplaceFont());
            }

        }

        foreach (ReplaceFont replaceFont in ReplaceFonts)
        {
            EditorGUILayout.BeginHorizontal();
            replaceFont.OriginalFont = (Font)EditorGUILayout.ObjectField(replaceFont.OriginalFont, typeof(Font), false);
            EditorGUILayout.PrefixLabel(" to: ");
            replaceFont.TargetFont = (TMP_FontAsset)EditorGUILayout.ObjectField(replaceFont.TargetFont, typeof(TMP_FontAsset), false);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Execute!"))
        {
            Text[] allTextObjects = Resources.FindObjectsOfTypeAll<Text>();
            Debug.Log("Total text field founds: " + allTextObjects.Length);

            m_needUpdate = new List<TextMeshProUGUI>();
            foreach (Text textObject in allTextObjects)
            {
                Undo.RegisterFullObjectHierarchyUndo(textObject.gameObject, "Text Undo");

                TMP_FontAsset textMeshProFont = GetTextMeshProFont(textObject);
                if (textMeshProFont == null)
                    continue;

                if (textObject.transform.parent.GetComponent<InputField>() || textObject.GetComponent<InputField>())
                {
                    Debug.LogWarning(string.Format("We can't mess with: {0} because {1} possible depend on the Text Component", textObject, FullPath(textObject.transform.parent.gameObject)), textObject);
                    continue;
                }

                GameObject tempObject = new GameObject();
                Text tempTextObject = tempObject.AddComponent<Text>(textObject);
                GameObject parentObject = textObject.gameObject;
                DestroyImmediate(textObject, true);

                TextMeshProUGUI textMeshPro = parentObject.GetComponent<TextMeshProUGUI>() ?? parentObject.gameObject.AddComponent<TextMeshProUGUI>();

                m_needUpdate.Add(textMeshPro);
                textMeshPro.font = textMeshProFont;
                textMeshPro.fontSize = tempTextObject.fontSize;
                textMeshPro.fontSizeMax = tempTextObject.resizeTextMaxSize;
                textMeshPro.enableAutoSizing = tempTextObject.resizeTextForBestFit;
                textMeshPro.alignment = GetAligmentFromTextObject(tempTextObject.alignment);
                textMeshPro.color = tempTextObject.color;
                textMeshPro.enableWordWrapping = tempTextObject.horizontalOverflow != HorizontalWrapMode.Wrap;
                textMeshPro.overflowMode = GetOverflowMode(tempTextObject.verticalOverflow);
                textMeshPro.text = tempTextObject.text;
                textMeshPro.richText = tempTextObject.supportRichText;

                DestroyImmediate(tempObject, true);
            }

            foreach (TextMeshProUGUI textMeshProUgui in m_needUpdate)
            {
                textMeshProUgui.enabled = false;
            }

            m_supposedToCheckTime = true;
        }
    }

    private TextOverflowModes GetOverflowMode(VerticalWrapMode verticalOverflow)
    {
        if (verticalOverflow == VerticalWrapMode.Truncate)
            return TextOverflowModes.Truncate;

        return TextOverflowModes.Overflow;
    }

    private TextAlignmentOptions GetAligmentFromTextObject(TextAnchor alignment)
    {
        if (alignment == TextAnchor.LowerCenter)
            return TextAlignmentOptions.Bottom;

        else if (alignment == TextAnchor.LowerLeft)
            return TextAlignmentOptions.BottomLeft;

        else if (alignment == TextAnchor.LowerRight)
            return TextAlignmentOptions.BottomRight;

        else if (alignment == TextAnchor.MiddleCenter)
            return TextAlignmentOptions.Midline;

        else if (alignment == TextAnchor.MiddleLeft)
            return TextAlignmentOptions.MidlineLeft;

        else if (alignment == TextAnchor.MiddleRight)
            return TextAlignmentOptions.MidlineRight;

        else if (alignment == TextAnchor.UpperCenter)
            return TextAlignmentOptions.Top;

        else if (alignment == TextAnchor.UpperLeft)
            return TextAlignmentOptions.TopLeft;

        else if (alignment == TextAnchor.UpperRight)
            return TextAlignmentOptions.TopRight;

        return TextAlignmentOptions.Center;
    }

    private TMP_FontAsset GetTextMeshProFont(Text textObject)
    {
        foreach (ReplaceFont replaceFont in ReplaceFonts)
        {
            if (replaceFont.OriginalFont == textObject.font)
                return replaceFont.TargetFont;
        }
        return null;
    }


    private void Update()
    {
        if (m_supposedToCheckTime)
        {
            m_time += 0.01f;

            if (m_time >= 3.0f)
            {
                //make sure you reset your time
                m_time = 0.0f;
                m_supposedToCheckTime = false;

                //TODO: take action

                foreach (TextMeshProUGUI textMeshProUgui in m_needUpdate)
                {
                    textMeshProUgui.enabled = true;
                }
                FindMissingReferencesInCurrentScene();
            }
        }
    }

    private static void FindMissingReferences(string context, GameObject[] objects)
    {
        foreach (var go in objects)
        {
            var components = go.GetComponents<Component>();

            foreach (var c in components)
            {
                if (!c)
                {
                    Debug.LogError("Missing Component in GO: " + FullPath(go), go);
                    continue;
                }

                SerializedObject so = new SerializedObject(c);
                var sp = so.GetIterator();

                while (sp.NextVisible(true))
                {
                    if (sp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (sp.objectReferenceValue == null
                            && sp.objectReferenceInstanceIDValue != 0)
                        {
                            ShowError(context, go, c.GetType().Name, ObjectNames.NicifyVariableName(sp.name));
                        }
                    }
                }
            }
        }
    }

    [MenuItem("Tools/Show Missing Object References in scene", false, 50)]
    public static void FindMissingReferencesInCurrentScene()
    {
        var objects = GetSceneObjects();
        FindMissingReferences(EditorSceneManager.GetActiveScene().name, objects);
    }

    private static GameObject[] GetSceneObjects()
    {
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(go => string.IsNullOrEmpty(AssetDatabase.GetAssetPath(go))
                   && go.hideFlags == HideFlags.None).ToArray();
    }

    private const string err = "Missing Ref in: [{3}]{0}. Component: {1}, Property: {2}";
    private const string CAUUTION_S = "We can't update: [{3}]{0}. Component: {1}, Property: {2}";

    private static void ShowError(string context, GameObject go, string c, string property)
    {
        Debug.LogError(string.Format(err, FullPath(go), c, property, context), go);
    }

    private static string FullPath(GameObject go)
    {
        return go.transform.parent == null
            ? go.name
                : FullPath(go.transform.parent.gameObject) + "/" + go.name;
    }

}


public static class TextReplacerExtensions
{
    public static T GetCopyOf<T>(this Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos)
        {
            if (pinfo.CanWrite)
            {
                try
                {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos)
        {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf(toAdd) as T;
    }
}