using System;
using System.IO;
using System.Text;
//#if UNITY_EDITOR
//using UnityEditor;
//#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Runtime
/// </summary>

public class BundleManifest
{

    private Dictionary<string, string[]> mManifest = new Dictionary<string, string[]>();

    public BundleManifest()
    {

    }


    public BundleManifest(AssetBundleManifest manifest)
    {
        ApplyManifest(manifest);
    }


    public void Clear()
    {
        mManifest.Clear();
    }


    public string[] GetDirectDependencies(string assetBundleName)
    {
        string[] result = default(string[]);
        mManifest.TryGetValue(assetBundleName, out result);
        return result;
    }


    public string[] GetAllDependencies(string assetBundleName)
    {
        List<string> resultList = new List<string>();
        GetAllDependencies(ref resultList, ref assetBundleName);
        return resultList.ToArray();
    }


    public string[] GetAllAssetBundles()
    {
        string[] result = new string[mManifest.Keys.Count];
        int index = 0;
        foreach (var name in mManifest.Keys)
        {
            result[index ++] = name;
        }
        return result;
    }


    private void GetAllDependencies(ref List<string> result, ref string assetBundleName)
    {
        if (mManifest.ContainsKey(assetBundleName))
        {
            string[] depends = mManifest[assetBundleName];
            result.AddRange(depends);
            for (int i = 0; i < depends.Length; i++)
            {
                GetAllDependencies(ref result, ref depends[i]);
            }
        }
    }


    private void ApplyManifest(AssetBundleManifest manifest)
    {
        string[] allbundleNames = manifest.GetAllAssetBundles();
        for (int i = 0; i < allbundleNames.Length; i++)
        {
            string name = allbundleNames[i];
            string[] depends = manifest.GetDirectDependencies(allbundleNames[i]);
            mManifest.Add(name, depends);
        }
    }


    public void SaveToFile(string filePath)
    {
        using (StreamWriter sw = new StreamWriter(filePath))
        {
            foreach (var iter in mManifest)
            {
                sw.WriteLine(string.Format("{0}:{1}", iter.Key, Depend2String(iter.Value)));
            }
        }
    }


    public void LoadFile(string filePath)
    {
        Clear();
        string key;
        string[] depends;
        using (StreamReader sr = new StreamReader(filePath))
        {
            while (sr.Peek() >= 0)
            {
                string str = sr.ReadLine();
                String2Depend(str, out key, out depends);
                if (string.IsNullOrEmpty(key) == false)
                {
                    mManifest.Add(key, depends);
                }
            }
        }
    }


 


    private void String2Depend(string str, out string key, out string[] depends)
    {
        if (string.IsNullOrEmpty(str))
        {
            key = string.Empty;
            depends = null;
            return;
        }

        string[] data = str.Split(':');
        key = data[0];
        depends = data[1].Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
    }




#if UNITY_EDITOR


    private static StringBuilder sbBuilder = new StringBuilder(128);
    private string Depend2String(string[] depends)
    {
        if (depends == null)
            return string.Empty;

        sbBuilder.Length = 0;

        if (depends.Length >= 1)
            sbBuilder.Append(depends[0]).Append(",");

        for (int i = 1; i < depends.Length; i++)
        {
            sbBuilder.Append(depends[i]).Append(',');
        }

        return sbBuilder.ToString();
    }


    public static BundleManifest CombineBundleManifest(string outputPath,AssetBundleManifest beforManifest, AssetBundleManifest nowManifest)
    {
        BundleManifest data = new BundleManifest(beforManifest);


        //  remove non exist
        List<string> removeList = new List<string>();
        foreach (var iter in data.mManifest)
        {
            if (File.Exists(outputPath + "/" + iter.Key) == false)
            {
                removeList.Add(iter.Key);
            }
        }

        for (int i = 0; i < removeList.Count; i++)
        {
            data.mManifest.Remove(removeList[i]);
        }


        string nowManifestName = string.Empty;
        string[] allNowManifest = nowManifest.GetAllAssetBundles();
        for (int i = 0; i < allNowManifest.Length; i++)
        {
            nowManifestName = allNowManifest[i];
            string[] nowDepend = nowManifest.GetDirectDependencies(nowManifestName);
            if (data.mManifest.ContainsKey(nowManifestName))
            {
                data.mManifest[nowManifestName] = nowDepend;
            }
            else
            {
                data.mManifest.Add(nowManifestName, nowDepend);
            }
        }

        return data;
    }



    public void DebugString()
    {
        if (mManifest == null)
        {
            Debug.Log("NULL");
        }


        else
        {
            foreach (var item in mManifest)
            {
                Debug.Log(item.Value);
            }

            Debug.Log(string.Format(" Count {0}", mManifest.Count));
        }
    }


#endif



}
