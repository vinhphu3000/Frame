using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Text;

internal class BMDataAccessor 
{

    static public BMConfiger BMConfiger
    {
        get
        {
            if (mBMConfier == null)
                mBMConfier = LoadObjectFromJsonFile<BMConfiger>(ConfigerPath);
            return mBMConfier ?? (mBMConfier = new BMConfiger());
        }
    }


    static public List<BundleBuildState> BuildStates
    {
        get
        {
            if (mBuildStates == null)
                mBuildStates = LoadObjectFromJsonFile<List<BundleBuildState>>(BuildStatesPath);
            return mBuildStates ?? (mBuildStates = new List<BundleBuildState>());
        }
    }


    static public ExporterPlan ExporterPlan
    {
        get
        {
            if (mExporterPlan == null)
                mExporterPlan = LoadObjectFromJsonFile<ExporterPlan>(ExporterPlanPath);
            return mExporterPlan ?? (mExporterPlan = new ExporterPlan());
        }
    }


    static public void SaveConfiger()
    {
        SaveObjectToJsonFile(BMConfiger, ConfigerPath);
        SaveObjectToJsonFile(ExporterPlan, ExporterPlanPath);
    }

    static public void SaveBundleBuildeStates()
    {
        SaveObjectToJsonFile(BuildStates, BuildStatesPath);
    }


    static public void ClearBundleBuildStates( List<BundleDependsData> Data)
    {
        HashSet<string> set = new HashSet<string>();
        for (int i = 0; i < Data.Count; i++)
        {
            set.Add(Data[i].name);
        }

        for (int i = BuildStates.Count - 1; i >= 0 ; i--)
		{
			if (!set.Contains( Path.GetFileNameWithoutExtension (BuildStates[i].bundlePath)))
            {
                BuildStates.RemoveAt(i);
            }
		}

        SaveBundleBuildeStates();
    }


    static public T LoadObjectFromJsonFile<T>(string path)
    {
        T data = default(T);

        if (File.Exists(path) == false)
            return data;

        TextReader reader = new StreamReader(path);
        if (null == reader)
        {
            Debug.LogError("Cannot find " + path);
            reader.Close();
            return default(T);
        }

        data = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());
        if (null == data)
        {
            Debug.LogError("Cannot read data from " + path);
        }

        reader.Close();
        return data;
    }


    static public void SaveObjectToJsonFile<T>(T data, string path)
    {
        TextWriter tw = new StreamWriter(path);
        if (null == tw)
        {
            Debug.LogError("Cannot write to " + path);
            return;
        }

        string jsonStr = JsonConvert.SerializeObject(data, Formatting.Indented);

        tw.Write(jsonStr);
        tw.Flush();
        tw.Close();
    }


    static public void SaveReleaseDependsData(List<BundleDependsData> resArray, string path)
    {
        using (StreamWriter sw = new StreamWriter(path))
        {
            for (int i = 0; i < resArray.Count; i++)
            {
                sw.WriteLine(resArray[i].Serialize());
            }
        }
    }


    private static ExporterPlan mExporterPlan;
    static private BMConfiger mBMConfier = null;
    static private List<BundleBuildState> mBuildStates = null;

    public const string ExporterPlanPath = "Assets/BundleManager/Setting/ExporterPlan.txt";
    public const string ConfigerPath = "Assets/BundleManager/Setting/BMConfiger.txt";
    public const string BuildStatesPath = "Assets/BundleManager/Setting/BuildStates.txt";
}
