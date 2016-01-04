using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Reflection;

public static class CSCommonEditor
{



    static public bool DrawHeader(string text, string key, bool forceOn = false, bool minimalistic = false)
    {
        bool state = EditorPrefs.GetBool(key, true);

        //if (!minimalistic) GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state) text = "\u25BC" + (char)0x200a + text;
            else text = "\u25BA" + (char)0x200a + text;

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        }

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }


    static public void DrawHeader(string text, bool forceOn = false, bool minimalistic = false)
    {
        if (!forceOn) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            text = (char)0x200a + text;
            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f));
        }


        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn) GUILayout.Space(3f);
    }


    static public T GetField<T>(object instance, string name)
    {
        if (instance == null)
        {
            throw new NullReferenceException();
        }

        Type t = instance.GetType();
        FieldInfo field = t.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
        return (T)field.GetValue(instance);
    }


    static bool mEndHorizontal = false;
    static public void BeginContents(bool minimalistic = false)
    {
        if (!minimalistic)
        {
            mEndHorizontal = true;
            GUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
        }
        else
        {
            mEndHorizontal = false;
            EditorGUILayout.BeginHorizontal(GUILayout.MinHeight(10f));
            GUILayout.Space(10f);
        }
        GUILayout.BeginVertical();
        GUILayout.Space(2f);
    }


    static public void EndContents()
    {
        GUILayout.Space(3f);
        GUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        if (mEndHorizontal)
        {
            GUILayout.Space(3f);
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(3f);
    }


    static public string ToString(object obj)
    {
        return obj == null ? "Null" : obj.ToString();
    }


    static public bool DrawTextButton(string lab, ref string text, string buttonName)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(lab, GUILayout.Width(70f));
        text = GUILayout.TextField(text, GUILayout.MinWidth(120f));
        bool isClick = GUILayout.Button(buttonName, GUILayout.Width(70f));
        GUILayout.EndHorizontal();
        return isClick;
    }


    static public void DrawTextButton(string lab, ref string text)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(lab, GUILayout.Width(70f));
        text = GUILayout.TextField(text, GUILayout.MinWidth(120f));
        GUILayout.EndHorizontal();
    }
}
