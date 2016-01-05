using UnityEngine;
using System.Collections;
using UnityEditor;
public class EditorStyleViewer : EditorWindow
{
    Vector2 scrollPosition = new Vector2(0, 0);
    string search = "";
    [MenuItem("Tools/EditorStyleViewer")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(EditorStyleViewer));
    }
    void OnGUI()
    {
        GUI.color = Color.red;
        GUILayout.BeginHorizontal("HelpBox");
        GUI.color = Color.white;
        GUILayout.Label("Click a Sample to copy its AssetName to your Clipboard", "MiniBoldLabel");
        GUILayout.FlexibleSpace();
        GUILayout.Label("Search:");
        search = EditorGUILayout.TextField(search);

        GUILayout.EndHorizontal();
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (GUIStyle style in GUI.skin.customStyles)
        {

            if (style.name.ToLower().Contains(search.ToLower()))
            {
                //GUI.color = Color.red;
                GUILayout.BeginHorizontal("PopupCurveSwatchBackground");
                //GUI.color = Color.white
                GUILayout.Space(7);
                if (GUILayout.Button(style.name, style))
                {
                    EditorGUIUtility.systemCopyBuffer = style.name;
                }
                GUILayout.FlexibleSpace();
                EditorGUILayout.SelectableLabel(style.name);
                GUILayout.EndHorizontal();
                GUILayout.Space(11);
            }
        }

        GUILayout.EndScrollView();
    }
}