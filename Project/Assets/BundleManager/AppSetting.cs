using System.IO;
using CS;
using UnityEngine;
using System.Collections;


public class AppConfig
{
    public bool isAssets = false;
    public string mCdn = string.Empty;
}


public sealed class AppSetting
{
    static AppConfig appConfig;

    static string resourcePath;
    static string resourceUrl;


    static public string ResourceUrl
    {
        get 
        {
            if (string.IsNullOrEmpty(resourceUrl))
            {
                resourceUrl = "file:///" + ResourcePath;
            }

            return resourceUrl;
        }
    }


    static public string ResourcePath
    {
        get
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
#if UNITY_EDITOR
                resourcePath = Application.dataPath + "/../../Resources/TestBuild";
#elif UNITY_STANDALONE
                
#elif UNITY_ANDROID

#elif UNITY_IOS
#endif

#if USE_5_BUNDLE
                resourcePath += "New";
#endif
            }

            return resourcePath;
        }
    }


    static public AppConfig App
    {
        get
        {
            if (appConfig == null)
                LoadAppSrtting();

            return appConfig;
        }
    }


    static public string CDN
    {
        get { return App.mCdn; }
    }


    static public void LoadAppSrtting()
    {
        string filePath = ResourcePath + "/AppConfig.json";
        if (File.Exists(filePath))
            appConfig = Utility.LoadObjectFromJsonFile<AppConfig>(filePath);
        else
            appConfig = new AppConfig();
    }


    static public void SaveAppSetting()
    {
        string filePath = ResourcePath + "/AppConfig.json";
        if (File.Exists(filePath) == false)
        {
            File.CreateText(@filePath);
        }
        Utility.SaveObjectToJsonFile(appConfig, filePath);
    }
}
