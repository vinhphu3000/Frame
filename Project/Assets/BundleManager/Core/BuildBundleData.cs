using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;


public class BundleDependsData
{
    public string md5;
    public string name;
    public string[] depends;


    /// <summary>
    /// name , md5 , depends
    /// </summary>
    /// <returns></returns>

    public string Serialize()
    {
        string result = string.Format("{0}:{1}:", name, md5);

        if (depends.Length > 0)
            result += depends[0];

        for (int i = 1; i < depends.Length; i++)
        {
            result += "," + depends[i];
        }

        return result;
    }


    static public BundleDependsData Deserialize(string str)
    {
        if (string.IsNullOrEmpty(str))
            return null;
        BundleDependsData data = new BundleDependsData();

        var paragraph = str.Split(':');
        data.name = paragraph[0];
        data.md5 = paragraph[1];

        var depends = paragraph[2];
        if (string.IsNullOrEmpty(depends))
            return data;

        data.depends = depends.Split(',');
        return data;
    }
}


public class BundleData
{

    public string name;

    public string md5;

    public string guid;

    public string assetPath;

    public List<string> depends;
}


public class BundleFileData
{
    public string path;
    public string suffix;
}


public class BMConfiger
{

    public bool isUse5 = false;
    public bool compress = true;
    public bool deterministicBundle = false;
    public string bundleSuffix = "assetBundle";
    public string buildOutputPath = "";
    public bool useCache = true;
    public bool useEditorTarget = true;
    public List<BundleFileData> selectBuildPath;
    public BuildPlatform bundleTarget = BuildPlatform.Standalones;


    public Dictionary<string, string> outputs;


    public BMConfiger()
    {
        outputs = new Dictionary<string, string>();
        selectBuildPath = new List<BundleFileData>();
    }


    public string GetOutputPath()
    {
        return buildOutputPath;
    }

}


public class BundleBuildState
{
    public string bundlePath;
    public long size;
    public uint crc;
    public long changeTime;
    public string[] lastDependencies = null;
}


public enum BuildPlatform
{
    WebPlayer,
    Standalones,
    IOS,
    Android,
    WP8,
}

