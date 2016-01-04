//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;


//namespace Tools
//{

//    public interface IMultiProgress
//    {
//        void OnProgressUpdate(int current, int max);
//    }

//    public enum MultiPriorityType
//    {
//        First,
//        Last,
//    }

//    public abstract class MultiForWait<T>
//    {

//        public System.Action OnAllCompleted;
//        public System.Action<WaitState, object> OnSubState;

//        protected int mTaskCount;
//        protected MultiState mState;
//        protected List<T> mWaitList;
//        protected List<IMultiProgress> mProgress;


//        public MultiForWait()
//        {
//            mTaskCount = 1;
//            mWaitList = new List<T>();
//            mProgress = new List<IMultiProgress>();
//        }


//        public void AddProgressListen(IMultiProgress listen)
//        {
//            if (!mProgress.Contains(listen))
//                mProgress.Add(listen);
//        }


//        public void RemoveProgressListen(IMultiProgress listen)
//        {
//            mProgress.Remove(listen);
//        }


//        protected void NoticeProgress( int c, int m)
//        {
//            for (int i = 0; i < mProgress.Count; i++)
//            {
//                mProgress[i].OnProgressUpdate(c, m);
//            }
//        }


//        protected void NoticeAllCompleted()
//        {
//            if (OnAllCompleted != null)
//                OnAllCompleted();
//        }


//        protected void NoticeSubState( WaitState state, object info)
//        {
//            if (OnSubState != null)
//                OnSubState(state, info);
//        }


//        virtual public void PushTask(T wc)
//        {
//            PushTask(wc, MultiPriorityType.Last);
//        }


//        virtual public void PushTask(T wc, MultiPriorityType type)
//        {
//            int index = 0;

//            switch (type)
//            {
//                case MultiPriorityType.First: index = 0;
//                    break;
//                case MultiPriorityType.Last: index = mWaitList.Count - 1;
//                    break;
//            }

//            Insert(index, wc);
//        }


//        protected void Insert(int index, T wc)
//        {
//            index = Mathf.Clamp(index, 0, mWaitList.Count);
//            mWaitList.Insert(index, wc);
//        }


//        abstract public void DoUpdate();
//    }
//}


