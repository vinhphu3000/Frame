using System;
using System.Collections;



public interface ISingleManager
{
    bool IsInitComplete();

    IEnumerator Init();
}



public class Singleton<T> where T: new()
{
    protected static T instance;
    protected bool isInit;

    static Singleton()
    {
        T local = default(T);
        Singleton<T>.instance = (local != null) ? default(T) : Activator.CreateInstance<T>();
    }

    protected Singleton()
    {
        isInit = false;
    }

    public static T Instance
    {
        get
        {
            return Singleton<T>.instance;
        }
    }
}
