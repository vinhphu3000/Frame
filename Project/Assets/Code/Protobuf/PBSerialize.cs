using UnityEngine;
using System;
using System.IO;
using System.Collections;
using ProtoBuf;

public static class PBSerialize
{
    public static byte[] Serialize<T>(T instance)
    {
        byte[] bytes = null;
        using (var ms = new MemoryStream())
        {
            Serializer.Serialize<T>(ms, instance);
            bytes = new byte[ms.Position];
            var fullBytes = ms.GetBuffer();
            Array.Copy(fullBytes, bytes, bytes.Length);
        }
        return bytes;
    }


    public static T DeSerialize<T>(byte[] bytes)
    {
        using (var ms = new MemoryStream(bytes))
        {
            return Serializer.Deserialize<T>(ms);
        }
    }
}
