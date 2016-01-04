using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace CS.CSUI
{ 

    public enum StackState
    {
        Normal,
        EffectHide,
    }

    public class StackData
    {
        public GameObject mObject;
        public UIWindowInfo mInfo;
        public int mOrder;
        public int mStartDepth;
        public int mEndDepth;
        public StackState mState;

        void StackState()
        {
            mState = CSUI.StackState.Normal;
        }

        public void SetVisible( bool isShow)
        {
            mObject.SetActive(isShow);
        }
    }


    public class StackChunk
    {
        public UIWindowType mChunkType { get; private set; }
        public int mStartDepth { get; private set; }
        public List<StackData> mChunkList{get; private set;}
        public StackData TopStack { get; private set; }

        public StackChunk(UIWindowType t)
        {
            mChunkType = t;
            mChunkList = new List<StackData>();
            mStartDepth = (int)t;
        }


        public int GetUsableDepth()
        {
            if (mChunkList.Count == 0)
                return mStartDepth;

            return mChunkList[mChunkList.Count - 1].mEndDepth + 1;
        }


        public int Push(StackData data)
        {
            ExistPop(ref data);
            data.mStartDepth = GetUsableDepth();
            data.mEndDepth = UITools.SetObjectDepth(data.mObject, data.mStartDepth);
            TopStack = data;
            mChunkList.Add(data);
            TriggerPush(ref data);

            return data.mEndDepth;
        }


        public void Pop(StackData data)
        {
            int index = mChunkList.FindIndex((StackData d) => { return d.mObject == data.mObject; });
            if (index == -1)
                return;

            TriggerPop(index);

            Remove(index);

            if (mChunkList.Count > 0)
                TopStack = mChunkList[mChunkList.Count - 1];
            else
                TopStack = null;
        }

        private void ExistPop(ref StackData data)
        {
            Pop(data);
        }


        public void SafeInsert( int insertIndex, StackData data)
        {
            int index = mChunkList.FindIndex((StackData d) => { return d.mObject == data.mObject; });

            if (index == -1)
                insertIndex = mChunkList.Count;
            else
            {
                if (Remove(index) == false)
                {
                    insertIndex = mChunkList.Count;
                }
            }

            Insert(insertIndex, data);
        }


        /// <summary>
        /// 插入一个指定位置
        /// </summary>
        /// <param name="index">插入索引,原索引后移</param>
        /// <param name="data"></param>

        private void Insert(int index, StackData data)
        {
            data.mStartDepth = mChunkList[index].mStartDepth;
            data.mEndDepth = UITools.SetObjectDepth(mChunkList[index].mObject, data.mStartDepth);
            
            int endDepth = data.mEndDepth;
            while (index < mChunkList.Count)
            {
                ++endDepth;
                mChunkList[index].mStartDepth = endDepth;
                endDepth = UITools.SetObjectDepth(mChunkList[index].mObject, endDepth);
                mChunkList[index].mEndDepth = endDepth;
                ++index;
            }

            mChunkList.Insert(index, data);
        }


        /// <summary>
        /// 移除指定位置深度排列
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>

        private bool Remove( int index )
        {
            if (index < 0 || index >= mChunkList.Count)
                return false;

            int startDepth = mChunkList[index].mStartDepth;
            int rindex = index;
            while (++index < mChunkList.Count)
            {
                mChunkList[index].mEndDepth = UITools.SetObjectDepth(mChunkList[index].mObject, startDepth);
                mChunkList[index].mStartDepth = startDepth;
                startDepth = mChunkList[index].mEndDepth + 1;
                ++index;
            }
            mChunkList.RemoveAt(rindex);

            return true;
        }


        private void TriggerPush( ref StackData data)
        {
            UIWindowEffect eff = data.mInfo.WinEffect;
            if ( (eff & UIWindowEffect.ExclusiveView) != 0 )
            {
                MonopolizeChunk(data);
            }


        }

        private void TriggerPop( int index)
        {
            StackData data = mChunkList[index];
            UIWindowEffect eff = data.mInfo.WinEffect;
            if ((eff & UIWindowEffect.ExclusiveView) != 0)
            {
                DeMonopolizeChunk(data);
            }
        }


        /// <summary>
        /// 当前窗口需要独占窗口
        /// </summary>
        /// <param name="data"></param>

        private void MonopolizeChunk( StackData data )
        {
            int index = mChunkList.FindIndex((StackData v) => { return v == data; });
            if (index != -1)
            {
                VisibleRange(0, index);
            }
            else
            {
                Utility.Log(string.Format("MonopolizeChunk( ... ) Can't find StackData {0} ", data.mInfo.Key.ToString()));
            }
        }


        /// <summary>
        /// 当前窗口取消独占窗口
        /// </summary>
        /// <param name="data"></param>

        private void DeMonopolizeChunk( StackData data )
        {
            int index = mChunkList.FindIndex((StackData v) => { return v == data; });
            if (index != -1)
            {
                DeVisibleRange(0, index);
            }
            else
            {
                Utility.Log(string.Format("DeMonopolizeChunk( ... ) Can't find StackData {0} ", data.mInfo.Key.ToString()));
            }
        }

        private void VisibleRange( int startIndex, int endIndex)
        {
            StackData data = null;
            for (int i = endIndex - 1; i >= startIndex; i--)
            {
                data = mChunkList[i];

                if (data.mState == StackState.EffectHide)
                    break;

                data.mState = StackState.EffectHide;
                data.SetVisible(false);

                //  之前窗口依然是独占
                if ((data.mInfo.WinEffect & UIWindowEffect.ExclusiveView) != 0)
                    break;
            }
        }


        private void DeVisibleRange(int startIndex, int endIndex)
        {
            StackData data = null;
            for (int i = endIndex - 1; i >= startIndex; i--)
            {
                data = mChunkList[i];

                if (data.mState == StackState.Normal)
                    break;

                data.mState = StackState.Normal;
                data.SetVisible(true);

                //  之前窗口依然是独占
                if ((data.mInfo.WinEffect & UIWindowEffect.ExclusiveView) != 0)
                    break;
            }
        }
    }


    public class StackManager 
    {

        private List<StackData> mListWindow;
        private Dictionary<UIWindowType, StackChunk> mChunkWindow;


        public StackManager()
        {
            mListWindow = new List<StackData>();
            mChunkWindow = new Dictionary<UIWindowType, StackChunk>();

            foreach (int item in System.Enum.GetValues(typeof(UIWindowType)))
            {
                UIWindowType t = (UIWindowType)item;
                mChunkWindow.Add(t, new StackChunk(t));
            }
        }


        public void Push( GameObject obj, UIWindowInfo info)
        {
            StackData data = FindByType(info.Key);
            if (data == null)
            {
                data = new StackData();
                data.mObject = obj;
                data.mInfo = info;
                data.mOrder = mListWindow.Count;
                mListWindow.Add(data);
            }

            StackChunk chunk;
            if ( mChunkWindow.TryGetValue( info.WinType, out chunk))
            {
                OnPushTrigger(ref data);
                chunk.Push(data);
            }
            else
            {
                mListWindow.Remove(data);
                Debug.LogError(string.Format("Can't find chunk [{0}], Key [{1}]", info.WinType.ToString(), info.Key.ToString()));
            }
           
        }


        public void Pop( UIType type )
        {
            StackData data = FindDataByType(ref type);
            if (data != null)
            {
                StackChunk chunk;
                if (mChunkWindow.TryGetValue(data.mInfo.WinType, out chunk)) 
                {
                    chunk.Pop(data);
                    mListWindow.Remove(data);
                    OnPopTrigger( ref data);
                }
                else
                {
                    Debug.LogError(string.Format("Can't find chunk [{0}], Key [{1}]", data.mInfo.WinType.ToString(), data.mInfo.Key.ToString()));
                }
            }
            else
                Debug.Log(string.Format("Can't find type : {0}", type.ToString()));
        }


        public bool Exist(UIType t)
        {
            return mListWindow.FindIndex((StackData data) => { return data.mInfo.Key == t; }) != -1;
        }

        public StackData FindByType( UIType t)
        {
            return mListWindow.Find((StackData v) => { return v.mInfo.Key == t; });
        }

        public StackData GetTopStackByWinType(UIWindowType type)
        {
            StackChunk chunk;
            if (mChunkWindow.TryGetValue(type, out chunk))
            {
                return chunk.TopStack;
            }
            return null;
        }


        public StackData GetTopLevel()
        {
            StackData result = null;
            int maxDepth = int.MinValue;
            foreach (var iter in mChunkWindow)
            {
                if ( iter.Value.TopStack != null && iter.Value.TopStack.mStartDepth > maxDepth)
                {
                    result = iter.Value.TopStack;
                    maxDepth = iter.Value.TopStack.mStartDepth;
                }
            }

            return result;
        }


        public StackData FindDataByType( ref UIType t)
        {
            for (int i = 0; i < mListWindow.Count; i++)
            {
                if (mListWindow[i].mInfo.Key == t)
                    return mListWindow[i];
            }

            return null;
        }


        public void OnPushTrigger( ref StackData data)
        {
            if ((data.mInfo.WinEffect & UIWindowEffect.CommonBack) != 0)
            {
                if (IsHaveGoback(data.mObject) == false)
                    AddGobackToObject(data.mObject, UIManager.Instance.mPrefabGoback);
            }

            data.mObject.SetActive(true);
        }


        public void OnPopTrigger( ref StackData data)
        {
            data.mObject.SetActive(false);
        }



        private void AddGobackToObject( GameObject obj, GameObject prefab)
        {
            GameObject newObj = NGUITools.AddChild(obj, prefab);
            newObj.transform.SetAsLastSibling();
            newObj.name = "_Goback";
        }


        private bool IsHaveGoback( GameObject obj )
        {
            if (obj == null) return false;
            return obj.transform.FindChild("_Goback") != null;
        }
    }
}
