using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.Logic.Misc.Other.Extensions;
using frame8.ScrollRectItemsAdapter.Util.GridView;
using frame8.ScrollRectItemsAdapter.Util;

namespace frame8.ScrollRectItemsAdapter.SelectionExample
{
    /// <summary>
    /// Implementation demonstrating the usage of a <see cref="GridAdapter{TParams, TCellVH}"/> with support for selecting items on long click & deleting them
    /// </summary>
    public class SelectionExample : MonoBehaviour
    {
        /// <summary>Configuration visible in the inspector</summary>
        public MyGridParams gridParams;

        // Instance of the GridAdapter implementation
        MyGridAdapter _GridAdapter;


        void Start()
        {
            _GridAdapter = new MyGridAdapter();

            gridParams.deleteButton.onClick.AddListener(DeleteSelectedItems);
            gridParams.cancelButton.onClick.AddListener(() => _GridAdapter.SelectionMode = false);

            // Need to initialize adapter after a few frames, so Unity will finish initializing its layout
            StartCoroutine(DelayedInit());
        }

        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
                _GridAdapter.SelectionMode = false;
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

        #region UI callbacks
        /// <summary>Callback from UI Button. Parses the text in <see cref="countText"/> as an int and sets it as the new item count, refreshing all the views</summary>
        public void UpdateItems()
        {
            int newCount;
            int.TryParse(gridParams.countText.text, out newCount);

            // Generating some random models
            var models = new BasicModel[newCount];
            for (int i = 0; i < newCount; ++i)
            {
                models[i] = new BasicModel();
                models[i].title = "Item " + i;
            }
            _GridAdapter.ChangeModels(models);
        }

        /// <summary>Deletes the selected items</summary>
        void DeleteSelectedItems()
        {
            var toBeDeleted = new List<BasicModel>();
            for (int i = 0; i < _GridAdapter.DataCount; ++i)
            {
                var model = _GridAdapter.GetModel(i);
                if (model.isSelected)
                    toBeDeleted.Add(model);
            }

            if (toBeDeleted.Count > 0)
            {
                // Remove models from adapter & update views
                _GridAdapter.Remove(toBeDeleted.ToArray());

                // Re-enable selection mode
                if (gridParams.keepSelectionModeAfterDeletion)
                    _GridAdapter.SelectionMode = true;

                // Remove from disk
                foreach (var item in toBeDeleted)
                    HandleItemDeletion(item);
            }
        }
        #endregion

        void HandleItemDeletion(BasicModel model)
        {
            Debug.Log("Deleted: " + model.title);
        }


        [Serializable]
        public class MyGridParams : GridParams
        {
            /// <summary>Holds the number of items which will be contained in the ScrollView</summary>
            public Text countText;

            /// <summary>Will be enabled when in selection mode and there are items selsted. Disabled otherwise</summary>
            public Button deleteButton;

            /// <summary>Will be enabled when in selection mode. Pressing it will exit selection mode. Useful for devices with no back/escape (iOS)</summary>
            public Button cancelButton;

            /// <summary>Select the first item when entering selection mode</summary>
            public bool autoSelectFirstOnSelectionMode = true;

            /// <summary>Wether to remain in selection mode after deletion or not</summary>
            public bool keepSelectionModeAfterDeletion = true;
        }

        public class BasicModel
        {
            // Data state
            public string title;

            // View state
            public bool isSelected;
        }

        /// <summary>All view holders used with GridAdapter should inherit from <see cref="CellViewsHolder"/></summary>
        public class MyCellViewsHolder : CellViewsHolder
        {
            public Text title;
            public Toggle toggle;
            public LongClickableItem longClickableComponent;


            public override void CollectViews()
            {
                base.CollectViews();

                toggle = views.Find("Toggle").GetComponent<Toggle>();
                title = views.Find("TitleText").GetComponent<Text>();
                longClickableComponent = views.Find("LongClickableArea").GetComponent<LongClickableItem>();
            }

            protected override RectTransform GetViews() { return root.Find("Views") as RectTransform; }
        }


        #region ScrollRectItemsAdapter8 code
        public class MyGridAdapter : GridAdapter<MyGridParams, MyCellViewsHolder>, LongClickableItem.IItemLongClickListener
        {
            public int DataCount { get { return _Data.Count; } }
            public bool SelectionMode
            {
                get { return _SelectionMode; }
                set
                {
                    if (_SelectionMode != value)
                    {
                        SetSelectionMode(value);
						RefreshSelectionStateForVisibleCells();
						UpdateSelectionActionButtons();
                    }
                }
            }

            List<BasicModel> _Data = new List<BasicModel>();
            bool _SelectionMode;


