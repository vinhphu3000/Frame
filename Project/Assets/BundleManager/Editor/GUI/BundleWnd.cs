using UnityEngine;
using UnityEditor;
using System.Collections;


/// <summary>
/// 暂时没什么卵用
/// </summary>

public class BundleWnd : EditorWindow
{

    //[MenuItem("Resource Tools/Bundle Object Inspector")]
    static void Init()
    {
        EditorWindow.GetWindow<BundleWnd>("Bundles");
    }


    void OnGUI()
    {
        Rect curWindowRect = EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                // Create drop down
                Rect createBtnRect = GUILayoutUtility.GetRect(new GUIContent("Create"), EditorStyles.toolbarDropDown, GUILayout.ExpandWidth(false));
                if (GUI.Button(createBtnRect, "Create", EditorStyles.toolbarDropDown))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.DropDown(createBtnRect);
                }


                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Settings", EditorStyles.toolbarButton))
                    BuildSettingEditor.Show();
            }
            EditorGUILayout.EndHorizontal();


        }
        EditorGUILayout.EndVertical();
    }

}
