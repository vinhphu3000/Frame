using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;


public class ExporterData
{
    public string Path;
    public List<string> Suffixs;

    public ExporterData()
    {
        Suffixs = new List<string>();
    }
}


public sealed class ExporterPlan
{
    private string[] GUIAtlasExportFileType = new string[] { ".prefab", ".ttf" };
    private string[] GUITextureExportFileType = new string[] { ".png" };
    private string[] GUIFontExportFileType = new string[] { ".prefab", ".ttf" };


    private List<ExporterData> mListExporterPlan;

    public ExporterPlan()
    {
        ListExporterPlan = new List<ExporterData>();
    }

    public List<ExporterData> ListExporterPlan
    {
        get { return mListExporterPlan; }
        set { mListExporterPlan = value; }
    }

    public bool IsExportFileType( string path)
    {
        var suffix = Path.GetExtension(path);
        if (suffix == null) return false;
        suffix = suffix.ToLower();

        if (path.StartsWith("Assets/_Art/Atlas"))
        {
            return GUIAtlasExportFileType.Contains(suffix);
        }
        else if (path.StartsWith("Assets/_Art/Fonts"))
        {
            return GUIFontExportFileType.Contains(suffix);
        }
        //else if (path.StartsWith("Assets/_Art/Texture"))
        //{
        //    return GUITextureExportFileType.Contains(suffix); 
        //}

        return false;
    }

//#if UNITY_EDITOR

//    public static void DrawSelf( ExporterPlan plan )
//    {
//        editor
//    }

//#endif
}



public sealed class BuildExporterPlan
{
    public class ExporterPlanData
    {
        public string Name;
        public string FileMatching;
        public string[] FileType;


        public bool IsExportFileType(ref string suffix)
        {
            for (int i = 0; i < FileType.Length; i++)
            {
                if (FileType[i] == suffix)
                    return true;
            }
            return false;
        }
    }

    public List<ExporterPlanData> mExporterPlanDatas;

    public BuildExporterPlan()
    {
        mExporterPlanDatas = new List<ExporterPlanData>();
    }

    public bool IsExportFileType(string path)
    {
        string suffix = Path.GetExtension(path).ToLower();
        for (int i = 0; i < mExporterPlanDatas.Count; i++)
        {
            if (path.Contains(mExporterPlanDatas[i].FileMatching) && mExporterPlanDatas[i].IsExportFileType(ref suffix))
                return true;
        }
        return false;
    }

}


public interface IExporterPlan
{
    bool Exclude(string path);
}


public class ExporterPlanBundleObject : IExporterPlan
{
    private static string[] GUIAtlasExportFileType = new string[] { ".prefab", ".ttf" };
    private static string[] GUITextureExportFileType = new string[] { ".png" };
    private static string[] GUIFontExportFileType = new string[] { ".prefab", ".ttf" };


    public bool Exclude(string path)
    {
        string suffix = Path.GetExtension(path);
        if (suffix != null)
        {
            suffix = suffix.ToLower();
            return suffix.Equals(".cs");
        }
        return true;
    }


    public static bool IsExportFileType(string path)
    {
        var suffix = Path.GetExtension(path);
        if (suffix == null) return false;
        suffix = suffix.ToLower();

        if (path.StartsWith("Assets/_Art/Atlas"))
        {
            return GUIAtlasExportFileType.Contains(suffix);
        }
        else if (path.StartsWith("Assets/_Art/Fonts"))
        {
            return GUIFontExportFileType.Contains(suffix);
        }
        //else if (path.StartsWith("Assets/_Art/Texture"))
        //{
        //    return GUITextureExportFileType.Contains(suffix); 
        //}

        return false;
    }
}