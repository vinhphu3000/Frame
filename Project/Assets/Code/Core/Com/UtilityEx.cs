using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections;

using CS;

public static class Utility
{

    static public void Log(object o)
    {
        
    }

    static public void LogColor(string color, object o)
    {
        
    }

    static public void SaveObjectToJsonFile<T>( T data, string path)
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


    static public void SafePath(string path)
    {
        path = Path.GetDirectoryName(path);
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
    }
    
}
