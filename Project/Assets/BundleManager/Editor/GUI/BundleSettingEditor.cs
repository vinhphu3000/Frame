using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(BuildSettingInspectorObj))]
public class BuildSettingEditor : Editor {

    static BuildSettingInspectorObj settingsInspectorObj = null;


    [MenuItem("Resource Tools/Bundle Setting &s", false, 0)]
    public static void Show()
    {
        if (settingsInspectorObj == null)
        {
            settingsInspectorObj = ScriptableObject.CreateInstance<BuildSettingInspectorObj>();
            settingsInspectorObj.hideFlags = HideFlags.DontSave;
            settingsInspectorObj.name = "BundleManager Settings";
        }

        Selection.activeObject = settingsInspectorObj;
    }


#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_0)
    public override bool UseDefaultMargins()
    {
        return false;
    }
#endif


    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            DrawBMConfiger();
        }
        EditorGUILayout.EndVertical();
    }



    private void DrawBMConfiger()
    {
        BundleSetting.IsUseFive = EditorGUILayout.Toggle("Use 5.0 Above", BundleSetting.IsUseFive);
        BundleSetting.Compress = EditorGUILayout.Toggle("Compress", BundleSetting.Compress);
        BundleSetting.DeterministicBundle = EditorGUILayout.Toggle("Deterministic", BundleSetting.DeterministicBundle);
        BundleSetting.UseEditorTarget = EditorGUILayout.Toggle("Use Editor Target", BundleSetting.UseEditorTarget);

        if (BundleSetting.UseEditorTarget)
        {
            BundleSetting.UnityBuildTarget = EditorUserBuildSettings.activeBuildTarget;
        }

        BuildPlatform origPlatform = BundleSetting.BundleBuildTarget;
        GUI.enabled = !BundleSetting.UseEditorTarget;
        BuildPlatform newPlatform = (BuildPlatform)EditorGUILayout.EnumPopup("Build Target", (System.Enum)origPlatform);
        GUI.enabled = true;

        if (origPlatform != newPlatform)
        {
            GUIUtility.keyboardControl = 0;
            BundleSetting.BundleBuildTarget = newPlatform;
        }

        EditorGUILayout.BeginHorizontal();
        {
            BundleSetting.OutputPath = EditorGUILayout.TextField("Output Path", BundleSetting.OutputPath);
            if(GUILayout.Button("...", GUILayout.MaxWidth(24)))
            {
                GUIUtility.keyboardControl = 0;
                BundleSetting.OutputPath = EditorUtility.OpenFolderPanel("Choose Output Path", BundleSetting.OutputPath, "");
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(5f);

        //  default path

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Select Deafault Path");
        if ( GUILayout.Button("+") )
        {
            BundleSetting.SelectDefaultPath.Add(new BundleFileData());
        }
        EditorGUILayout.EndHorizontal();

        List<int> deleteDefaultPath = new List<int>();
        for (int i = 0; i < BundleSetting.SelectDefaultPath.Count; i++)
        {
            if (BundleSetting.SelectDefaultPath[i] == null)
            {
                deleteDefaultPath.Add(i);
                continue;
            }


            GUILayout.Label((i + 1).ToString());
            EditorGUILayout.BeginHorizontal();
            BundleSetting.SelectDefaultPath[i].path = EditorGUILayout.TextField("Path ", BundleSetting.SelectDefaultPath[i].path);
            
            if (GUILayout.Button("...", GUILayout.MaxWidth(24)))
            {
                BundleSetting.SelectDefaultPath[i].path = EditorUtility.OpenFolderPanel("Choose Output Path", BundleSetting.OutputPath, "");
            }

            GUI.color = Color.red;
            if (GUILayout.Button("-", GUILayout.MaxWidth(24)))
            {
                deleteDefaultPath.Add(i);
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();

            BundleSetting.SelectDefaultPath[i].suffix = EditorGUILayout.TextField("Suffix ".ToString(), BundleSetting.SelectDefaultPath[i].suffix);

            GUILayout.Space(2f);
        }

        deleteDefaultPath.Sort((int l, int r) => r.CompareTo(l));
        for (int i = 0; i < deleteDefaultPath.Count; i++)
        {
            BundleSetting.SelectDefaultPath.RemoveAt(deleteDefaultPath[i]);
        }
        deleteDefaultPath.Clear();


        //  指定打包路径与后缀过滤
        DrawExporterPlan();
        
        GUILayout.Space(5f);
        if (GUILayout.Button("Save Data"))
        {
            BMDataAccessor.SaveConfiger();
        }
    }


    #region ExporterPlan
    
   
    private void DrawExporterPlan()
    {
        GUILayout.Space(5f);
        //isShowExporterPlan = EditorGUILayout.ToggleLeft("Exproter Plan", isShowExporterPlan);
        if (CSCommonEditor.DrawHeader("Exproter Plan", "Exproter Plan"))
        {
            var data = BMDataAccessor.ExporterPlan;
            List<int> deleteExporter = new List<int>();
            List<int> deleteSuffix = new List<int>();

            if (GUILayout.Button("Add New Exporter Plan" ))
            {
                data.ListExporterPlan.Add(new ExporterData());
            }

            for (int index = data.ListExporterPlan.Count - 1; index >= 0; index--)
            {
                EditorGUI.indentLevel = 0;
                DrawSubExporterPlan(data.ListExporterPlan[index], ref deleteSuffix);
            }

            //  delete 
            foreach (int i in deleteExporter)
            {
                data.ListExporterPlan.RemoveAt(i);
            }
        }
    }

    private bool DrawSubExporterPlan(ExporterData iter, ref List<int> deleteSuffix)
    {
       
        string title = BundleHelp.FullPath2AssetPath(ref iter.Path);

        if (!CSCommonEditor.DrawHeader(title ?? "<空>", iter.Path, true, true) )
            return false;

        CSCommonEditor.BeginContents();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Path ", title);
        if (GUILayout.Button("...", GUILayout.MaxWidth(24)))
        {
            iter.Path = EditorUtility.OpenFolderPanel("Choose Output Path", Application.dataPath, "");
        }
        GUI.color = Color.red;
        if (GUILayout.Button("-", GUILayout.MaxWidth(24)))
        {
            return true;
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel = 1;

        for (int i = iter.Suffixs.Count - 1; i >= 0; i--)
        {
            EditorGUILayout.BeginHorizontal();

            string safeSuffix = EditorGUILayout.TextField("Suffix ", iter.Suffixs[i]);
            if (string.IsNullOrEmpty(safeSuffix))
            {
                safeSuffix = "*.prefab";
            }
            else if (!safeSuffix.StartsWith("*."))
            {
                safeSuffix = "*." + safeSuffix;
            }
            iter.Suffixs[i] = safeSuffix;

            GUI.color = Color.red;
            if (GUILayout.Button("-", GUILayout.MaxWidth(24)))
            {
                deleteSuffix.Add(i);
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }


        //  delelte click '-'
        foreach (int j in deleteSuffix)
        {
            iter.Suffixs.RemoveAt(j);
        }

        deleteSuffix.Clear();

        GUI.color = Color.green;
        //  new suffix
        if (GUILayout.Button("Add New Suffix"))
        {
            iter.Suffixs.Add("*.prefab");
        }
        GUI.color = Color.white;

        CSCommonEditor.EndContents();

        return false;
    }
    #endregion
}
