using System.Collections.Generic;
using System.IO;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;
using System.Collections;


[CustomEditor(typeof(ShaderList), true)]
public class ShaderListInspector : Editor
{
    private ReorderableList list;

    private void OnEnable()
    {
        list = new ReorderableList(serializedObject, serializedObject.FindProperty("mAllShaders"), true, true, true, true);
        list.drawElementCallback = DrawShaderElement;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        serializedObject.Update();
        list.DoLayoutList();
        serializedObject.ApplyModifiedProperties();

        ShaderList selfData = target as ShaderList;

        if (GUILayout.Button("Find All Shader"))
        {
            selfData.mAllShaders = FindAllShader();
            Debug.Log(" Shader Count " + selfData.mAllShaders.Count);
        }
    }


    private List<Shader> FindAllShader()
    {
        string[] files = Directory.GetFiles(Application.dataPath, "*.shader", SearchOption.AllDirectories);
        List<Shader> result = new List<Shader>();
        for (int i = 0; i < files.Length; i++)
        {
            string assetPath = BundleHelp.FullPath2AssetPath(ref files[i]);
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
            result.Add(shader);
        }
        return result;
    }


    private void DrawShaderElement( Rect rect, int index, bool isActive, bool isFocused)
    {
        var element = list.serializedProperty.GetArrayElementAtIndex(index);
        rect.y += 2;
        EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
    }


}
