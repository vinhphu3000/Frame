//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//namespace Tools
//{
//    public class MultiWaitForWww : MultiForWait<string>
//    {

       
//        List<WaitForWww> mWwwList;

//        int mMaxCount;
//        int mCompletedCount;
        
        
//        public MultiWaitForWww( string[] resArray )
//        {
//            mWwwList = new List<WaitForWww>();

//            mCompletedCount = 0;
//            mMaxCount = resArray.Length;
//            mWaitList = new List<string>(resArray);

//            mState = MultiState.Puase;
//        }


//        public void Pause()
//        {
//            mState = MultiState.Puase;
//        }


//        public void Start()
//        {
//            mState = MultiState.Run;
//        }


//        public bool Exist(string res)
//        {
//            bool isExist = false;

//            //	in task ?
//            isExist = mWaitList.Contains(res);

//            //	in loading ?
//            for (int i = 0; i < mWwwList.Count; i++) {
//                if (mWwwList[i].Url.LastIndexOf(res) != -1)
//                {
//                    isExist = true;
//                    break;
//                }
//            }

//            return isExist;
//        }


//        public override void PushTask(string wc, MultiPriorityType type)
//        {
//            base.PushTask(wc, type);

//            ++mMaxCount;
//            if (mState == MultiState.Puase)
//                mState = MultiState.Run;
//        }


//        override public void DoUpdate()
//        {

//            Debug.Log( "State " + mState.ToString() );
//            if (mState == MultiState.Puase)
//                return;

//            if (mState == MultiState.Run)
//            {
//                if (mWaitList.Count == 0 || mCompletedCount == mMaxCount)
//                {
//                    mState = MultiState.Done;
//                }
//                else
//                {
//                    int index = -1;
//                    while (++index < mMaxCount && mWaitList.Count > 0) {
//                        string res = Dequeue();
//                        WaitForWww www = CreateWWW();
//                        mWwwList.Add ( www );

//                        //	TODO : res是资源名称,需要在这里合并为路径
//                        www.Start(res);
//                    }
//                }
//            }
//            //  next frame
//            else
//            {
//                mState = MultiState.Puase;
//                NoticeAllCompleted();
//            }
//        }


//        private string Dequeue()
//        {
//            if (mWaitList.Count == 0)
//                return string.Empty;
//            string url = mWaitList[0];
//            mWaitList.RemoveAt(0);
//            return url;
//        }


//        private WaitForWww CreateWWW()
//        {
//            WaitForWww w = new WaitForWww();
//            w.OnCompleted = OnWaitCompleted;
//            return w;
//        }


//        private void OnWaitCompleted(WaitCoroutine wc)
//        {
//            ++mCompletedCount;

//            WaitForWww www = (WaitForWww)wc;

//            mWwwList.Remove(www);
//            NoticeProgress(mCompletedCount, mMaxCount);
//            NoticeSubState(wc.mState, www.Url);

//            // todo debug info 
//            Debug.Log( "CompletedCount : " + mCompletedCount + " State : " + wc.mState.ToString() + " Finish : " + www.Url);
//        }
//    }
//}


