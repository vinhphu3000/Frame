using System.Collections.Generic;
using UnityEngine;
using System.Collections;


namespace Tools
{
    public sealed class WaitForStartupAuditor : IGlobalEvent
    {

        private int currentIndex;
        List<ISingleManager> mWaitFunctions;


        public WaitForStartupAuditor()
        {
            mWaitFunctions = new List<ISingleManager>();
        }


        public void Push(ISingleManager isinManager)
        {
            mWaitFunctions.Add(isinManager);
        }


        public void Start()
        {
            currentIndex = -1;
            TryNext();
        }


        private void TryNext()
        {
            ++currentIndex;
            if (currentIndex >= mWaitFunctions.Count)
                return;
            GameStarter.Mono.StartCoroutine(mWaitFunctions[currentIndex].Init());
        }


        public bool IsComplete()
        {
            return currentIndex >= mWaitFunctions.Count;
        }


        public void DoUpdate()
        {
            if (currentIndex <= mWaitFunctions.Count && mWaitFunctions[currentIndex].IsInitComplete())
            {
                Debug.Log(string.Format("{0} Complete", mWaitFunctions[currentIndex].ToString()));
                TryNext();
            }
        }

        public void OnAppExit()
        {
            currentIndex = int.MaxValue;
            mWaitFunctions.Clear();
        }
    }
}


