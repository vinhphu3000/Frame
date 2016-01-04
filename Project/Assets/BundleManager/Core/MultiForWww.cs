using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Tools;


public class MultiForWww 
{

	public bool IsWaitLoad;
    public object Data;
    public System.Action<MultiForWww> OnAllCompleted;
	public System.Action<WaitForWww> OnSubCompleted;
    
	
	public bool IsError{ get; private set;}

	public MultiState State{ get; private set;}

    public int Index { get { return index; } }

    public int ParallelCount { get { return parallelCount; } }


#if UNITY_EDITOR

    public List<string> ResList { get { return mResList; } }

    public List<WaitForWww> WwwList { get { return mWwwList; } }

#endif

	int index;
	int parallelCount = 1;
	List<string> mResList = new List<string>();
	List<WaitForWww> mWwwList = new List<WaitForWww>();


	public MultiForWww()
	{
		State = MultiState.Puase;
	}


	public void Start( List<string> resArray )
	{
		IsError = false;
		index = 0;
        State = MultiState.Run;
		mResList.AddRange(resArray);
	}


	public void DoUpdate()
	{
		if (State == MultiState.Puase || State == MultiState.Done)
			return;

		if (IsWaitLoad && mWwwList.Count > 0)
			return;

		int count = -1;
		while (++count < parallelCount && (count + index) < mResList.Count) 
        {
#if USE_5_BUNDLE
			string url = AppSetting.ResourceUrl + "/" + mResList[index + count];
#else
            string url = AppSetting.ResourceUrl + "/" + mResList[index + count] + ".assetbundle";
#endif
			WaitForWww www = new WaitForWww {OnCompleted = OnCompleted};
		    mWwwList.Add(www);
			www.Start(url);
		}
		index += count;

		//	finish
        if (mWwwList.Count == 0)
		{
			State = MultiState.Done;

			if (OnAllCompleted != null)
				OnAllCompleted(this);
			Reset();
		}
	}


	private void Reset()
	{
		index = 0;
		IsError = false;
		mResList.Clear();
		OnAllCompleted = null;
	}


	private void OnCompleted(WaitCoroutine wc)
	{
		WaitForWww www = (WaitForWww)wc;
		mWwwList.Remove(www);

		if (wc.mState == WaitState.Error)
			IsError = true;

		if (OnSubCompleted != null)
		{
			OnSubCompleted(www);
		}

	}

}
