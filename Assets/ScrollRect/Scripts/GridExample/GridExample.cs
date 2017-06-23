using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using frame8.ScrollRectItemsAdapter.Util;

namespace frame8.ScrollRectItemsAdapter.GridExample
{
    /// <summary>
    /// Implementation demonstrating the usage of a <see cref="GridAdapter{TParams, TCellVH}"/> for a simple gallery of remote images downloaded with a <see cref="SimpleImageDownloader"/>
    /// </summary>
    public class GridExample : MonoBehaviour
    {
        /// <summary>Configuration visible in the inspector</summary>
        public GridParams gridParams;

        /// <summary>Holds the number of items which will be contained in the ScrollView</summary>
        public Text countText;

        // Instance of the GridAdapter implementation
        MyGridAdapter _GridAdapter;


        void Start()
        {
            _GridAdapter = new MyGridAdapter();

            // Need to initialize adapter after a few frames, so Unity will finish initializing its layout
            StartCoroutine(DelayedInit());
        }

        void OnDestroy()
        {
            // The adapter has some resources that need to be disposed after you destroy the scroll view
            if (_GridAdapter != null)
                _GridAdapter.Dispose();
        }

        // Initialize the adapter after 3 frames
        // You can also try calling Canvas.ForceUpdateCanvases() instead if you for some reason can't wait 3 frames, although it wasn't tested
        IEnumerator DelayedInit()
        {
            // Wait 3 frames
            yield return null;
            yield return null;
            yield return null;

            _GridAdapter.Init(gridParams);

            // Initially set the number of items to the number in the input field
            UpdateItems();
        }

        /// <summary>Callback from UI Button. Parses the text in <see cref="countText"/> as an int and sets it as the new item count, refreshing all the views</summary>
        public void UpdateItems()
        {
            int newCount;
            int.TryParse(countText.text, out newCount);

            // Generating some random models
            var models = new BasicModel[newCount];
            for (int i = 0; i < newCount; ++i)
            {
                models[i] = new BasicModel();
                models[i].title = "Item " + i;
                models[i].imageURL = C.GetRandomSmallImageURL();
            }
            _GridAdapter.ChangeModels(models);
        }

        // Testing the SmoothScrollTo functionality & showing how to transform from item space (i.e. "group" space, i.e. row or column) to cell space 
        public void ScrollToItemWithIndex10()
        {
            if (_GridAdapter != null && _GridAdapter.CellCount > 10)
                _GridAdapter.SmoothScrollTo(gridParams.GetGroupIndex(10), 1f);
        }

        // This is your model
        public class BasicModel
        {
            public string title;
            public string imageURL;
        }

        /// <summary>
        /// All view holder used with GridAdapter should inherit from <see cref="CellViewsHolder"/>
        /// </summary>
        public class MyCellViewsHolder : CellViewsHolder
        {
            public RawImage icon; // using a raw image because it works with less code when we already have a Texture2D (downloaded from www with SimpleImageDownloader)
            public Image loadingProgress; 
            public Text title;


            public override void CollectViews()
            {
                base.CollectViews();

                icon = views.Find("IconRawImage").GetComponent<RawImage>();
                loadingProgress = views.Find("LoadingProgressImage").GetComponent<Image>();
                title = views.Find("TitleText").GetComponent<Text>();
            }

            protected override RectTransform GetViews() { return root.Find("Views") as RectTransform; }
        }


        #region ScrollRectItemsAdapter8 code
        public class MyGridAdapter : GridAdapter<GridParams, MyCellViewsHolder>
        {
            public int CellCount { get { return _Data.Count; } }

            List<BasicModel> _Data = new List<BasicModel>();


            /// <summary> Called when a cell becomes visible </summary>
            /// <param name="viewHolder"> use viewHolder.itemIndex to find your corresponding model and feed data into its views</param>
            protected override void UpdateCellViewsHolder(MyCellViewsHolder viewHolder)
            {
                var model = _Data[viewHolder.itemIndex];

                viewHolder.icon.enabled = false;
                viewHolder.title.text = "Loading";
                int itemIdexAtRequest = viewHolder.itemIndex;

                string requestedPath = model.imageURL;
                var request = new SimpleImageDownloader.Request()
                {
                    url = requestedPath,
                    onDone = result =>
                    {
                        if (IsModelStillValid(viewHolder.itemIndex, itemIdexAtRequest, requestedPath))
                        {
                            viewHolder.title.text = model.title;
                            viewHolder.icon.enabled = true;
                            //viewHolder.loadingProgress.fillAmount = 0f;
                            if (viewHolder.icon.texture)
                            {
                                var as2D = viewHolder.icon.texture as Texture2D;
                                if (as2D)
                                {
                                    result.LoadTextureInto(as2D); // re-use Texture2D object

                                    return;
                                }

                                Destroy(viewHolder.icon.texture); // texture type incompatible => destroy it
                            }

                            viewHolder.icon.texture = result.CreateTextureFromReceivedData();
                        }
                    },
                    onError = () =>
                    {
                        if (IsModelStillValid(viewHolder.itemIndex, itemIdexAtRequest, requestedPath))
                            viewHolder.title.text = "No connection";
                    }

                };

                SimpleImageDownloader.Instance.Enqueue(request);
            }

            // Common utility methods to manipulate the data list
            public void Add(params BasicModel[] newModels)
            {
                _Data.AddRange(newModels);
                ChangeItemCountTo(_Data.Count);
            }
            public void Remove(BasicModel newModel)
            {
                _Data.Add(newModel);
                ChangeItemCountTo(_Data.Count);
            }
            public void ChangeModels(BasicModel[] newModels)
            {
                _Data.Clear();
                Add(newModels);
            }
            public void Clear()
            {
                _Data.Clear();
                ChangeItemCountTo(_Data.Count);
            }

            bool IsModelStillValid(int itemIndex, int itemIdexAtRequest, string imageURLAtRequest)
            {
                return
                    _Data.Count > itemIndex // be sure the index still points to a valid model
                    && itemIdexAtRequest == itemIndex // be sure the view's associated model index is the same (i.e. the viewHolder wasn't re-used)
                    && imageURLAtRequest == _Data[itemIndex].imageURL; // be sure the model at that index is the same (could have changed if ChangeItemCountTo would've been called meanwhile)
            }
        }
        #endregion

    }
}
