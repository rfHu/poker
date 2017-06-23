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
    /// <para>An optimized adapter for a GridView </para>
    /// <para>Implements <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}"/> to simulate a grid by using</para>
    /// <para>a runtime-generated "row" prefab (or "colum" prefab, if horizontal ScrollRect), having a Horizontal (or Vertical, respectively) LayoutGroup component, inside which its corresponding cells will lie.</para>
    /// <para>This prefab is represented by a <see cref="CellGroupViewsHolder{TCellVH}"/>, which nicely abstractizes the mechanism to using cell prefabs. This view holder is managed internally and is no concern for most users.</para> 
    /// <para>The cell prefab is used the same way as the "item prefab", for those already familiarized with the ListView examples. It is represented</para>
    /// <para>by a <see cref="CellViewsHolder"/>, which are the actual view holders you need to create/update and nothing else. </para>
    /// </summary>
    /// <typeparam name="TParams">Must inherit from GridParams. See also <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.Parameters"/></typeparam>
    /// <typeparam name="TCellVH">The view holder type to use for the cell. Must inherit from CellViewsHolder</typeparam>
    public abstract class GridAdapter<TParams, TCellVH> : ScrollRectItemsAdapter8<TParams, CellGroupViewsHolder<TCellVH>> 
        where TParams : GridParams
        where TCellVH : CellViewsHolder, new()
    {
        protected int _CellsCount;

        /// <summary> Calculates how many groups are needed in order to fit a number of <paramref name="cellsCount"/> cells and passes that number to the base implementation</summary>
        /// <seealso cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.ChangeItemCountTo(int)"/>
        public override void ChangeItemCountTo(int cellsCount)
        {
            _CellsCount = cellsCount;

            // The number of groups is passed to the base's implementation
            int groupsCount = _Params.GetNumberOfRequiredGroups(_CellsCount);

            base.ChangeItemCountTo(groupsCount);
        }


		#region Cell view holders helpers
		/// <summary>The number of visible cells</summary>
		public virtual int GetNumVisibleCells()
		{
			if (_VisibleItemsCount == 0)
				return 0;
			return (_VisibleItemsCount - 1) * _Params.numCellsPerGroup + _VisibleItems[_VisibleItemsCount - 1].NumActiveCells;
		}

		/// <summary>
		/// <para>Retrieve the view holder of a cell with speciffic index in view. For example, one can iterate from 0 to <see cref="GetNumVisibleCells"/> </para>
		/// <para>in order to do something with each visible cell. Not to be mistaken for <see cref="GetCellViewsHolderIfVisible(int)"/>,</para>
		/// <para>which retrieves a cell by the index of its corresponding model in your data list (<see cref="AbstractViewHolder.itemIndex"/>)</para>
		/// </summary>
		public virtual TCellVH GetCellViewsHolder(int cellViewHolderIndex)
		{
			if (_VisibleItemsCount == 0)
				return null;

			if (cellViewHolderIndex > GetNumVisibleCells() - 1)
				return null;

			return _VisibleItems[_Params.GetGroupIndex(cellViewHolderIndex)]
					.ContainingCellViewHolders[cellViewHolderIndex % _Params.numCellsPerGroup];
		}

		/// <summary>
		/// <para>Retrieve the view holder of a cell whose associated model's index in your data list is <paramref name="withCellItemIndex"/>.</para>
		/// <para>Not to be mistaken for <see cref="GetCellViewsHolder(int)"/> which retrieves a cell by its index in view</para>
		/// </summary>
		/// <returns>null, if the item is outside the viewport (and thus no view is associated with it)</returns>
		public virtual TCellVH GetCellViewsHolderIfVisible(int withCellItemIndex)
		{
			var groupVH = GetItemViewsHolderIfVisible(_Params.GetGroupIndex(withCellItemIndex));
			if (groupVH == null)
				return null;

			int indexOfFirstCellInGroup = groupVH.itemIndex * _Params.numCellsPerGroup;

			if (withCellItemIndex < indexOfFirstCellInGroup + groupVH.NumActiveCells)
				return groupVH.ContainingCellViewHolders[withCellItemIndex - indexOfFirstCellInGroup];

			return null;
		}
		#endregion


		/// By default, it assumes all groups have equal height
		/// <seealso cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.GetItemHeight(int)"/>
		protected override float GetItemHeight(int groupIndex)
        { return _Params.GetGroupHeight(); }

        /// By default, it assumes all groups have equal width
        /// <seealso cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.GetItemWidth(int)"/>
        protected override float GetItemWidth(int groupIndex)
        { return _Params.GetGroupWidth(); }

        /// <summary> Creates the Group viewholder which instantiates the group prefab using the provided params in <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.Init(TParams)"/></summary>
        /// <seealso cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.CreateViewsHolder(int)"/>
        /// <param name="itemIndex">the index of the GROUP (attention, not the CELL) that needs creation</param>
        /// <returns>The created group views holder </returns>
        protected override CellGroupViewsHolder<TCellVH> CreateViewsHolder(int itemIndex)
        {
            var instance = new CellGroupViewsHolder<TCellVH>();
            instance.Init(_Params.GetGroupPrefab(itemIndex).gameObject, itemIndex, _Params.cellPrefab, _Params.numCellsPerGroup);

            return instance;
        }

        /// <summary>Here the grid adapter checks if new groups need to be created or if old ones need to be disabled or destroyed, after which it calls <see cref="UpdateCellViewsHolder(TCellVH)"/> for each remaining cells</summary>
        /// <seealso cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.UpdateViewsHolder(TItemViewsHolder)"/>
        /// <param name="newOrRecycled">The viewholder of the group that needs updated</param>
        protected override void UpdateViewsHolder(CellGroupViewsHolder<TCellVH> newOrRecycled)
        {
            // At this point there is for sure enough groups, but there may not be enough enabled cells, or there may be too much enabled cells

            int activeCellsForThisGroup;
            // If it's the last one
            if (newOrRecycled.itemIndex + 1 == GetItemCount())
            {
                int totalCellsBeforeThisGroup = 0;
                if (newOrRecycled.itemIndex > 0)
                {
                    totalCellsBeforeThisGroup = newOrRecycled.itemIndex * _Params.numCellsPerGroup;
                }
                activeCellsForThisGroup = _CellsCount - totalCellsBeforeThisGroup;
            }
            else
            {
                activeCellsForThisGroup = _Params.numCellsPerGroup;
            }
            newOrRecycled.NumActiveCells = activeCellsForThisGroup;

            for (int i = 0; i < activeCellsForThisGroup; ++i)
                UpdateCellViewsHolder(newOrRecycled.ContainingCellViewHolders[i]);
        }

		/// <summary>The only important callback for inheritors. It provides cell's views holder which has just become visible and whose views should be updated from its corresponding data model. viewHolder.itemIndex (<see cref="AbstractViewHolder.itemIndex"/>) can be used to know what data model is associated with. </summary>
		/// <param name="viewHolder">The cell's view holder</param>
		protected abstract void UpdateCellViewsHolder(TCellVH viewHolder);
	}
}
