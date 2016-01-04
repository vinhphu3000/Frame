using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using CS;

namespace Tools
{
    public sealed class TaskForSequenceQuery 
    {

        List<TaskForCriteriaQuery> mQueryList;
        System.Action<bool, string> mOnComplete;
        TaskStateOption mOption;


        public TaskForSequenceQuery( System.Action<bool, string> OnComplete , TaskStateOption option = TaskStateOption.None )
        {
            mOption = option;
            mOnComplete = OnComplete;
            mQueryList = new List<TaskForCriteriaQuery>();
        }


        public void AddQuery( System.Func<bool> queryFunc )
        {
            mQueryList.Add(new TaskForCriteriaQuery(queryFunc));
        }


        public void AddQuery(TaskForCriteriaQuery query)
        {
            if (query == null)
                return;

            mQueryList.Add(query);
        }


        public void DoUpdate()
        {

            bool isFinish = true;

            for (int i = mQueryList.Count - 1; i >= 0; i--)
            {
                try
                {
                    isFinish = mQueryList[i].IsQueryConclude();
                }
                catch (System.Exception e)
                {
                    if (mOption == TaskStateOption.IgnoreError)
                    {
                        mQueryList.RemoveAt(i);
                        Utility.LogColor("ff0000", "Ignore Task Index {0} Error !");
                    }
                    else
                        Debug.LogError(string.Format("Task Index {0} Exception Info {1}", i.ToString(), e.ToString()));
                }
                
                if (isFinish == false)
                    return;
            }

            if (mOnComplete != null)
                mOnComplete(true, string.Empty);
        }

    }

}

