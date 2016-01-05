using System.IO;
using UnityEngine;
using System.Collections;

public sealed class BundleHelp
{


    static public string FullPath2AssetPath(ref string src)
    {
        if (string.IsNullOrEmpty(src))
            return string.Empty;

        int index = src.IndexOf("Assets", System.StringComparison.Ordinal);
        return src.Substring(index, src.Length - index);
    }


    static public string AssetPath2FullPath(ref string src)
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, src.Substring("Assets/".Length));
#elif
        return src;
#endif
    }


    static public string GetFolderName(ref string src)
    {
        if (Directory.Exists(src))
        {
            return new DirectoryInfo(src).Name;
        }
        return string.Empty;
    }
}
