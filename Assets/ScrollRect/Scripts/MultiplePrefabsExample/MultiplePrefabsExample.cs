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
    /// <summary>
    /// <para>Example implementation demonstrating the use of 2 different view holders, representing 2 different models into their own prefab, with a common Title property, displayed in a Text found on both prefabs. </para>
    /// <para>The only constrain is for the models to have a common ancestor class and for the view holders to also have a common ancestor class</para>
    /// <para>Also, the <see cref="BidirectionalModel"/> is used to demonstrate how the data can flow both from the model to the views, but also from the views to the model (i.e. this model updates when the user changes the value of its corresponding slider)</para>
    /// <para>At the core, everything's the same as in other example implementations, so if something's not clear, check them (SimpleTutorial is a good start)</para>
    /// </summary>
    public class MultiplePrefabsExample : MonoBehaviour
    {
        /// <summary>Configuration visible in the inspector</summary>
        public MyParams adapterParams;

        /// <summary>Holds the number of items which will be contained in the ScrollView</summary>
        public Text countText;

        /// <summary>Shows the average of all <see cref="BidirectionalModel.value"/>s in all models of type <see cref="BidirectionalModel"/></summary>
        public Text averageValuesInModelsText;

        // Instance of your ScrollRectItemsAdapter8 implementation
        MyScrollRectAdapter _Adapter;


        void Start()
        {
            _Adapter = new MyScrollRectAdapter();

            // Need to initialize adapter after a few frames, so Unity will finish initializing its layout
            StartCoroutine(DelayedInit());
        }

        void Update()
        {
            if (adapterParams == null || adapterParams.data == null)
            {
                averageValuesInModelsText.text = "0";
                return;
            }

            // Keeping the update rate the smaller the bigger the data set. Then, clamped between 1 and 60. 
            // This way, the performance of computing the average stays relatively constant, regardless of the data set
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
            // The adapter has some resources that need to be disposed after you destroy the scroll view
            if (_Adapter != null)
                _Adapter.Dispose();
        }

        // Initialize the adapter after 3 frames
        // You can also try calling Canvas.ForceUpdateCanvases() instead if you for some reason can't wait 3 frames, although it wasn't tested
        IEnumerator DelayedInit()
        {
            // Wait 3 frames
            yield return null;
            yield return null;
            yield return null;


            _Adapter.Init(adapterParams);

            // Initially set the number of items to the number in the input field
            UpdateItems();
        }

        /// <summary>Callback from UI Button. Parses the text in <see cref="countText"/> as an int and sets it as the new item count, deleting the old models & randomly creating enough new ones, then refreshing all the views</summary>
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

            // Just notify the adapter the data changed, so it can refresh the views (even if the count is the same, this must be done)
            _Adapter.ChangeItemCountTo(newModels.Length);
        }

        #region ScrollRectItemsAdapter8 code
        /// <summary>
        /// Contains the 2 prefabs associated with the 2 view holders & the data list containing models of the 2 type, stored as <see cref="BaseModel"/>
        /// </summary>
        [Serializable] // serializable, so it can be shown in inspector
        public class MyParams : BaseParams
        {
            public RectTransform bidirectionalPrefab, expandablePrefab;

            [NonSerialized]
            public List<BaseModel> data = new List<BaseModel>();

            //Dictionary<Type, RectTransform> _MapModelTypeToItemPrefab;


            //public RectTransform GetPrefabForModelType(Type modelType)
            //{
            //    // Lazy-initialization
            //    if (_MapModelTypeToItemPrefab == null)
            //    {
            //        _MapModelTypeToItemPrefab[typeof(BidirectionalModel)] = bidirectionalPrefab;
            //        _MapModelTypeToItemPrefab[typeof(ExpandableModel)] = expandablePrefab;
            //    }

            //    return _MapModelTypeToItemPrefab[modelType];
            //}
        }


        public class MyScrollRectAdapter : ScrollRectItemsAdapter8<MyParams, ViewHolders.BaseVH>, ExpandCollapseOnClick.ISizeChangesHandler
        {
            // Will be called for vertical scroll views
            protected override float GetItemHeight(int index)
            { return _Params.data[index].visualSize; }

            // Will be called for horizontal scroll views
            protected override float GetItemWidth(int index)
            { return _Params.data[index].visualSize; }

            /// <summary>
            /// Creates either a <see cref="BidirectionalVH"/> or a <see cref="ExpandableVH"/>, depending on the type of the model at index <paramref name="itemIndex"/>. Calls <see cref="AbstractViewHolder.Init(RectTransform, int, bool, bool)"/> on it, which instantiates the prefab etc.
            /// </summary>
            /// <seealso cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.CreateViewsHolder(int)"/>
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

                    instance.expandCollapseOnClickBehaviour.sizeChangesHandler = this;

                    return instance;
                }
                else
                    throw new InvalidOperationException("Unrecognized model type: " + modelType.Name);
            }

            /// <inheritdoc/>
            protected override void UpdateViewsHolder(BaseVH newOrRecycled)
            {
                // Initialize/update the views from the associated model
                BaseModel model = _Params.data[newOrRecycled.itemIndex];
                newOrRecycled.UpdateViews(model/*, _Sizes[newOrRecycled.itemIndex]*/);
            }

            /// <summary>Overriding the base implementation, which always returns true. In this case, a view holder is recyclable only if its <see cref="BaseVH.CanPresentModelType(Type)"/> returns true for the model at index <paramref name="indexOfItemThatWillBecomeVisible"/></summary>
            /// <seealso cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.IsRecyclable(TItemViewsHolder, int, float)"/>
            protected override bool IsRecyclable(BaseVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
            { return potentiallyRecyclable.CanPresentModelType(_Params.data[indexOfItemThatWillBecomeVisible].cachedType); }

            #region ExpandCollapseOnClick.ISizeChangesHandler implementation
            bool ExpandCollapseOnClick.ISizeChangesHandler.HandleSizeChangeRequest(RectTransform rt, float newSize)
            {
                var vh = GetItemViewsHolderIfVisible(rt);

                // If the vh is visible and the request is accepted, we update our list of sizes
                if (vh != null)
                {
                    float resolvedSize = RequestChangeItemSizeAndUpdateLayout(vh, newSize);
                    if (resolvedSize != -1f)
                    {
                        //_Sizes[vh.itemIndex] = newSize;
                        _Params.data[vh.itemIndex].visualSize = newSize;

                        return true;
                    }
                }

                return false;
            }

            public void OnExpandedStateChanged(RectTransform rt, bool expanded)
            {
                var vh = GetItemViewsHolderIfVisible(rt);

                // If the vh is visible and the request is accepted, we update the model's "expanded" field
                if (vh != null)
                {
                    var asExpandableModel = _Params.data[vh.itemIndex] as ExpandableModel;
                    if (asExpandableModel == null)
                        throw new UnityException(
                            "MultiplePrefabsExample.MyScrollRectAdapter.OnExpandedStateChanged: item model at index " + vh.itemIndex
                            + " is not of type " + typeof(ExpandableModel).Name + ", as expected by the view holder having this itemIndex. You messed up. Happy debugging :)");
                    asExpandableModel.expanded = expanded;
                }
            }
            #endregion
        }
        #endregion

    }
}
