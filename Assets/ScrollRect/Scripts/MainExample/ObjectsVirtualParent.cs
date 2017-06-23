using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
    /// <summary>When this component or gameObject is disabled/enabled, the virtual children GameObjects will also be actived/de-activated</summary>
    public class ObjectsVirtualParent : MonoBehaviour
    {
        public GameObject realParent;
        public int startIndex, count;
        public GameObject[] virtualChildren;


        void OnEnable()
        {
            if (virtualChildren.Length == 0)
            {
                var list = new List<GameObject>();
                for (int i = startIndex; i < startIndex + count; ++i)
                {
                    list.Add(realParent.transform.GetChild(i).gameObject);
                }
                virtualChildren = list.ToArray();
            }

            foreach (var c in virtualChildren)
                c.SetActive(true);
        }

        void OnDisable()
        {
            foreach (var c in virtualChildren)
                c.SetActive(false);
        }
    }
}
