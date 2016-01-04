using UnityEditor;
using UnityEngine;
using System.Collections;


[CustomEditor(typeof(GameStarter))]
public class GameStarterInspector : Editor
{
    


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        AppConfig appConfig = AppSetting.App;

        DrawAppConfig(appConfig);

    }



    private void DrawAppConfig(AppConfig appConfig )
    {
        appConfig.isAssets = EditorGUILayout.Toggle("Load Assets(Editor Only)", appConfig.isAssets);
        appConfig.mCdn = EditorGUILayout.TextField("CDN", appConfig.mCdn);


        if (GUILayout.Button("Apply"))
        {
            AppSetting.SaveAppSetting();
        }
    }
}
