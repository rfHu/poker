using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.GridView
{
    /// <summary>
    /// <para>Base class to inherit from in order to use <see cref="GridAdapter{TParams, TCellVH}"/></para>
    /// </summary>
    [Serializable] // serializable, so it can be shown in inspector
    public class GridParams : BaseParams, ISerializationCallbackReceiver
    {
        /// <summary>The prefab to use for each cell</summary>
        [Header("Grid")]
        public RectTransform cellPrefab;

        /// <summary>The max. number of cells in a row (for vertical ScrollView) or column (for horizontal ScrollView)</summary>
        [Tooltip("The max. number of cells in a row group (for vertical ScrollView) or column group (for horizontal ScrollView)")]
        public int numCellsPerGroup;

        /// <summary>The alignment of cells inside their parent LayoutGroup (Vertical or Horizontal, depending on ScrollView's orientation)</summary>
        public TextAnchor alignmentOfCellsInGroup = TextAnchor.UpperLeft;

        /// <summary>The padding of cells as a whole inside their parent LayoutGroup</summary>
        public RectOffset groupPadding;

        /// <summary>Wether to force the cells to expand in width inside their parent LayoutGroup</summary>
        public bool cellWidthForceExpandInGroup;

        /// <summary>Wether to force the cells to expand in height inside their parent LayoutGroup</summary>
        public bool cellHeightForceExpandInGroup;

        /// <summary>Cached prefab, auto-generated at runtime, first time <see cref="GetGroupPrefab(int)"/> is called</summary>
        HorizontalOrVerticalLayoutGroup _TheOnlyGroupPrefab;
        int _NumCellsPerGroupHorizontally = 1, _NumCellsPerGroupVertically = 1; // Both of these should be at least 1


        /// <summary>Returns the prefab to use as LayoutGroup for the group with index <paramref name="forGroupAtThisIndex"/></summary>
        public virtual HorizontalOrVerticalLayoutGroup GetGroupPrefab(int forGroupAtThisIndex)
        {
            if (_TheOnlyGroupPrefab == null)
            {
                var go = new GameObject(scrollRect.name + "_GridAdapter_GroupPrefab");
                go.SetActive(false);
                go.transform.SetParent(scrollRect.transform, false);
                if (scrollRect.horizontal)
                {
                    _TheOnlyGroupPrefab = go.AddComponent<VerticalLayoutGroup>(); // groups are columns in a horizontal scrollview
                }
                else
                {
                    _TheOnlyGroupPrefab = go.AddComponent<HorizontalLayoutGroup>(); // groups are rows in a vertical scrollview
                }
                _TheOnlyGroupPrefab.spacing = contentSpacing;
                _TheOnlyGroupPrefab.childForceExpandWidth = cellWidthForceExpandInGroup;
                _TheOnlyGroupPrefab.childForceExpandHeight = cellHeightForceExpandInGroup;
                _TheOnlyGroupPrefab.childAlignment = alignmentOfCellsInGroup;
                _TheOnlyGroupPrefab.padding = groupPadding;
            }

            return _TheOnlyGroupPrefab;
        }

        /// <summary>Approximates the group width using the provided padding, cell prefab's width, the space</summary>
        /// <returns></returns>
        public virtual float GetGroupWidth()
        { return groupPadding.left + (cellPrefab.rect.width + contentSpacing) * _NumCellsPerGroupHorizontally - contentSpacing + groupPadding.right; }

        public virtual float GetGroupHeight()
        { return groupPadding.top  + (cellPrefab.rect.height + contentSpacing)* _NumCellsPerGroupVertically   - contentSpacing + groupPadding.bottom; }

        //public virtual int GetCellIndex(int groupIndex) { }
        public virtual int GetGroupIndex(int cellIndex) { return cellIndex / numCellsPerGroup; }

        public virtual int GetNumberOfRequiredGroups(int numberOfCells) { return numberOfCells == 0 ? 0 : GetGroupIndex(numberOfCells - 1) + 1; }

        protected virtual void InitNonSerializedData()
        {
            if (scrollRect.horizontal)
                _NumCellsPerGroupVertically = numCellsPerGroup;
            else
                _NumCellsPerGroupHorizontally = numCellsPerGroup;
        }

        #region ISerializationCallbackReceiver
        public void OnBeforeSerialize() {}
        public void OnAfterDeserialize() { InitNonSerializedData(); }
        #endregion
    }
}
