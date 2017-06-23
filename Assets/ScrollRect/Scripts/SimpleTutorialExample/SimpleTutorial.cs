using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.Util;

namespace frame8.ScrollRectItemsAdapter.SimpleTutorialExample
{
    /// <summary>Class implemented during this YouTube tutorial: https://youtu.be/aoqq_j-aV8I . It demonstrates a simple use case with items that expand/collapse on click</summary>
    public class SimpleTutorial : MonoBehaviour
    {
        /// <summary>Configuration visible in the inspector</summary>
        public MyParams adapterParams;

        /// <summary>Holds the number of items which will be contained in the ScrollView</summary>
        public Text countText;

        /// <summary>Fired when the number of items changes or refreshes</summary>
        public UnityEngine.Events.UnityEvent OnItemsUpdated;

        // Instance of your ScrollRectItemsAdapter8 implementation
        MyScrollRectAdapter _Adapter;


        void Start()
        {
            _Adapter = new MyScrollRectAdapter();

            // Need to initialize adapter after a few frames, so Unity will finish initializing its layout
            StartCoroutine(DelayedInit());
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

        /// <summary>Callback from UI Button. Parses the text in <see cref="countText"/> as an int and sets it as the new item count, refreshing all the views. It mocks a basic server communication where you request x items and you receive them after a few seconds</summary>
        public void UpdateItems()
        {
            int newCount;
            int.TryParse(countText.text, out newCount);
            StartCoroutine(FetchItemModelsFromServer(newCount, results => OnReceivedNewModels(results)));
        }

        // Testing the SmoothScrollTo functionality
        public void ScrollToItemWithIndex10()
        {
            if (adapterParams.data == null || adapterParams.data.Count > 10)
                _Adapter.SmoothScrollTo(10, 1f);
        }

        // Updating the models list and notify the adapter that it changed; 
        // it'll call GetItemHeight() for each item and UpdateViewsHolder for the visible ones
        void OnReceivedNewModels(ExampleItemModel[] models)
        {
            adapterParams.data.Clear();
            adapterParams.data.AddRange(models);

            // Just notify the adapter the data changed, so it can refresh the views (even if the count is the same, this must be done)
            _Adapter.ChangeItemCountTo(models.Length);

            if (OnItemsUpdated != null)
                OnItemsUpdated.Invoke();
        }

        IEnumerator FetchItemModelsFromServer(int count, Action<ExampleItemModel[]> onDone)
        {
            // Simulating server delay
            yield return new WaitForSeconds(.5f);

            // Generating some random models
            var results = new ExampleItemModel[count];
            for (int i = 0; i < count; ++i)
            {
                results[i] = new ExampleItemModel();
                results[i].title = "Item " + i;
                results[i].icon1Index = UnityEngine.Random.Range(0, adapterParams.availableIcons.Length);
                results[i].icon2Index = UnityEngine.Random.Range(0, adapterParams.availableIcons.Length);
                results[i].icon3Index = UnityEngine.Random.Range(0, adapterParams.availableIcons.Length);
            }

            onDone(results);
        }


        // This is your model
        public class ExampleItemModel
        {
            public string title;
            public int icon1Index, icon2Index, icon3Index;
            public bool expanded;
        }


        #region ScrollRectItemsAdapter8 code
        // This in almost all cases will contain the prefab and your list of models
        [Serializable] // serializable, so it can be shown in inspector
        public class MyParams : BaseParams
        {
            public Texture2D[] availableIcons; // used to randomly generate models;
            public RectTransform prefab;
            public List<ExampleItemModel> data = new List<ExampleItemModel>();
        }

        // Self-explanatory
        public class MyItemViewsHolder : BaseItemViewsHolder
        {
            public Text titleText;
            public RawImage icon1Image, icon2Image, icon3Image;
            internal ExpandCollapseOnClick expandOnCollapseComponent;

            public override void CollectViews()
            {
                base.CollectViews();

                titleText = root.Find("TitlePanel/TitleText").GetComponent<Text>();
                icon1Image = root.Find("Icon1Image").GetComponent<RawImage>();
                icon2Image = root.Find("Icon2Image").GetComponent<RawImage>();
                icon3Image = root.Find("TitlePanel/Icon3Image").GetComponent<RawImage>();
                expandOnCollapseComponent = root.GetComponent<ExpandCollapseOnClick>();
            }
        }

        /// <summary>
        /// <para>The custom adapter implementation that'll manage the list of items using rules customized for our use-case</para>
        /// <para>It implements <see cref="ExpandCollapseOnClick.ISizeChangesHandler"/> because it needs to keep track of the height of each item, and because they are expandable,</para>
        /// <para>we need to know when their sizes change externally (or actually are requested to change), in order to notify the adapter to push/pull the other items accordingly</para>
        /// </summary>
        public class MyScrollRectAdapter : ScrollRectItemsAdapter8<MyParams, MyItemViewsHolder>, ExpandCollapseOnClick.ISizeChangesHandler
        {
            /// <summary>The sizes are variable, so we need to keep track of them somewhere</summary>
            float[] sizes;


            // Will be called for vertical scroll views
            protected override float GetItemHeight(int index)
            {
                return sizes[index];
            }

            // Will be called for horizontal scroll views
            protected override float GetItemWidth(int index)
            {
                return sizes[index];
            }

            // Optionally handle the change item count command before/after calling the base implementation
            public override void ChangeItemCountTo(int itemsCount)
            {
                // Initializing the array of sizes.
                if (sizes == null || sizes.Length != itemsCount)
                    sizes = new float[itemsCount];

                // All sizes are the prefab's size, initially
                if (_Params.scrollRect.horizontal) for (int i = 0; i < itemsCount; ++i) sizes[i] = _Params.prefab.rect.width;
                else for (int i = 0; i < itemsCount; ++i) sizes[i] = _Params.prefab.rect.height;

                base.ChangeItemCountTo(itemsCount);
            }

            /// <summary>See <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.CreateViewsHolder(int)"/></summary>
            protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
            {
                var instance = new MyItemViewsHolder();
                instance.Init(_Params.prefab, itemIndex);
                instance.root.GetComponent<ExpandCollapseOnClick>().sizeChangesHandler = this;

                return instance;
            }

            /// <summary>See <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.UpdateViewsHolder(TItemViewsHolder)"/></summary>
            protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
            {
                // Initialize the views from the associated model
                ExampleItemModel model = _Params.data[newOrRecycled.itemIndex];

                newOrRecycled.titleText.text = model.title;
                newOrRecycled.icon1Image.texture = _Params.availableIcons[model.icon1Index];
                newOrRecycled.icon2Image.texture = _Params.availableIcons[model.icon2Index];
                newOrRecycled.icon3Image.texture = _Params.availableIcons[model.icon3Index];

                if (newOrRecycled.expandOnCollapseComponent)
                {
                    newOrRecycled.expandOnCollapseComponent.expanded = model.expanded;
                    if (!model.expanded)
                        newOrRecycled.expandOnCollapseComponent.nonExpandedSize = sizes[newOrRecycled.itemIndex];
                }
            }


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
                        sizes[vh.itemIndex] = newSize;

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
                    _Params.data[vh.itemIndex].expanded = expanded;
            }
            #endregion
        }
        #endregion

    }
}
