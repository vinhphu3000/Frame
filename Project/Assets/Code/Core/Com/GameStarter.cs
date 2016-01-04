using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CS;
using CS.CSUI;
using FSM;
using System.Reflection;
//using UnityEditor.VersionControl;


public interface IGlobalEvent
{
    void DoUpdate();

    void OnAppExit();
}


public interface IQueryTrue
{
    bool IsCompeleter();
}


public class GameStarter : MonoBehaviour
{
    public static GameStarter Instance { get; set; }
    public static MonoBehaviour Mono;
    private List<IGlobalEvent> mGlobalEvents;

    void Awake()
    {
        Instance = this;
        Mono = this;
        mGlobalEvents = new List<IGlobalEvent>();
        DontDestroyOnLoad(this);
    }


    void Start()
    {
        gameObject.AddComponent<GameFSM>();
        GameFSM.Instance.ChangeState(GameStateType.StartupAuditorState);
    }

    public void AddGlobalEvent(IGlobalEvent listen)
    {
        if (mGlobalEvents.Contains(listen)) return;
        mGlobalEvents.Add(listen);
    }


    public void RemoveGlobalEvent(IGlobalEvent listen)
    {
        mGlobalEvents.Remove(listen);
    }


    void OnApplicationQuit()
    {
        NoticeAppQuit();
    }


    void Update()
    {
        NoticeTick();
    }


    private void NoticeAppQuit()
    {
        for (int i = 0; i < mGlobalEvents.Count; i++)
        {
            mGlobalEvents[i].OnAppExit();
        }
    }


    private void NoticeTick()
    {
        for (int i = 0; i < mGlobalEvents.Count; i++)
        {
            mGlobalEvents[i].DoUpdate();
        }
    }


}