            public override void ChangeItemCountTo(int cellsCount)
            {
				// Assure nothing is selected before changing the count
				// Update: not calling RefreshSelectionStateForVisibleCells(), since UpdateCellViewsHolder() will be called for all cells anyway
				if (_SelectionMode)
                    SetSelectionMode(false);
                UpdateSelectionActionButtons();

                base.ChangeItemCountTo(cellsCount);
            }

            /// <inheritdoc/>
            protected override CellGroupViewsHolder<MyCellViewsHolder> CreateViewsHolder(int itemIndex)
            {
                var cellsGroupVHInstance = base.CreateViewsHolder(itemIndex);

                // Set listeners for the Toggle in each cell. Will call OnCellToggled() when the toggled state changes
                // Set this adapter as listener for the OnItemLongClicked event
                for (int i = 0; i < cellsGroupVHInstance.ContainingCellViewHolders.Length; ++i)
                {
                    var cellVH = cellsGroupVHInstance.ContainingCellViewHolders[i];
                    cellVH.toggle.onValueChanged.AddListener(_ => OnCellToggled(cellVH));
                    cellVH.longClickableComponent.longClickListener = this;
                }

                return cellsGroupVHInstance;
            }

            /// <summary> Called when a cell becomes visible </summary>
            /// <param name="viewHolder"> use viewHolder.itemIndex to find your corresponding model and feed data into its views</param>
            protected override void UpdateCellViewsHolder(MyCellViewsHolder viewHolder)
            {
                var model = _Data[viewHolder.itemIndex];

                viewHolder.title.text = model.title;

                UpdateSelectionState(viewHolder, model);
            }

            void UpdateSelectionState(MyCellViewsHolder viewHolder, BasicModel model)
            {
                viewHolder.longClickableComponent.gameObject.SetActive(!_SelectionMode); // can be long-clicked only if selection mode is off
                viewHolder.toggle.gameObject.SetActive(_SelectionMode); // can be selected only if selection mode is on
                viewHolder.toggle.isOn = model.isSelected;
            }

            /// <summary>Assumes the current state of SelectionMode is different than <paramref name="active"/></summary>
            void SetSelectionMode(bool active)
            {
                _SelectionMode = active;

                // Entering or exiting selection mode, each model should be marked as NON-selected beforehand
                for (int i = 0; i < DataCount; ++i)
                    _Data[i].isSelected = false;
            }

			void UpdateSelectionActionButtons()
            {
                if (!_SelectionMode)
                    _Params.deleteButton.interactable = false;

                _Params.cancelButton.interactable = _SelectionMode;
            }

            // Common utility methods to manipulate the data list
            public void Add(params BasicModel[] newModels)
            {
                _Data.AddRange(newModels);
                ChangeItemCountTo(_Data.Count);
            }
            public void Remove(params BasicModel[] toBeRemoved)
            {
                for (int i = 0; i < toBeRemoved.Length; ++i)
                    _Data.Remove(toBeRemoved[i]);
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
            public BasicModel GetModel(int index)
            { return _Data[index]; }
			

			void RefreshSelectionStateForVisibleCells()
			{
				// Rather than calling Refresh, which calls GetItemHeight etc. again for each item, visible or not, we retrieve the already-visible ones and update them manually (less lag)
				int visibleCellCount = GetNumVisibleCells();
				for (int i = 0; i < visibleCellCount; ++i)
				{
					var cellVH = GetCellViewsHolder(i);
					UpdateSelectionState(cellVH, _Data[cellVH.itemIndex]);
				}
			}


			#region LongClickableItem.IItemLongClickListener implementation
			public void OnItemLongClicked(LongClickableItem longClickedItem)
            {
                // Enter selection mode
                SetSelectionMode(true);
				RefreshSelectionStateForVisibleCells();
                UpdateSelectionActionButtons();

                if (_Params.autoSelectFirstOnSelectionMode)
                {
                    // Find the cell view holder that corresponds to the LongClickableItem parameter & mark it as toggled
                    int visibleCellCount = GetNumVisibleCells();
                    for (int i = 0; i < visibleCellCount; ++i)
                    {
                        var cellVH = GetCellViewsHolder(i);

                        if (cellVH.longClickableComponent == longClickedItem)
                        {
                            var model = _Data[cellVH.itemIndex];
                            model.isSelected = true;
                            UpdateSelectionState(cellVH, model);

                            break;
                        }
                    }
                }
            }
            #endregion

            void OnCellToggled(MyCellViewsHolder cellVH)
            {
                // Update the model this cell is representing
                _Data[cellVH.itemIndex].isSelected = cellVH.toggle.isOn;

                // Activate the delete button if at least one item was selected
                if (cellVH.toggle.isOn) // selected
                    _Params.deleteButton.interactable = true;
                else // de-selected
                {
                    foreach (var model in _Data)
                        if (model.isSelected)
                            return;

                    // No item selected => disable the delete button
                    _Params.deleteButton.interactable = false;
                }
            }
        }
        #endregion
    }
}
