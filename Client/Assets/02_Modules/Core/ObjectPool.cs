using System.Collections.Generic;
using UnityEngine;

namespace Vams2.Core
{
    public class ObjectPool<T> where T : MonoBehaviour
    {
        private GameObject mPrefab;
        private Queue<T> mPool;
        private Transform mParent;
        private int mActiveCount;

        public int ActiveCount => mActiveCount;

        public ObjectPool(GameObject prefab, int initialCount, Transform parent = null)
        {
            mPrefab = prefab;
            mPool = new Queue<T>();
            mParent = parent;
            mActiveCount = 0;

            for (int i = 0; i < initialCount; i++)
            {
                T obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                mPool.Enqueue(obj);
            }
        }

        public T Get()
        {
            T obj;
            if (mPool.Count > 0)
            {
                obj = mPool.Dequeue();
                obj.gameObject.SetActive(true);
            }
            else
            {
                obj = CreateNewObject();
            }
            mActiveCount++;
            return obj;
        }

        public void Return(T obj)
        {
            obj.gameObject.SetActive(false);
            mPool.Enqueue(obj);
            mActiveCount--;
        }

        public void ReturnAll(List<T> activeObjects)
        {
            for (int i = activeObjects.Count - 1; i >= 0; i--)
            {
                Return(activeObjects[i]);
            }
            activeObjects.Clear();
        }

        private T CreateNewObject()
        {
            GameObject go = Object.Instantiate(mPrefab, mParent);
            T component = go.GetComponent<T>();
            return component;
        }
    }
}
