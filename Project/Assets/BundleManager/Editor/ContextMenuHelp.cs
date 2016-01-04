using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;

public static class ContextMenuHelp
{

    static List<string> mEntries = new List<string>();
    static GenericMenu mMenu;

    static public void AddItem(string item, bool isChecked, GenericMenu.MenuFunction2 callback, object param)
    {
        if (callback != null)
        {
            if (mMenu == null)
                mMenu = new GenericMenu();
            int count = 0;

            for (int i = 0; i < mEntries.Count; i++)
            {
                string str = mEntries[i];
                if (str == item) ++count;
            }
            mEntries.Add(item);

            if (count > 0) item += " [" + count + "]";
            mMenu.AddItem(new GUIContent(item), isChecked, callback, param);
        }
        else
        {
            AddDisabledItem(item);
        }
    }


    static public void AddDisabledItem(string item)
    {
        if (mMenu == null) mMenu = new GenericMenu();
        mMenu.AddDisabledItem(new GUIContent(item));
    }


    static public void Show()
    {
        if (mMenu != null)
        {
            mMenu.ShowAsContext();
            mMenu = null;
            mEntries.Clear();
        }
    }


    static public void Clear()
    {
        mEntries.Clear();
        mMenu = null;
    }

}
