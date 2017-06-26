using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
    /// <summary>
    /// <para>The main example implementation demonstrating common (not all) functionalities: </para>
    /// <para>- using both a horizontal and a vertical ScrollRect with a complex prefab, </para>
    /// <para>- changing the item count, </para>
    /// <para>- expanding/collapsing an item, </para>
    /// <para>- smooth scrolling to an item &amp; optionally doing an action after the animation is done, </para>
    /// <para>- random item sizes,</para>
    /// <para>- comparing the performance to the default implementation of a ScrollView,</para>
    /// <para>- the use of <see cref="ScrollbarFixer8"/></para>
    /// <para>At the core, everything's the same as in other example implementations, so if something's not clear, check them (SimpleTutorial is a good start)</para>
    /// </summary>
    public class ScrollRectItemsAdapterExample : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField]
        MyParams _ScrollRectAdapterParams;
#pragma warning restore 0649

        MyScrollRectItemsAdapter _ScrollRectItemsAdapter;
        List<SampleObjectModel> _Data;

        public BaseParams Params { get { return _ScrollRectAdapterParams; } }
        public MyScrollRectItemsAdapter Adapter { get { return _ScrollRectItemsAdapter; } }
        public List<SampleObjectModel> Data { get { return _Data; } }


        IEnumerator Start()
        {
            // Wait for Unity's layout (UI scaling etc.)
            yield return null;
            yield return null;
            yield return null;

            _Data = new List<SampleObjectModel>();
            _ScrollRectItemsAdapter = new MyScrollRectItemsAdapter(_Data, _ScrollRectAdapterParams);
            _ScrollRectAdapterParams.updateItemsButton.onClick.AddListener(UpdateItems);

            // Initially set the number of items to the number in the input field
            UpdateItems();
        }

        void OnDestroy()
        {
            if (_ScrollRectItemsAdapter != null)
                _ScrollRectItemsAdapter.Dispose();
        }

        // This is your data model
        // this one will generate 5 random colors
        public class SampleObjectModel
        {
            public string objectName;
            public Color aColor, bColor, cColor, dColor, eColor;
            public bool expanded;

            public SampleObjectModel(string name)
            {
                objectName = name;
                aColor = GetRandomColor();
                bColor = GetRandomColor();
                cColor = GetRandomColor();
                dColor = GetRandomColor();
                eColor = GetRandomColor();
            }

            Color GetRandomColor()
            {
                return new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            }
        }

        public sealed class MyItemViewsHolder : BaseItemViewsHolder
        {
            public Text objectTitle;
            public Image a, b, c, d, e;
            public ExpandCollapseOnClick expandOnCollapseComponent;

            public override void CollectViews()
            {
                base.CollectViews();

                a = root.GetChild(0).GetChild(0).GetComponent<Image>();
                b = root.GetChild(0).GetChild(1).GetComponent<Image>();
                c = root.GetChild(0).GetChild(2).GetComponent<Image>();
                d = root.GetChild(0).GetChild(3).GetComponent<Image>();
                e = root.GetChild(0).GetChild(4).GetComponent<Image>();

                objectTitle = root.GetChild(0).GetChild(5).GetComponentInChildren<Text>();

                expandOnCollapseComponent = root.GetComponent<ExpandCollapseOnClick>();
            }
        }

        void UpdateItems()
        {
            _Data.Clear();
            int capacity = 0;
            int.TryParse(_ScrollRectAdapterParams.numItemsInputField.text, out capacity);

            _Data.Capacity = capacity;

            for (int i = 0; i < capacity; ++i)
                _Data.Add(new SampleObjectModel("Item " + i));

            _ScrollRectItemsAdapter.ChangeItemCountTo(capacity);
        }


        [Serializable]
        public class MyParams : BaseParams
        {
            public RectTransform itemPrefab;
            public Toggle randomizeSizesToggle;
            public InputField numItemsInputField;
            public Button updateItemsButton;
        }

        /// <summary>
        /// <para>At the core, it's the same as <see cref="SimpleTutorial"/>, but it also re-generates the <see cref="_ItemsSizessToUse"/></para>
        /// <para>each time the count changes, optionally inserting random sizes if the "Randomize sizes" toggle is checked. And the data lsit is passed directly in the constructor rather than having it in <see cref="Params"/></para>
        /// </summary>
        public sealed class MyScrollRectItemsAdapter : ScrollRectItemsAdapter8<MyParams, MyItemViewsHolder>, ExpandCollapseOnClick.ISizeChangesHandler
        {
            bool _RandomizeSizes;
            float _PrefabSize;
            float[] _ItemsSizessToUse;
            List<SampleObjectModel> _Data;


            public MyScrollRectItemsAdapter(List<SampleObjectModel> data, MyParams parms)
            {
                _Data = data;

                if (parms.scrollRect.horizontal)
                    _PrefabSize = parms.itemPrefab.rect.width;
                else
                    _PrefabSize = parms.itemPrefab.rect.height;

                InitSizes(false);

                parms.randomizeSizesToggle.onValueChanged.AddListener((value) => _RandomizeSizes = value);

                // Need to call Init(Params) AFTER we init our stuff, because both GetItem[Height|Width]() and UpdateViewsHolder() will be called in this method
                Init(parms);
            }

            void InitSizes(bool random)
            {
                int newCount = _Data.Count;
                if (_ItemsSizessToUse == null || newCount != _ItemsSizessToUse.Length)
                    _ItemsSizessToUse = new float[newCount];

                if (random)
                    for (int i = 0; i < newCount; ++i)
                        _ItemsSizessToUse[i] = UnityEngine.Random.Range(30, 400);
                else
                    for (int i = 0; i < newCount; ++i)
                        _ItemsSizessToUse[i] = _PrefabSize;
            }

            public override void ChangeItemCountTo(int itemsCount)
            {
                // Keep the mocked heights array's size up to date, so the GetItemHeight(int) callback won't throw an index out of bounds exception
                InitSizes(_RandomizeSizes);

                base.ChangeItemCountTo(itemsCount);
            }

            // Remember, only GetItemHeight (for vertical scroll) or GetItemWidth (for horizontal scroll) will be called
            protected override float GetItemHeight(int index)
            {
                return _ItemsSizessToUse[index];
            }

            // Remember, only GetItemHeight (for vertical scroll) or GetItemWidth (for horizontal scroll) will be called
            protected override float GetItemWidth(int index)
            {
                return _ItemsSizessToUse[index];
            }

            protected override MyItemViewsHolder CreateViewsHolder(int itemIndex)
            {
                var instance = new MyItemViewsHolder();
                instance.Init(_Params.itemPrefab, itemIndex);
                instance.expandOnCollapseComponent.sizeChangesHandler = this;

                return instance;
            }

            protected override void UpdateViewsHolder(MyItemViewsHolder newOrRecycled)
            {
                // Populating with data from associated model
                SampleObjectModel dataModel = _Data[newOrRecycled.itemIndex];
                newOrRecycled.objectTitle.text = dataModel.objectName + " [Size:" + _ItemsSizessToUse[newOrRecycled.itemIndex] + "]";
                newOrRecycled.a.color = dataModel.aColor;
                newOrRecycled.b.color = dataModel.bColor;
                newOrRecycled.c.color = dataModel.cColor;
                newOrRecycled.d.color = dataModel.dColor;
                newOrRecycled.e.color = dataModel.eColor;

                if (newOrRecycled.expandOnCollapseComponent)
                {
                    newOrRecycled.expandOnCollapseComponent.expanded = dataModel.expanded;
                    if (!dataModel.expanded)
                        newOrRecycled.expandOnCollapseComponent.nonExpandedSize = _ItemsSizessToUse[newOrRecycled.itemIndex];
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
                        _ItemsSizessToUse[vh.itemIndex] = newSize;

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
                    _Data[vh.itemIndex].expanded = expanded;
            }
            #endregion
        }
    }
}
