using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.ViewHolders;
using frame8.ScrollRectItemsAdapter.Util;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample
{
    public class MultiplePrefabsExample : MonoBehaviour
    {
        public MyParams adapterParams;

        public Text countText;

        public Text averageValuesInModelsText;

        MyScrollRectAdapter _Adapter;


        void Start()
        {
            _Adapter = new MyScrollRectAdapter();

            StartCoroutine(DelayedInit());
        }

        void Update()
        {
            if (adapterParams == null || adapterParams.data == null)
            {
                averageValuesInModelsText.text = "0";
                return;
            }

            float frameStep = (adapterParams.data.Count / 100000f);
            int frameStepInt = Mathf.Min(60, Mathf.Max(1, (int)(frameStep * 60)));

            if (Time.frameCount % frameStepInt == 0)
            {
                float avg = 0f;
                int bidiNum = 0;
                foreach (var model in adapterParams.data)
                {
                    var asBidi = model as BidirectionalModel;
                    if (asBidi != null)
                    {
                        ++bidiNum;
                        avg += asBidi.value;
                    }
                }
                averageValuesInModelsText.text = (avg / bidiNum).ToString("0.000");
            }
        }

        void OnDestroy()
        {
            if (_Adapter != null)
                _Adapter.Dispose();
        }

        IEnumerator DelayedInit()
        {
            yield return null;
            yield return null;
            yield return null;


            _Adapter.Init(adapterParams);

            UpdateItems();
        }

        public void UpdateItems()
        {
            int newCount;
            int.TryParse(countText.text, out newCount);


            // Generating some random models
            var newModels = new BaseModel[newCount];
            for (int i = 0; i < newCount; ++i)
            {
                BaseModel model;
                string titleValue = "[" + i + "]";
                Rect prefabRect;
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    model = new ExpandableModel() { title = titleValue, imageURL = C.GetRandomSmallImageURL() };
                    prefabRect = adapterParams.expandablePrefab.rect;
                }
                else
                {
                    model = new BidirectionalModel() { title = titleValue, value = UnityEngine.Random.Range(-5f, 5f) };
                    prefabRect = adapterParams.bidirectionalPrefab.rect;
                }
                model.visualSize = adapterParams.scrollRect.horizontal ? prefabRect.width : prefabRect.height;

                newModels[i] = model;
            }

            adapterParams.data.Clear();
            adapterParams.data.AddRange(newModels);

            _Adapter.ChangeItemCountTo(newModels.Length);
        }

        #region ScrollRectItemsAdapter8 code
        [Serializable] // serializable, so it can be shown in inspector
        public class MyParams : BaseParams
        {
            public RectTransform bidirectionalPrefab, expandablePrefab;

            public List<BaseModel> data = new List<BaseModel>();

        }


        public class MyScrollRectAdapter : ScrollRectItemsAdapter8<MyParams, ViewHolders.BaseVH> 
        {
            protected override float GetItemHeight(int index)
            { return _Params.data[index].visualSize; }

            protected override float GetItemWidth(int index)
            { return _Params.data[index].visualSize; }

            protected override BaseVH CreateViewsHolder(int itemIndex)
            {
                var modelType = _Params.data[itemIndex].cachedType;// _ModelTypes[itemIndex];
                if (modelType == typeof(BidirectionalModel)) // very efficient type comparison, since typeof() is evaluated at compile-time
                {
                    var instance = new BidirectionalVH();
                    instance.Init(_Params.bidirectionalPrefab, itemIndex);

                    return instance;
                }
                else if (modelType == typeof(ExpandableModel))
                {
                    var instance = new ExpandableVH();
                    instance.Init(_Params.expandablePrefab, itemIndex);
                    return instance;
                }
                else
                    throw new InvalidOperationException("Unrecognized model type: " + modelType.Name);
            }

            protected override void UpdateViewsHolder(BaseVH newOrRecycled)
            {
                BaseModel model = _Params.data[newOrRecycled.itemIndex];
                newOrRecycled.UpdateViews(model/*, _Sizes[newOrRecycled.itemIndex]*/);
            }

            protected override bool IsRecyclable(BaseVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
            { return potentiallyRecyclable.CanPresentModelType(_Params.data[indexOfItemThatWillBecomeVisible].cachedType); }

















            #region ExpandCollapseOnClick.ISizeChangesHandler implementation
            // bool ExpandCollapseOnClick.ISizeChangesHandler.HandleSizeChangeRequest(RectTransform rt, float newSize)
            // {
            //     var vh = GetItemViewsHolderIfVisible(rt);

            //     // If the vh is visible and the request is accepted, we update our list of sizes
            //     if (vh != null)
            //     {
            //         float resolvedSize = RequestChangeItemSizeAndUpdateLayout(vh, newSize);
            //         if (resolvedSize != -1f)
            //         {
            //             //_Sizes[vh.itemIndex] = newSize;
            //             _Params.data[vh.itemIndex].visualSize = newSize;

            //             return true;
            //         }
            //     }

            //     return false;
            // }

            // public void OnExpandedStateChanged(RectTransform rt, bool expanded)
            // {
            //     var vh = GetItemViewsHolderIfVisible(rt);

            //     // If the vh is visible and the request is accepted, we update the model's "expanded" field
            //     if (vh != null)
            //     {
            //         var asExpandableModel = _Params.data[vh.itemIndex] as ExpandableModel;
            //         if (asExpandableModel == null)
            //             throw new UnityException(
            //                 "MultiplePrefabsExample.MyScrollRectAdapter.OnExpandedStateChanged: item model at index " + vh.itemIndex
            //                 + " is not of type " + typeof(ExpandableModel).Name + ", as expected by the view holder having this itemIndex. You messed up. Happy debugging :)");
            //         asExpandableModel.expanded = expanded;
            //     }
            // }
            #endregion
        }
        #endregion

    }
}
