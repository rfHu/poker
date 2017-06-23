using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System;
using frame8.Logic.Core.MonoBehaviours;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.MonoBehaviours;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	/// <summary>
	/// <para>Sometimes abbreviated SRIA</para>
	/// <para>Base abstract class that you need to extend in order to provide an implementation for GetItem[Height/Width](int) and UpdateViewsHolder(TItemViewsHolder).
	/// You must first extend BaseItemViewsHolder, so you can provide it as the generic parameter TItemViewsHolder when implementing ScrollRectItemsAdapter8.
	/// Extending BaseParams is optional.Based on your needs.Provide it as generic parameter TParams when implementing ScrollRectItemsAdapter8</para>
	/// <para>HOW IT WORKS (it's recommended to manually go through example code in order to fully understand the mechanism):</para>
	/// <para>1. create your own implementation of BaseItemViewsHolder, let's name it MyItemViewsHolder</para>
	/// <para>2. create your own implementation of BaseParams (if needed), let's name it MyParams</para>
	/// <para>3. create your own implementation of ScrollRectItemsAdapter8&lt;MyParams, MyItemViewsHolder&gt;, let's name it MyScrollRectItemsAdapter</para>
	/// <para>4. instantiate MyScrollRectItemsAdapter</para>
	/// <para>5. call MyScrollRectItemsAdapter.ChangeItemCountTo(int) once (and any time your dataset is changed) and the following things will happen:</para>
	/// <para>    5.1. if the scroll rect has vertical scrolling, MyScrollRectItemsAdapter.GetItemHeight(int) will be called &lt;count&gt; times (with index going from 0 to &lt;count-1&gt;)</para>
	/// <para>       else if the scroll rect has horizontal scrolling, MyScrollRectItemsAdapter.GetItemWidth(int) will ...[idem above]...</para>
	/// <para>    5.2. CreateViewsHolder(int) will be called for each view that needs created. Once a view is created, it'll be re-used when it goes off-viewport </para>
	/// <para>          - newOrRecycledViewsHolder.root will be null, so you need to instantiate your prefab (or whatever), assign it and call newOrRecycledViewsHolder.CollectViews(). Alternatively, you can call its <see cref="AbstractViewHolder.Init(GameObject, int, bool, bool)"/> method, which can do a lot of things for you, mainly instantiate the prefab and (if you want) call CollectViews()</para>
	/// <para>          - after creation, only MyScrollRectItemsAdapter.UpdateViewsHolder() will be called for it when its represented item changes and becomes visible</para>
	/// <para>          - this method is also called when the viewport's size grows, thus needing more items to be visible at once</para>
	/// <para>    5.3. MyScrollRectItemsAdapter.UpdateViewsHolder(MyItemViewsHolder) will be called when an item is to be displayed or simply needs updating:</para>
	/// <para>        - use newOrRecycledViewsHolder.itemIndex to get the item index, so you can retrieve its associated model from your data set (most common practice is to store the data list in your Params implementation)</para>
	/// <para>        - newOrRecycledViewsHolder.root is not null here (given the view holder was properly created in CreateViewsHolder(..)). It's assigned a valid object whose UI elements only need their values changed (common practice is to implement helper methods in the view holder that take the model and update the views itself)</para>
	/// <para></para>
	/// <para> *NOTE: the vertical/horizontal LayoutGroup on content panel will be disabled, if any (you're not allowed to use it, since all the layouting is delegated to this adapter)</para>
	/// </summary>
	/// <typeparam name="TParams">The params type to use (the ones passed in <see cref="Init(TParams)"/>)</typeparam>
	/// <typeparam name="TItemViewsHolder"></typeparam>
	public abstract class ScrollRectItemsAdapter8<TParams, TItemViewsHolder> : OnScreenSizeChangedEventDispatcher.IOnScreenSizeChangedListener
	where TParams : BaseParams
	where TItemViewsHolder : BaseItemViewsHolder//, new()
	{
		/// <summary> Fired when the item count changes or the views are refreshed</summary>
		public event Action<int, int> ItemsRefreshed;

		/// <summary>The parameters passed in <see cref="Init(TParams)"/></summary>
		public TParams Parameters { get { return _Params; } }

		//public List<TItemViewsHolder> VisibleItemsCopy { get { return new List<TItemViewsHolder>(_VisibleItems); } }

		/// <summary> The number of currently visible items (view holders). Can be used to iterate through all of them using GetItemViewsHolder(int viewHolderIndex) </summary>
		public int VisibleItemsCount { get { return _VisibleItemsCount; } }


		protected TParams _Params;
		protected List<TItemViewsHolder> _VisibleItems;
		protected int _VisibleItemsCount;

		InternalState _InternalState;
		MonoBehaviourHelper8 _MonoBehaviourHelper;
		Coroutine _SmoothScrollCoroutine;


		protected ScrollRectItemsAdapter8() { }

		/// <summary>Initialize the adapter, passing your custom params. Will call Params.InitIfNeeded(), will initialize the internal state and will change the item count to 0</summary>
		/// <param name="parms">Your BaseParams implementation</param>
		public void Init(TParams parms)
		{
			if (_Params != null)
				Dispose();

			_Params = parms;
			_Params.InitIfNeeded();

			_InternalState = InternalState.CreateFromSourceParamsOrThrow(_Params, this);
			_VisibleItems = new List<TItemViewsHolder>();

			// Need to initialize before ChangeCountInternal, as _MonoBehaviourHelper is referenced there
			_MonoBehaviourHelper = MonoBehaviourHelper8.CreateInstance(MyUpdate, _Params.scrollRect.transform, "SRIA-Helper");
			_MonoBehaviourHelper.gameObject.AddComponent<OnScreenSizeChangedEventDispatcher>().RegisterListenerManually(this);

			ChangeItemCountInternal(0, false);
			_Params.ScrollToStart();
			_InternalState.ResetLastProcessedNormalizedScrollPositionToStart();
			_Params.scrollRect.onValueChanged.AddListener(OnScrollPositionChanged);
		}

		/// <summary>Same as ChangeItemCountTo(&lt;currentCount&gt;)</summary>
		public virtual void Refresh() { ChangeItemCountTo(_InternalState.itemsCount); }

		/// <summary>Same as ChangeItemCountTo(&lt;currentCount&gt;, false)</summary>
		public virtual void ChangeItemCountTo(int itemsCount) { ChangeItemCountTo(itemsCount, false); }

		/// <summary>Self-explanatory. Will remove all the current items and create new view holders for the new ones</summary>
		public void ChangeItemCountTo(int itemsCount, bool contentPanelEndEdgeStationary) { ChangeItemCountInternal(itemsCount, contentPanelEndEdgeStationary); }

		/// <summary>Always returns the last value that was passed to ChangeItemCountTo(...)</summary>
		public int GetItemCount() { return _InternalState.itemsCount; }

		/// <summary>
		/// <para>Get the viewHolder with a specific index in the "visible items" array.</para>
		/// <para>Example: if you pass 0, the first visible ViewHolder will be returned (if there's any)</para>
		/// <para>Not to be mistaken to the other method 'GetItemViewsHolderIfVisible(int withItemIndex)', which uses the itemIndex, i.e. the index in the list of data models.</para>
		/// <para>Returns null if the supplied parameter is >= VisibleItemsCount</para>
		/// </summary>
		/// <param name="viewHolderIndex"> the index of the ViewsHolder in the visible items array</param>
		public TItemViewsHolder GetItemViewsHolder(int viewHolderIndex)
		{
			if (viewHolderIndex >= _VisibleItemsCount)
				return null;
			return _VisibleItems[viewHolderIndex];
		}

		/// <summary>Gets the view holder representing the <paramref name="withItemIndex"/>'th item in the list of data models, if it's visible.</summary>
		/// <returns>null, if not visible</returns>
		public TItemViewsHolder GetItemViewsHolderIfVisible(int withItemIndex)
		{
			int curVisibleIndex = 0;
			int curIndexInList;
			TItemViewsHolder curItemViewsHolder;
			for (curVisibleIndex = 0; curVisibleIndex < _VisibleItemsCount; ++curVisibleIndex)
			{
				curItemViewsHolder = _VisibleItems[curVisibleIndex];
				curIndexInList = curItemViewsHolder.itemIndex;
				// Commented: with introduction of itemIndexInView, this chek is no longer useful
				//if (curIndexInList > withItemIndex) // the requested item is before the visible ones, so no viewsHolder for it
				//    break;
				if (curIndexInList == withItemIndex)
					return curItemViewsHolder;
			}

			return null;
		}

		/// <summary>Same as GetItemViewsHolderIfVisible(int withItemIndex), but searches by the root RectTransform reference, rather than the item index</summary>
		/// <param name="withRoot">RectTransform reference to the searched viw holder's root</param>
		public TItemViewsHolder GetItemViewsHolderIfVisible(RectTransform withRoot)
		{
			TItemViewsHolder curItemViewsHolder;
			for (int i = 0; i < _VisibleItemsCount; ++i)
			{
				curItemViewsHolder = _VisibleItems[i];
				if (curItemViewsHolder.root == withRoot)
					return curItemViewsHolder;
			}

			return null;
		}

		#region ScrollTo helper methods
		/// <summary> 
		/// <para>Aligns the ScrollRect's content so that the item with <paramref name="itemIndex"/> will be at the top.</para>
		/// <para>But the two optional parameters can be used for more fine-tuning. One common use-case is to set them both at 0.5 so the item will be end up exactly in the middle of the viewport</para>
		/// </summary>
		/// <param name="itemIndex">The item with this index will be considered</param>
		/// <param name="normalizedOffsetFromViewportStart">0f=no effect; 0.5f= the item's start edge (top or left) will be at the viewport's center; 1f=the item's start edge will be exactly at the viewport's end (thus, the item will be completely invisible)</param>
		/// <param name="normalizedPositionOfItemPivotToUse">For even more fine-adjustment, you can also specify what point on the item will be used to bring it to <paramref name="normalizedOffsetFromViewportStart"/>. The same principle applies as to the <paramref name="normalizedOffsetFromViewportStart"/> parameter: 0f=start(top/left), 1f=end(bottom/right)</param>
		public void ScrollTo(int itemIndex, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f)
		{
			float minContentOffsetFromVPAllowed = _InternalState.viewportSize - _InternalState.contentPanelSize;
			if (minContentOffsetFromVPAllowed >= 0f)
				return; // can't, because content is not bigger than viewport

			SetContentStartOffsetFromViewportStart(
				ClampContentStartOffsetFromViewportStart(minContentOffsetFromVPAllowed, itemIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse)
			);
		}

		/// <summary> Utility to smooth scroll. Identical to ScrollTo(..) in functionality, but the scroll is animated (scroll is done gradually, throughout multiple frames) </summary>
		/// <param name="onProgress">gets the progress (0f..1f) and returns if the scrolling should continue</param>
		/// <returns>if no smooth scroll animation was already playing. if it was, then no new animation will begin</returns>
		public bool SmoothScrollTo(int itemIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null)
		{
			if (_SmoothScrollCoroutine != null)
				return false;

			_SmoothScrollCoroutine = _MonoBehaviourHelper.StartCoroutine(SmoothScrollProgressCoroutine(itemIndex, duration, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse, onProgress));

			return true;
		}

		IEnumerator SmoothScrollProgressCoroutine(int itemIndex, float duration, float normalizedOffsetFromViewportStart = 0f, float normalizedPositionOfItemPivotToUse = 0f, Func<float, bool> onProgress = null)
		{
			float minContentOffsetFromVPAllowed = _InternalState.viewportSize - _InternalState.contentPanelSize;
			// Positive values indicate CT is smaller than VP, so no scrolling can be done
			if (minContentOffsetFromVPAllowed >= 0f)
			{
				// This is dependent on the case. sometimes is needed, sometimes not
				//if (duration > 0f)
				//    yield return new WaitForSeconds(duration);

				_SmoothScrollCoroutine = null;

				if (onProgress != null)
					onProgress(1f);
				yield break;
			}

			Canvas.ForceUpdateCanvases();
			_Params.scrollRect.StopMovement();
			float initialInsetFromParent = _Params.content.GetInsetFromParentEdge(_Params.viewport as RectTransform, _InternalState.startEdge);
			float targetInsetFromParent = ClampContentStartOffsetFromViewportStart(minContentOffsetFromVPAllowed, itemIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);
			float startTime = Time.time;
			float elapsedTime;
			float progress;
			float value;
			var endOfFrame = new WaitForEndOfFrame();
			bool notCanceled = true;
			do
			{
				yield return null;
				yield return endOfFrame;

				elapsedTime = Time.time - startTime;
				if (elapsedTime >= duration)
					progress = 1f;
				else
					// Normal in; sin slow out
					progress = Mathf.Sin((elapsedTime / duration) * Mathf.PI / 2); ;

				value = Mathf.Lerp(initialInsetFromParent, targetInsetFromParent, progress);

				SetContentStartOffsetFromViewportStart(value);
			}
			while ((onProgress == null || (notCanceled = onProgress(progress))) && progress < 1f);

			// Assures the end result is the expected one
			if (notCanceled)
				ScrollTo(itemIndex, normalizedOffsetFromViewportStart, normalizedPositionOfItemPivotToUse);

			_SmoothScrollCoroutine = null;
			yield break;
		}

		/// <summary><paramref name="offset"/> should be a valid value. See how it's clamped in <see cref="ScrollTo(int, float, float)"/></summary>
		public void SetContentStartOffsetFromViewportStart(float offset) { SetContentEdgeOffsetFromViewportEdge(_InternalState.startEdge, offset); }

		/// <summary><paramref name="offset"/> should be a valid value. See how it's clamped in <see cref="ScrollTo(int, float, float)"/></summary>
		public void SetContentEndOffsetFromViewportEnd(float offset) { SetContentEdgeOffsetFromViewportEdge(_InternalState.endEdge, offset); }
		#endregion

		/// <summary>
		/// <para>Will call GetItem[Height|Width](int) for each other item to have an updated sizes cache</para>
		/// <para>After, will change the size of the item's RectTransform to <paramref name="requestedSize"/> and will shift down/right the next ones, if any</para>
		/// </summary>
		/// <param name="withViewHolder">the view holder. A common usage for an "expand on click" behavior is to have a button on a view whose onClick fires a method in the adapter where it retrieves the view holder via <see cref="GetItemViewsHolderIfVisible(RectTransform)"/> </param>
		/// <param name="requestedSize">the height or width (depending on scrollview's orientation)</param>
		/// <param name="itemEndEdgeStationary">if to grow to the top/left (less common) instead of down/right (more common)</param>
		/// <returns>the resolved size. This can be slightly different than <paramref name="requestedSize"/> if the number of items is huge (>100k))</returns>
		public float RequestChangeItemSizeAndUpdateLayout(TItemViewsHolder withViewHolder, float requestedSize, bool itemEndEdgeStationary = false)
		{
			_Params.scrollRect.StopMovement(); // we don't want a ComputeVisibility() during changing an item's size, so we cut off any inertia 

			int v1_h0 = _Params.scrollRect.vertical ? 1 : 0;
			int vMinus1_h1 = -v1_h0 * 2 + 1;

			float oldSize = _InternalState.itemsSizes[withViewHolder.itemIndexInView];
			float resolvedSize = _InternalState.ChangeItemSizeAndUpdateContentSizeAccordingly(withViewHolder, requestedSize, itemEndEdgeStationary);
			float sizeChange = resolvedSize - oldSize;

			// Move all the next visible elements down / to the right
			// iterating from resized+1 to the last in _VisibleItems;
			TItemViewsHolder curVH;
			int i = 0;
			int indexOfResized = _VisibleItems.IndexOf(withViewHolder);
			Vector3 pos;
			for (i = indexOfResized + 1; i < _VisibleItemsCount; ++i)
			{
				curVH = _VisibleItems[i];
				//pos = curVH.root.InverseTransformPoint(curVH.root.position);
				pos = curVH.root.localPosition;
				pos[v1_h0] += vMinus1_h1 * sizeChange;
				//curVH.root.position = curVH.root.TransformPoint(pos);
				curVH.root.localPosition = pos;
			}

			return resolvedSize;
		}

		/// <summary>
		/// <para>returns the distance of the item's left (if scroll view is Horizontal) or top (if scroll view is Vertical) edge </para>
		/// <para>from the parent's left (respectively, top) edge</para>
		/// </summary>
		public float GetItemOffsetFromParentStart(int itemIndex)
		{ return _InternalState.GetItemOffsetFromParentStartUsingItemIndexInView(_InternalState.GetItemViewIndexFromRealIndex(itemIndex)); }

		/// <summary>
		/// <para>This is called automatically when screen size (or the orientation) changes</para>
		/// <para>But if you somehow resize the scrollview manually, you also must call this</para>
		/// </summary>
		public virtual void NotifyScrollViewSizeChanged()
		{
			_InternalState.layoutRebuildPendingDueToScreenSizeChangeEvent = true;
			ChangeItemCountInternal(_InternalState.itemsCount, false);
		}

		/// <summary>Call this when the adapter is no longer needed</summary>
		public virtual void Dispose()
		{
			if (_Params != null && _Params.scrollRect)
				_Params.scrollRect.onValueChanged.RemoveListener(OnScrollPositionChanged);

			if (_SmoothScrollCoroutine != null)
			{
				_SmoothScrollCoroutine = null;
			}

			if (_MonoBehaviourHelper)
			{
				_MonoBehaviourHelper.Dispose();
				_MonoBehaviourHelper = null;
			}

			ClearCachedRecyclableItems();

			ClearVisibleItems();
			_VisibleItems = null;

			_Params = null;
			_InternalState = null;

			if (ItemsRefreshed != null)
				ItemsRefreshed = null;
		}


		/// <summary> Only called for vertical ScrollRects </summary>
		/// <param name="index">the element's index in your dataset</param>
		/// <returns>The height to be allocated for its visual representation in the view</returns>
		protected abstract float GetItemHeight(int index);

		/// <summary> Only called for horizontal ScrollRects </summary>
		/// <param name="index">the element's index in your dataset</param>
		/// <returns>The width to be allocated for its visual representation in the view</returns>
		protected abstract float GetItemWidth(int index);

		/// <summary> 
		/// <para>Called when there are no recyclable views for itemIndex. Provide a new viewholder instance for itemIndex. This is the place where you must initialize the viewholder </para>
		/// <para>via TItemViewsHolder.Init(..) shortcut or manually set its itemIndex, instantiate the prefab and call CollectViews()</para>
		/// </summary>
		/// <param name="itemIndex">the index of the model that'll be presented by this view holder</param>
		protected abstract TItemViewsHolder CreateViewsHolder(int itemIndex);

		/// <summary>
		/// <para>Here the data in your model should be bound to the views. Use newOrRecycled.itemIndex to retrieve its associated model</para>
		/// <para>Note that view holders are re-used (this is the whole purpose of this adapter), so a view holder's views will contain data from its previously associated model and if, </para>
		/// <para>for example, you're downloading an image to be set as an icon, it makes sense to first clear the previous one (and probably temporarily replace it with a generic "Loading" image)</para>
		/// </summary>
		/// <param name="newOrRecycled"></param>
		protected abstract void UpdateViewsHolder(TItemViewsHolder newOrRecycled);

		/// <summary> Self-explanatory. The default implementation returns true each time</summary>
		/// <param name="potentiallyRecyclable"></param>
		/// <param name="indexOfItemThatWillBecomeVisible"></param>
		/// <param name="heightOfItemThatWillBecomeVisible"></param>
		/// <returns>If the provided view holder is compatible with the item with index <paramref name="indexOfItemThatWillBecomeVisible"/></returns>
		protected virtual bool IsRecyclable(TItemViewsHolder potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
		{ return true; }

		/// <summary>Destroying any remaining game objects in the <see cref="InternalState.recyclableItems"/> list, clearing it and setting <see cref="InternalState.recyclableItemsCount"/> to 0</summary>
		protected virtual void ClearCachedRecyclableItems()
		{
			if (_InternalState != null && _InternalState.recyclableItems != null)
			{
				foreach (var recyclable in _InternalState.recyclableItems)
				{
					if (recyclable != null && recyclable.root != null)
						try { GameObject.Destroy(recyclable.root.gameObject); } catch (Exception e) { Debug.LogException(e); }
				}
				_InternalState.recyclableItems.Clear();
				_InternalState.recyclableItemsCount = 0;
			}
		}

		/// <summary>Destroying any remaining game objects in the <see cref="_VisibleItems"/> list, clearing it and setting <see cref="_VisibleItemsCount"/> to 0</summary>
		protected virtual void ClearVisibleItems()
		{
			if (_VisibleItems != null)
			{
				foreach (var item in _VisibleItems)
				{
					if (item != null && item.root != null)
						try { GameObject.Destroy(item.root.gameObject); } catch (Exception e) { Debug.LogException(e); }
				}
				_VisibleItems.Clear();
				_VisibleItemsCount = 0;
			}
		}

		/// <summary>
		/// <para>Called mainly when it's detected that the scroll view's size has changed. Marks everything for a layout rebuild and then calls Canvas.ForceUpdateCanvases(). </para>
		/// <para>Make sure to override <see cref="AbstractViewHolder.MarkForRebuild"/> in your view holder implementation if you have child layout groups and call LayoutRebuilder.MarkForRebuild() on them</para> 
		/// </summary>
		protected virtual void RebuildLayoutDueToScrollViewSizeChange()
		{
			MarkViewHoldersForRebuild(_VisibleItems);
			MarkViewHoldersForRebuild(_InternalState.recyclableItems);

			Canvas.ForceUpdateCanvases();

			_InternalState.CacheScrollViewInfo(); // update vp size etc.
			_InternalState.maxVisibleItemsSeenSinceLastScrollViewSizeChange = 0;
		}

		///// <summary>
		///// Only to be called inside <see cref="UpdateViewsHolder(TItemViewsHolder)"/>
		///// </summary>
		///// <param name="withViewHolder"></param>
		///// <param name="newSize"></param>
		///// <param name="itemEndEdgeStationary"></param>
		//protected float NotifyCurrentViewHolderChangedSizeDuringUpdate(TItemViewsHolder withViewHolder, float newSize, bool itemEndEdgeStationary = false)
		//{ return _InternalState.ChangeItemSizeAndUpdateContentSizeAccordingly(withViewHolder, newSize, itemEndEdgeStationary, false); }

		/// <summary> Only called for vertical ScrollRects. Called just before a "Twin" ComputeVisibility will execute. Retrieve the new width from viewHolder.root.rect.width and cache it in your model</summary>
		/// <seealso cref="ScheduleComputeVisibilityTwinPass"/>
		protected virtual void OnItemHeightChangedPreTwinPass(TItemViewsHolder viewHolder) { }

		/// <summary> Only called for horizontal ScrollRects. Called just before a "Twin" ComputeVisibility will execute. Retrieve the new width from viewHolder.root.rect.height and cache it in your model</summary>
		/// <seealso cref="ScheduleComputeVisibilityTwinPass"/>
		protected virtual void OnItemWidthChangedPreTwinPass(TItemViewsHolder viewHolder) { }


		/// <summary>
		/// <para>This can be called in order to schedule a "Twin" ComputeVisibility() call after exactly 1 frame.</para> 
		/// <para>A use case is to enable a ContentSizeFitter on your item, call this, </para> 
		/// <para>then in <see cref="OnItemHeightChangedPreTwinPass(TItemViewsHolder)"/> (or <see cref="OnItemWidthChangedPreTwinPass(TItemViewsHolder)"/> if horizontal ScrollRect)</para> 
		/// <para>retrieve the height (cache it in order to return it in future GetItemHeight()/GetItemWidth() callbacks) and disable the ContentSizeFitter</para> 
		/// </summary>
		protected void ScheduleComputeVisibilityTwinPass()
		{
			if (_Params.updateMode != BaseParams.UpdateMode.MONOBEHAVIOUR_UPDATE)
				throw new UnityException("Twin pass is only possible if updateMode is " + BaseParams.UpdateMode.MONOBEHAVIOUR_UPDATE);

			_InternalState.computeVisibilityTwinPassScheduled = true;
		}

		void MarkViewHoldersForRebuild(List<TItemViewsHolder> vhs)
		{
			if (vhs != null)
				foreach (var v in vhs)
					if (v != null && v.root != null)
						v.MarkForRebuild();
		}

		/// <summary> It assumes that the content is bigger than the viewport </summary>
		float ClampContentStartOffsetFromViewportStart(float minContentOffsetFromVPAllowed, int itemIndex, float normalizedOffsetFromStart, float normalizedPositionOfItemPivotToUse)
		{
			float maxContentOffsetFromVPAllowed = 0f;
			int itemViewIdex = _InternalState.GetItemViewIndexFromRealIndex(itemIndex);
			float itemSize = _InternalState.itemsSizes[itemViewIdex];
			float offsetToAdd = _InternalState.viewportSize * normalizedOffsetFromStart - itemSize * normalizedPositionOfItemPivotToUse;

			return Mathf.Max(
						minContentOffsetFromVPAllowed,
						Math.Min(maxContentOffsetFromVPAllowed, -GetItemOffsetFromParentStart(itemIndex) + offsetToAdd)
					);
		}

		void ChangeItemCountInternal(int itemsCount, bool contentPanelEndEdgeStationary)
		{
			int prevCount = _InternalState.itemsCount;
			_Params.scrollRect.StopMovement();
			_InternalState.OnItemsCountChanged(itemsCount, contentPanelEndEdgeStationary);

			// Re-build the content: mark all currentViews as recyclable
			// _RecyclableItems must be zero;
			if (GetNumExcessObjects() > 0)
			{
				throw new UnityException("ChangeItemCountInternal: GetNumExcessObjects() > 0 when calling ChangeItemCountInternal(); this may be due ComputeVisibility not being finished executing yet");
			}

			_InternalState.recyclableItems.AddRange(_VisibleItems);
			_InternalState.recyclableItemsCount += _VisibleItemsCount;

			// If the itemsCount is 0, then it makes sense to destroy all the views, instead of marking them as recyclable. Maybe the ChangeItemCountTo(0) was called in order to clear the current contents
			if (itemsCount == 0)
				ClearCachedRecyclableItems();

			_VisibleItems.Clear();
			_VisibleItemsCount = 0;

			_InternalState.updateRequestPending = true;
			//_MonoBehaviourHelper.CallDelayedByFrames(
			//                 () =>
			//                 {
			//                     // Only computing the visibility if it didn't already
			//                     if (!_InternalState.onScrollPositionChangedFiredAndVisibilityComputedForCurrentItems)
			//                     {
			//                         _Params.ClampScroll01();

			//                         // On some devices(i.e. some androids) this is not fired on initialization, so we're firing it here
			//                         //OnScrollPositionChanged(_Params.scrollRect.normalizedPosition);
			//                         _InternalState.updateRequestPending = true;
			//                     }
			//                 },
			//                 3
			//             );

			if (ItemsRefreshed != null)
				ItemsRefreshed(prevCount, itemsCount);
		}

		/// <summary>Called by MonobehaviourHelper.Update</summary>
		void MyUpdate()
		{
			if (_InternalState.updateRequestPending)
			{
				// ON_SCROLL is the only case when we don't regularly update and are using only onScroll event for ComputeVisibility
				_InternalState.updateRequestPending = _Params.updateMode != BaseParams.UpdateMode.ON_SCROLL;
				ComputeVisibilityForCurrentPosition();
			}
			else if (_InternalState.computeVisibilityTwinPassScheduled)
			{
				ComputeVisibilityForCurrentPosition();
			}
		}

		/// <summary>Called by ScrollRect.onValueChanged event</summary>
		void OnScrollPositionChanged(Vector2 pos)
		{
			if (_Params.updateMode != BaseParams.UpdateMode.MONOBEHAVIOUR_UPDATE)
				ComputeVisibilityForCurrentPosition();
			if (_Params.updateMode != BaseParams.UpdateMode.ON_SCROLL)
				_InternalState.updateRequestPending = true;

			_InternalState.onScrollPositionChangedFiredAndVisibilityComputedForCurrentItems = true;
		}

		/// <summary>Called by <see cref="MyUpdate"/>, <see cref="OnScrollPositionChanged(Vector2)"/> or both</summary>
		void ComputeVisibilityForCurrentPosition()
		{
			float curPos = _Params.GetAbstractNormalizedScrollPosition();
			float delta = curPos - _InternalState.lastProcessedAbstractNormalizedScrollPosition;

			bool wasTwinPass = false;
			if (_InternalState.layoutRebuildPendingDueToScreenSizeChangeEvent)
			{
				RebuildLayoutDueToScrollViewSizeChange();
				_InternalState.layoutRebuildPendingDueToScreenSizeChangeEvent = false;
			}
			else
			{
				if (_InternalState.computeVisibilityTwinPassScheduled)
				{
					// This assures a smooth content resizing (when an edge other than the one where the pivot is is stationary, the content shifts back 
					// due to finger/click being still pressed - same issue as with the looping behavior)
					ForceSetPointerEventDistanceToZero(false);

					// 2 fors are more efficient
					if (_Params.scrollRect.horizontal)
					{
						for (int i = 0; i < _VisibleItemsCount; ++i)
							OnItemWidthChangedPreTwinPass(_VisibleItems[i]);
					}
					else
					{
						for (int i = 0; i < _VisibleItemsCount; ++i)
							OnItemHeightChangedPreTwinPass(_VisibleItems[i]);
					}
					_InternalState.OnItemsSizesChangedExternally(_VisibleItems, delta > 0);
					wasTwinPass = true;
				}
			}
			_InternalState.computeVisibilityTwinPassScheduled = false;

			if (_Params.loopItems)
				LoopIfNeeded(delta, curPos);

			ComputeVisibility(delta);
			// Important: do not remove this commented line: it's a remainder that the correct way of updating this var is to call 
			// again GetAbstractNormalizedScrollPosition(), since it can change due to LoopIfNeeded() being called.
			// Only using curPos if looping is disabled
			//_InternalParams.lastProcessedAbstractNormalizedScrollPosition = curPos;

			// If it's a twin pass, we should keep the delta, as this re-pass should behave as closely as possible to the previous one
			if (!wasTwinPass)
				_InternalState.lastProcessedAbstractNormalizedScrollPosition = _Params.loopItems ? _Params.GetAbstractNormalizedScrollPosition() : curPos;
		}

		void ForceSetPointerEventDistanceToZero(bool throwExceptionIfImpossible)
		{
			if (throwExceptionIfImpossible)
			{
				if (!(EventSystem.current.currentInputModule is PointerInputModule))
					throw new InvalidOperationException("Cannot use looping with if the current input module does not inherit from PointerInputModule");
			}

			// Dig into reflection and get the original pointer data
			var eventSystemAsPointerInputModule = (EventSystem.current.currentInputModule as PointerInputModule);
			var pointerEvents = eventSystemAsPointerInputModule
				.GetType()
				.GetField("m_PointerData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.GetValue(eventSystemAsPointerInputModule)
				as Dictionary<int, PointerEventData>;

			// Modify the original pointer to look like it was pressed at the current position and it didn't move
			//PointerEventData originalPED = null;
			foreach (var pointer in pointerEvents.Values)
			{
				if (pointer.pointerDrag == _Params.scrollRect.gameObject)
				{
					pointer.pointerPressRaycast = pointer.pointerCurrentRaycast;
					pointer.pressPosition = pointer.position;
					pointer.delta = Vector2.zero;
					pointer.dragging = false;
					pointer.scrollDelta = Vector2.zero;
					// TODO test
					//pointer.Use();
					//originalPED = pointer;
					break;
				}
			}
		}

		/// <summary>Looping behavior called in <see cref="ComputeVisibilityForCurrentPosition"/> when <see cref="BaseParams.loopItems"/> is true</summary>
		void LoopIfNeeded(float delta, float curPos)
		{
			//if (curPos < 0f || curPos > 1f)
			//{
			//    //Debug.Log(curPos);
			//    return;
			//}

			if (_VisibleItems.Count > 0)
			{
				int resetDir = 0;
				if (delta > 0f && curPos > .95f) // scrolling to start (1f) and curPos is somewhat near start
												 // crossed half of the content in the positive dir (up/left) => reset it and re-map heights, cumulatedHeights; re-order items
					resetDir = 2;
				else if (delta < 0f && curPos < .05f) // scrolling to end (0f) and curPos is somewhat near end
													  // crossed half of the content in the negative dir (down/right)  => reset it and re-map heights, cumulatedHeights; re-order items
					resetDir = 1;
				else
					return;

				var velocityBeforeLoop = _Params.scrollRect.velocity;
				int firstVisibleItem_IndexInView = _VisibleItems[0].itemIndexInView,
					lastVisibleItem_IndexInView = firstVisibleItem_IndexInView + _VisibleItemsCount - 1;

				ForceSetPointerEventDistanceToZero(true);

				RectTransform.Edge edgeToInsetContentFrom;
				float contentNewInsetFromParentEdge;
				if (resetDir == 1)
				{
					// Already done (this can sometimes mean there are too few items in list)
					if (firstVisibleItem_IndexInView == 0)
						return;

					float firstVisibleItemInsetFromStart = _InternalState.GetItemOffsetFromParentStartUsingItemIndexInView(firstVisibleItem_IndexInView);
					float contentInsetFromVPStart = _Params.content.GetInsetFromParentEdge(_Params.viewport, _InternalState.startEdge);
					float firstVisibleItemAmountOutside = -contentInsetFromVPStart - firstVisibleItemInsetFromStart;
					float contentNewInsetFromParentStart = -(firstVisibleItemAmountOutside + _InternalState.paddingContentStart);
					contentNewInsetFromParentEdge = contentNewInsetFromParentStart;
					edgeToInsetContentFrom = _InternalState.startEdge;
				}
				else
				{
					// Already done (this can sometimes mean there are too few items in list)
					if (lastVisibleItem_IndexInView + 1 >= _InternalState.itemsCount)
						return;

					float lastVisibleItemInsetFromStart = _InternalState.GetItemOffsetFromParentStartUsingItemIndexInView(lastVisibleItem_IndexInView);
					float lastVisibleItemSize = _InternalState.itemsSizes[lastVisibleItem_IndexInView];
					float lastVisibleItemInsetFromEnd = _InternalState.contentPanelSize - (lastVisibleItemInsetFromStart + lastVisibleItemSize);
					float contentInsetFromVPEnd = _Params.content.GetInsetFromParentEdge(_Params.viewport, _InternalState.endEdge);
					float lastVisibleItemAmountOutside = -contentInsetFromVPEnd - lastVisibleItemInsetFromEnd;
					float contentNewInsetFromParentEnd = -(lastVisibleItemAmountOutside + _InternalState.paddingContentEnd);
					contentNewInsetFromParentEdge = contentNewInsetFromParentEnd;
					edgeToInsetContentFrom = _InternalState.endEdge;
				}
				_Params.content.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_Params.viewport, edgeToInsetContentFrom, contentNewInsetFromParentEdge, _InternalState.contentPanelSize);
				_Params.scrollRect.Rebuild(CanvasUpdate.PostLayout);
				_Params.scrollRect.velocity = velocityBeforeLoop;

				int newRealIndexOfFirstItemInView;
				if (resetDir == 1)
				{
					newRealIndexOfFirstItemInView = _VisibleItems[0].itemIndex;

					// Adjust the itemIndexInView for the visible items. they'll be the last ones, so the last one of them will have, for example, viewIndex = itemsCount-1
					for (int i = 0; i < _VisibleItemsCount; ++i)
						_VisibleItems[i].itemIndexInView = i;
				}
				else
				{
					// The next item after this will become the first one in view
					newRealIndexOfFirstItemInView = _InternalState.GetItemRealIndexFromViewIndex(lastVisibleItem_IndexInView + 1);

					// Adjust the itemIndexInView for the visible items. they'll be the last ones, so the last one of them will have, for example, viewIndex = itemsCount-1
					for (int i = 0; i < _VisibleItemsCount; ++i)
						_VisibleItems[i].itemIndexInView = _InternalState.itemsCount - _VisibleItemsCount + i;
				}
				_InternalState.OnScrollViewLooped(newRealIndexOfFirstItemInView);

				// Update the positions of the visible items so they'll retain their position relative to the viewport
				TItemViewsHolder vh;
				for (int i = 0; i < _VisibleItemsCount; ++i)
				{
					vh = _VisibleItems[i];
					float insetFromStart = _InternalState.paddingContentStart + vh.itemIndexInView * _InternalState.spacing;
					if (vh.itemIndexInView > 0)
						insetFromStart += _InternalState.itemsSizesCumulative[vh.itemIndexInView - 1];

					vh.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_Params.content, _InternalState.startEdge, insetFromStart, _InternalState.itemsSizes[vh.itemIndexInView]);
				}

				_InternalState.UpdateLastProcessedNormalizedScrollPosition();
			}
		}

		/// <summary>The very core of <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}"/>. You must be really brave if you think about trying to understand it :)</summary>
		void ComputeVisibility(float abstractDelta)
		{
			// ALIASES:
			// scroll down = the content goes down(the "view point" goes up); scroll up = analogue
			// the notation "x/y" means "x, if vertical scroll; y, if horizontal scroll"
			// positive scroll = down/right; negative scroll = up/left
			// [start] = usually refers to some point above/to-left-of [end], if negativeScroll; 
			//          else, [start] is below/to-right-of [end]; 
			//          for example: -in context of _VisibleItems, [start] is 0 for negativeScroll and <_VisibleItemsCount-1> for positiveScroll;
			//                       -in context of an item, [start] is its top for negativeScroll and bottom for positiveScroll;
			//                       - BUT for ct and vp, they have fixed meaning, regardless of the scroll sign. they only depend on scroll direction (if vert, start = top, end = bottom; if hor, start = left, end = right)
			// [end] = inferred from definition of [start]
			// LV = last visible (the last item that was closest to the negVPEnd_posVPStart in the prev call of this func - if applicable)
			// NLV = new last visible (the next one closer to the negVPEnd_posVPStart than LV)
			// neg = negative scroll (down or right)
			// pos =positive scroll (up or left)
			// ch = child (i.e. ctChS = content child start(first child) (= ct.top - ctPaddingTop, in case of vertical scroll))

			#region visualization
			// So, again, this is the items' start/end notions! Viewport's and Content's start/end are constant throughout the session
			// Assume the following scroll direction (hor) and sign (neg) (where the VIEWPORT+SCROLLBAR goes, opposed to where the CONTENT goes):
			// hor, negative:
			// O---------------->
			//      [vpStart]  [start]item[end] .. [start]item2[end] .. [start]LVItem[end] [vpEnd]
			// hor, positive:
			// <----------------O
			//      [vpStart]  [end]item[start] .. [end]item2[start] .. [end]LVItem[start] [vpEnd]
			#endregion

			bool negativeScroll = abstractDelta <= 0f;
			//bool verticalScroll = _Params.scrollRect.vertical;

			// Viewport constant values
			float vpSize = _InternalState.viewportSize;

			// Content panel constant values
			float ctSpacing = _InternalState.spacing,
				  ctPadTransvStart = _InternalState.transversalPaddingContentStart;

			// Items constant values
			float allItemsTransversalSizes = _InternalState.itemsConstantTransversalSize;

			// Items variable values
			TItemViewsHolder nlvHolder = null;
			//int currentLVItemIndex;
			int currentLVItemIndexInView;

			float negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV;
			RectTransform.Edge negStartEdge_posEndEdge;
			RectTransform.Edge transvStartEdge = _InternalState.transvStartEdge;

			//int negEndItemIndex_posStartItemIndex,
			//int endItemIndex, // TODO pending removal
			int endItemIndexInView,
				  neg1_posMinus1,
				  //negMinus1_pos1,
				  neg1_pos0,
				  neg0_pos1;

			//float neg0_posVPSize;

			if (negativeScroll)
			{
				neg1_posMinus1 = 1;
				negStartEdge_posEndEdge = _InternalState.startEdge;
			}
			else
			{
				neg1_posMinus1 = -1;
				negStartEdge_posEndEdge = _InternalState.endEdge;
			}
			//negMinus1_pos1 = -neg1_posMinus1;
			neg1_pos0 = (neg1_posMinus1 + 1) / 2;
			neg0_pos1 = 1 - neg1_pos0;
			//neg0_posVPSize = neg0_pos1 * vpSize;

			// -1, if negativeScroll
			// _InternalParams.itemsCount, else
			currentLVItemIndexInView = neg0_pos1 * (_InternalState.itemsCount - 1) - neg1_posMinus1;

			// _InternalParams.itemsCount - 1, if negativeScroll
			// 0, else
			endItemIndexInView = neg1_pos0 * (_InternalState.itemsCount - 1);

			float negCTInsetFromVPS_posCTInsetFromVPE = _Params.content.GetInsetFromParentEdge(_Params.viewport, negStartEdge_posEndEdge);

			// _VisibleItemsCount is always 0 in the first call of this func after the list is modified 
			if (_VisibleItemsCount > 0)
			{
				// 
				// startingLV means the item in _VisibleItems that's the closest to the next one that'll spawn
				// 



				int startingLVHolderIndex;
				// The item that was the last in the _VisibleItems; We're inferring the positions of the other ones after(below/to the right, depending on hor/vert scroll) it this way, since the heights(widths for hor scroll) are known
				TItemViewsHolder startingLVHolder;
				//RectTransform startingLVRT;

				// startingLVHolderIndex will be:
				// _VisibleITemsCount - 1, if negativeScroll
				// 0, if upScroll
				startingLVHolderIndex = neg1_pos0 * (_VisibleItemsCount - 1);
				startingLVHolder = _VisibleItems[startingLVHolderIndex];
				//startingLVRT = startingLVHolder.root;

				// Approach name(will be referenced below): (%%%)
				// currentStartToUseForNLV will be:
				// NLV top (= LV bottom - spacing), if negativeScroll
				// NLV bottom (= LV top + spacing), else
				//---
				// More in depth: <down0up1 - startingLVRT.pivot.y> will be
				// -startingLVRT.pivot.y, if negativeScroll
				// 1 - startingLVRT.pivot.y, else
				//---
				// And: 
				// ctSpacing will be subtracted from the value, if negativeScroll
				// added, if upScroll

				// Commented: using a more efficient way of doing this by using cumulative sizes
				//if (verticalScroll)
				//{
				//    float sizePlusSpacing = startingLVRT.rect.height + ctSpacing;
				//    if (negativeScroll)
				//        negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = startingLVRT.GetInsetFromParentTopEdge(_Params.content) + sizePlusSpacing;
				//    else
				//        negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = startingLVRT.GetInsetFromParentBottomEdge(_Params.content) + sizePlusSpacing;
				//}
				//else
				//{
				//    float sizePlusSpacing = startingLVRT.rect.width + ctSpacing;
				//    if (negativeScroll)
				//        negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = startingLVRT.GetInsetFromParentLeftEdge(_Params.content) + sizePlusSpacing;
				//    else
				//        negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = startingLVRT.GetInsetFromParentRightEdge(_Params.content) + sizePlusSpacing;
				//}
				//float sizePlusSpacing = _InternalParams.itemsSizes[startingLVHolder.itemIndex] + ctSpacing;
				//if (negativeScroll)
				//    negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = _InternalParams.GetItemOffsetFromParentStart(startingLVHolder.itemIndex) + sizePlusSpacing;
				//else
				//    negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = _InternalParams.contentPanelSize - _InternalParams.GetItemOffsetFromParentStart(startingLVHolder.itemIndex);

				// Items variable values; initializing them to the current LV
				currentLVItemIndexInView = startingLVHolder.itemIndexInView;

				// Get a list of items that are above(if neg)/below(if pos) viewport and move them from 
				// _VisibleItems to itemsOutsideViewport; they'll be candidates for recycling
				TItemViewsHolder curRecCandidateVH;
				bool currentIsOutside;
				//RectTransform curRecCandidateRT;
				float curRecCandidateSizePlusSpacing;
				float insetFromParentEdge;
				while (true)
				{
					// vItemHolder is:
					// first in _VisibleItems, if negativeScroll
					// last in _VisibleItems, else
					int curRecCandidateVHIndex = neg0_pos1 * (_VisibleItemsCount - 1);
					curRecCandidateVH = _VisibleItems[curRecCandidateVHIndex];
					//curRecCandidateRT = curRecCandidateVH.root;
					//float lvSize = _InternalParams.itemsSizes[currentLVItemIndex];
					curRecCandidateSizePlusSpacing = _InternalState.itemsSizes[curRecCandidateVH.itemIndexInView] + ctSpacing; // major bugfix: 18.12.2016 1:20: must use vItemHolder.itemIndex INSTEAD of currentLVItemIndex

					// Commented: using a more efficient way of doing this by using cumulative sizes, even if we need to use an "if"
					//currentIsOutside = negCTInsetFromVPS_posCTInsetFromVPE + (curRecCandidateRT.GetInsetFromParentEdge(_Params.content, negStartEdge_posEndEdge) + curRecCandidateSizePlusSpacing) <= 0f;
					if (negativeScroll)
						insetFromParentEdge = _InternalState.GetItemOffsetFromParentStartUsingItemIndexInView(curRecCandidateVH.itemIndexInView);
					else
						insetFromParentEdge = _InternalState.GetItemOffsetFromParentEndUsingItemIndexInView(curRecCandidateVH.itemIndexInView);
					currentIsOutside = negCTInsetFromVPS_posCTInsetFromVPE + (insetFromParentEdge + curRecCandidateSizePlusSpacing) <= 0f;

					if (currentIsOutside)
					{
						_InternalState.recyclableItems.Add(curRecCandidateVH);
						_VisibleItems.RemoveAt(curRecCandidateVHIndex);
						--_VisibleItemsCount;
						++_InternalState.recyclableItemsCount;

						if (_VisibleItemsCount == 0) // all items that were considered visible are now outside viewport => will need to seek even more below 
							break;
					}
					else
						break; // the current item is outside(not necessarily completely) the viewport
				}
			}
			//else
			//    negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = neg1_pos0 * _InternalParams.paddingContentStart + neg0_pos1 * _InternalParams.paddingContentEnd;

			do
			{
				// negativeScroll vert/hor: there are no items below/to-the-right-of-the current LV that might need to be made visible
				// positive vert/hor: there are no items above/o-the-left-of-the current LV that might need to be made visible
				if (currentLVItemIndexInView == endItemIndexInView)
					break;

				// Searching for next item that might get visible: downwards on negativeScroll OR upwards else
				//int nlvIndex = currentLVItemIndexInView; // TODO pending removal
				int nlvIndexInView = currentLVItemIndexInView;
				float nlvSize;
				bool breakBigLoop = false,
					 negNLVCandidateBeforeVP_posNLVCandidateAfterVP; // before vpStart, if negative scroll; after vpEnd, else
				do
				{
					nlvIndexInView += neg1_posMinus1;
					if (negativeScroll)
						negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = _InternalState.GetItemOffsetFromParentStartUsingItemIndexInView(nlvIndexInView);
					else
						negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = _InternalState.GetItemOffsetFromParentEndUsingItemIndexInView(nlvIndexInView);
					nlvSize = _InternalState.itemsSizes[nlvIndexInView];
					negNLVCandidateBeforeVP_posNLVCandidateAfterVP = negCTInsetFromVPS_posCTInsetFromVPE + (negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV + nlvSize) <= 0f;
					if (negNLVCandidateBeforeVP_posNLVCandidateAfterVP)
					{
						if (nlvIndexInView == endItemIndexInView) // all items are outside viewport => abort
						{
							breakBigLoop = true;
							break;
						}
					}
					else
					{
						// Next item is after vp(if neg) or before vp (if pos) => no more items will become visible 
						// (this happens usually in the first iteration of this loop inner loop, i.e. negNLVCandidateBeforeVP_posNLVCandidateAfterVP never being true)
						if (negCTInsetFromVPS_posCTInsetFromVPE + negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV > vpSize)
						{
							breakBigLoop = true;
							break;
						}

						// At this point, we've found the real nlv: nlvIndex, nlvH and currentTopToUseForNLV(if negativeScroll)/currentBottomToUseForNLV(if upScroll) were correctly assigned
						break;
					}
				}
				while (true);

				if (breakBigLoop)
					break;

				int nlvRealIndex = _InternalState.GetItemRealIndexFromViewIndex(nlvIndexInView);

				// Search for a recyclable holder for current NLV
				// This block remains the same regardless of <negativeScroll> variable, because the items in <itemsOutsideViewport> were already added in an order dependent on <negativeScroll>
				// (they are always from <closest to [start]> to <closest to [end]>)
				int i = 0;
				TItemViewsHolder potentiallyRecyclable;
				while (true)
				{
					if (i < _InternalState.recyclableItemsCount)
					{
						potentiallyRecyclable = _InternalState.recyclableItems[i];
						if (IsRecyclable(potentiallyRecyclable, nlvRealIndex, nlvSize))
						{
							_InternalState.recyclableItems.RemoveAt(i);
							--_InternalState.recyclableItemsCount;
							nlvHolder = potentiallyRecyclable;
							break;
						}
						++i;
					}
					else
					{
						// Found no recyclable view with the requested height
						nlvHolder = CreateViewsHolder(nlvRealIndex);
						break;
					}
				}

				// Add it in list at [end]
				_VisibleItems.Insert(neg1_pos0 * _VisibleItemsCount, nlvHolder);
				++_VisibleItemsCount;

				// Update its index
				nlvHolder.itemIndex = nlvRealIndex;
				nlvHolder.itemIndexInView = nlvIndexInView;

				// Cache its height
				nlvHolder.cachedSize = _InternalState.itemsSizes[nlvIndexInView];

				// Make sure it's parented to content panel
				RectTransform nlvRT = nlvHolder.root;
				nlvRT.SetParent(_Params.content, false);

				// Update its views
				UpdateViewsHolder(nlvHolder);

				// Make sure it's GO is activated
				nlvHolder.root.gameObject.SetActive(true);

				// Make sure it's left-top anchored (the need for this arose together with the feature for changind an item's size 
				// (an thus, the content's size) externally, using RequestChangeItemSizeAndUpdateLayout)
				nlvRT.anchorMin = nlvRT.anchorMax = _InternalState.constantAnchorPosForAllItems;

				//// Though visually not relevant, maybe it helps the UI system 
				//if (negativeScroll) nlvRT.SetAsLastSibling();
				//else nlvRT.SetAsFirstSibling();

				nlvRT.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_Params.content, negStartEdge_posEndEdge, negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV, nlvSize);

				// Commented: using cumulative sizes
				//negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV += nlvSizePlusSpacing;


				//float itemSizesUntilNowPlusSpacing = _InternalParams.itemsSizesCumulative[nlvIndex] + nlvIndex * ctSpacing;
				//if (negativeScroll)
				//    negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = _InternalParams.paddingContentStart + itemSizesUntilNowPlusSpacing + ctSpacing;
				//else
				//    negCurrentInsetFromCTSToUseForNLV_posCurrentInsetFromCTEToUseForNLV = _InternalParams.paddingContentStart + itemSizesUntilNowPlusSpacing + ctSpacing;

				// Assure transversal size and transversal position based on parent's padding and width settings
				nlvRT.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_Params.content, transvStartEdge, ctPadTransvStart, allItemsTransversalSizes);

				currentLVItemIndexInView = nlvIndexInView;
			}
			while (true);

			// Keep track of the <maximum number of items that were visible since last scroll view size change>, so we can optimize the object pooling process
			// by destroying objects in recycle bin only if the aforementioned number is  less than <numVisibleItems + numItemsInRecycleBin>,
			// and of course, making sure at least 1 item is in the bin all the time
			if (_VisibleItemsCount > _InternalState.maxVisibleItemsSeenSinceLastScrollViewSizeChange)
				_InternalState.maxVisibleItemsSeenSinceLastScrollViewSizeChange = _VisibleItemsCount;

			// Disable all recyclable views
			// Destroy remaining unused views, BUT keep one, so there's always a reserve, instead of creating/destroying very frequently
			// + keep <numVisibleItems + numItemsInRecycleBin> abvove <_InternalParams.maxVisibleItemsSeenSinceLastScrollViewSizeChange>
			// See GetNumExcessObjects()
			GameObject go;
			for (int i = 0; i < _InternalState.recyclableItemsCount;)
			{
				go = _InternalState.recyclableItems[i].root.gameObject;
				go.SetActive(false);
				if (GetNumExcessObjects() > 0)
				{
					GameObject.Destroy(go);
					_InternalState.recyclableItems.RemoveAt(i);
					--_InternalState.recyclableItemsCount;
				}
				else
					++i;
			}
		}

		int GetNumExcessObjects()
		{
			if (_InternalState.recyclableItemsCount > 1)
			{
				int excess = (_InternalState.recyclableItemsCount + _VisibleItemsCount) - GetMinNumObjectsToKeepInMemory();
				if (excess > 0)
					return excess;
			}

			return 0;
		}

		int GetMinNumObjectsToKeepInMemory()
		{ return _Params.minNumberOfObjectsToKeepInMemory > 0 ? _Params.minNumberOfObjectsToKeepInMemory : _InternalState.maxVisibleItemsSeenSinceLastScrollViewSizeChange + 1; }

		void SetContentEdgeOffsetFromViewportEdge(RectTransform.Edge contentAndViewportEdge, float offset)
		{
			Canvas.ForceUpdateCanvases();
			_Params.scrollRect.StopMovement();
			_Params.content.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_Params.viewport, contentAndViewportEdge, offset, _InternalState.contentPanelSize);
			_InternalState.CacheScrollViewInfo(); // the content size might slignly change due to eventual rounding errors, so we cache the info again
												  // TODO see if a ChangeItemsCountTo is necessary for a refresh
		}

		void OnScreenSizeChangedEventDispatcher.IOnScreenSizeChangedListener.OnScreenSizeChanged(float lastWidth, float lastHeight, float width, float height)
		{
			NotifyScrollViewSizeChanged();
		}



		/// <summary>Contain cached variables, helper methods and generally things that are not exposed to inheritors. Note: the LayoutGroup component on content, if any, will be disabled</summary>
		class InternalState
		{
			/// Fields are in format: value if vertical scrolling/value if horizontal scrolling
			// Constant params 
			internal readonly Vector2 constantAnchorPosForAllItems = new Vector2(0f, 1f); // top-left
			internal float viewportSize;
			internal float paddingContentStart; // top/left
			internal float transversalPaddingContentStart; // left/top
			internal float paddingContentEnd; // bottom/right
			internal float paddingStartPlusEnd;
			internal float spacing;
			internal RectTransform.Edge startEdge; // RectTransform.Edge.Top/RectTransform.Edge.Left
			internal RectTransform.Edge endEdge; // RectTransform.Edge.Bottom/RectTransform.Edge.Right
			internal RectTransform.Edge transvStartEdge; // RectTransform.Edge.Left/RectTransform.Edge.Top
			internal float itemsConstantTransversalSize; // widths/heights

			// Cache params
			internal int itemsCount;
			internal float lastProcessedAbstractNormalizedScrollPosition; // normY / 1-normX
			internal int realIndexOfFirstItemInView;

			internal List<TItemViewsHolder> recyclableItems = new List<TItemViewsHolder>();
			internal int recyclableItemsCount = 0;
			internal float[] itemsSizes; // heights/widths
			internal float[] itemsSizesCumulative; // heights/widths
			internal float cumulatedSizesOfAllItemsPlusSpacing;
			internal float contentPanelSize; // height/width
			internal bool onScrollPositionChangedFiredAndVisibilityComputedForCurrentItems;
			internal bool updateRequestPending;
			internal int maxVisibleItemsSeenSinceLastScrollViewSizeChange = 0; // heuristic used to prevent destroying too much objects; reset back to 0 when the NotifyScrollViewSizeChanged is called
			internal bool layoutRebuildPendingDueToScreenSizeChangeEvent;
			internal bool computeVisibilityTwinPassScheduled;

			TParams _SourceParams;
			Func<int, float> _GetItemSizeFunc;

			InternalState(TParams sourceParams, ScrollRectItemsAdapter8<TParams, TItemViewsHolder> adapter)
			{
				_SourceParams = sourceParams;

				var lg = sourceParams.content.GetComponent<HorizontalOrVerticalLayoutGroup>();
				if (lg && lg.enabled)
				{
					lg.enabled = false;
					Debug.Log("LayoutGroup on GameObject " + lg.name + " has beed disabled in order to use ScrollRectItemsAdapter8");
				}

				var contentSizeFitter = sourceParams.content.GetComponent<ContentSizeFitter>();
				if (contentSizeFitter && contentSizeFitter.enabled)
				{
					contentSizeFitter.enabled = false;
					Debug.Log("ContentSizeFitter on GameObject " + contentSizeFitter.name + " has beed disabled in order to use ScrollRectItemsAdapter8");
				}

				var layoutElement = sourceParams.content.GetComponent<LayoutElement>();
				if (layoutElement)
				{
					GameObject.Destroy(layoutElement);
					Debug.Log("LayoutElement on GameObject " + contentSizeFitter.name + " has beed DESTROYED in order to use ScrollRectItemsAdapter8");
				}

				if (sourceParams.scrollRect.horizontal)
				{
					startEdge = RectTransform.Edge.Left;
					endEdge = RectTransform.Edge.Right;
					transvStartEdge = RectTransform.Edge.Top;

					// Need to create a lambda, not store the method directly (which will call the base's abstract method)
					_GetItemSizeFunc = i => adapter.GetItemWidth(i);
				}
				else
				{
					startEdge = RectTransform.Edge.Top;
					endEdge = RectTransform.Edge.Bottom;
					transvStartEdge = RectTransform.Edge.Left;

					// Need to create a lambda, not store the method directly (which will call the base's abstract method)
					_GetItemSizeFunc = i => adapter.GetItemHeight(i);
				}
				CacheScrollViewInfo();
			}

			internal static InternalState CreateFromSourceParamsOrThrow(TParams sourceParams, ScrollRectItemsAdapter8<TParams, TItemViewsHolder> adapter)
			{
				if (sourceParams.scrollRect.horizontal && sourceParams.scrollRect.vertical)
				{
					throw new UnityException("Can't optimize a ScrollRect with both horizontal and vertical scrolling modes. Disable one of them");
				}

				return new InternalState(sourceParams, adapter);
			}


			internal void CacheScrollViewInfo()
			{
				RectTransform vpRT = _SourceParams.viewport;
				Rect vpRect = vpRT.rect;

				if (_SourceParams.scrollRect.horizontal)
				{
					viewportSize = vpRect.width;
					paddingContentStart = _SourceParams.contentPadding.left;
					paddingContentEnd = _SourceParams.contentPadding.right;
					transversalPaddingContentStart = _SourceParams.contentPadding.top;
					itemsConstantTransversalSize = _SourceParams.content.rect.height - (transversalPaddingContentStart + _SourceParams.contentPadding.bottom);
				}
				else
				{
					viewportSize = vpRect.height;
					paddingContentStart = _SourceParams.contentPadding.top;
					paddingContentEnd = _SourceParams.contentPadding.bottom;
					transversalPaddingContentStart = _SourceParams.contentPadding.left;
					itemsConstantTransversalSize = _SourceParams.content.rect.width - (transversalPaddingContentStart + _SourceParams.contentPadding.right);
				}
				paddingStartPlusEnd = paddingContentStart + paddingContentEnd;
				spacing = _SourceParams.contentSpacing;
			}


			void AssureItemsSizesArrayCapacity()
			{
				if (itemsSizes == null || itemsSizes.Length != itemsCount)
					itemsSizes = new float[itemsCount];

				if (itemsSizesCumulative == null || itemsSizesCumulative.Length != itemsCount)
					itemsSizesCumulative = new float[itemsCount];
			}

			float CollectSizesOfAllItems()
			{
				AssureItemsSizesArrayCapacity();
				float size, cumulatedSizesOfAllItems = 0f;
				int realIndex;
				for (int viewIndex = 0; viewIndex < itemsCount; ++viewIndex)
				{
					realIndex = GetItemRealIndexFromViewIndex(viewIndex);
					size = _GetItemSizeFunc(realIndex);
					itemsSizes[viewIndex] = size;
					cumulatedSizesOfAllItems += size;
					itemsSizesCumulative[viewIndex] = cumulatedSizesOfAllItems;
				}

				return cumulatedSizesOfAllItems;
			}

			internal void OnItemsCountChanged(int itemsNewCount, bool contentPanelEndEdgeStationary)
			{
				realIndexOfFirstItemInView = itemsNewCount > 0 ? 0 : -1;

				itemsCount = itemsNewCount;
				OnTotalSizeOfAllItemsChanged(CollectSizesOfAllItems(), contentPanelEndEdgeStationary);

				// Schedule a new ComputeVisibility iteration
				onScrollPositionChangedFiredAndVisibilityComputedForCurrentItems = false;

				computeVisibilityTwinPassScheduled = false;
			}

			/// <returns>the resolved size, as this may be a bit different than the passed <paramref name="requestedSize"/> for huge data sets (>100k items)</returns>
			internal float ChangeItemSizeAndUpdateContentSizeAccordingly(TItemViewsHolder viewHolder, float requestedSize, bool itemEndEdgeStationary, bool rebuild = true)
			{
				//LiveDebugger8.logR("ChangeItemCountInternal");
				if (itemsSizes == null)
					throw new UnityException("Wait for initialization first");

				if (viewHolder.root == null)
					throw new UnityException("God bless: shouldn't happen if implemented according to documentation/examples"); // shouldn't happen if implemented according to documentation/examples

				RectTransform.Edge edge;
				float inset;
				if (itemEndEdgeStationary)
				{
					edge = endEdge;
					inset = GetItemOffsetFromParentEndUsingItemIndexInView(viewHolder.itemIndexInView);
				}
				else
				{
					edge = startEdge;
					inset = GetItemOffsetFromParentStartUsingItemIndexInView(viewHolder.itemIndexInView);
				}

				viewHolder.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_SourceParams.content, edge, inset, requestedSize);
				float resolvedSize;
				// Even though we know the desired size, the one actually set by the UI system may be different, so we cache that one
				if (_SourceParams.scrollRect.horizontal)
					resolvedSize = viewHolder.root.rect.width;
				else
					resolvedSize = viewHolder.root.rect.height;
				viewHolder.cachedSize = resolvedSize;

				float size, cumulatedSizesOfAllItems = 0f;
				int realIndex;
				for (int viewIndex = 0; viewIndex < itemsCount; ++viewIndex)
				{
					realIndex = GetItemRealIndexFromViewIndex(viewIndex);
					if (viewIndex == viewHolder.itemIndexInView) // don't request the size of the modified view; use the resolved one instead
						size = resolvedSize;
					else
						size = _GetItemSizeFunc(realIndex);

					itemsSizes[viewIndex] = size;
					cumulatedSizesOfAllItems += size;
					itemsSizesCumulative[viewIndex] = cumulatedSizesOfAllItems;
				}
				OnTotalSizeOfAllItemsChanged(cumulatedSizesOfAllItems, itemEndEdgeStationary, rebuild);

				return resolvedSize;
			}

			Func<int, float> getItemOffsetFunction; // re-usable temp var
			Func<TItemViewsHolder, float> getItemSizeFunction; // re-usable temp var
			internal void OnItemsSizesChangedExternally(List<TItemViewsHolder> viewHolders, bool itemEndEdgeStationary)
			{
				int num = viewHolders.Count;
				int viewIndex;
				TItemViewsHolder vh;
				//int start, endExcl, increment;
				var insetEdge = itemEndEdgeStationary ? endEdge : startEdge;
				float insetFromStationaryEdge;
				float size;

				if (_SourceParams.scrollRect.horizontal)
					getItemSizeFunction = v => v.root.rect.width;
				else
					getItemSizeFunction = v => v.root.rect.height;

				float cumulatedSizesOfAllItemsUntilNow = 0f;
				if (num > 0) // is it really needed?
				{
					int firstIndex = viewHolders[0].itemIndexInView;
					if (firstIndex > 0)
						cumulatedSizesOfAllItemsUntilNow = itemsSizesCumulative[firstIndex - 1];

					for (int i = 0; i < num; ++i)
					{
						vh = viewHolders[i];
						viewIndex = vh.itemIndexInView;
						size = getItemSizeFunction(vh);
						vh.cachedSize = size;

						itemsSizes[viewIndex] = size;
						cumulatedSizesOfAllItemsUntilNow += size;
						itemsSizesCumulative[viewIndex] = cumulatedSizesOfAllItemsUntilNow;
					}

					// Update the remaining cumulative values until end
					for (int i = num; i < itemsCount; ++i)
						itemsSizesCumulative[i] = itemsSizesCumulative[i - 1] + itemsSizes[i];

					if (itemEndEdgeStationary)
						getItemOffsetFunction = GetItemOffsetFromParentEndUsingItemIndexInView;
					else
						getItemOffsetFunction = GetItemOffsetFromParentStartUsingItemIndexInView;


					if (firstIndex > 0)
						cumulatedSizesOfAllItemsUntilNow = itemsSizesCumulative[firstIndex - 1];
					else
						cumulatedSizesOfAllItemsUntilNow = 0f;
					for (int i = 0; i < num; ++i)
					{
						vh = viewHolders[i];
						viewIndex = vh.itemIndexInView;
						insetFromStationaryEdge = getItemOffsetFunction(viewIndex);
						vh.root.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_SourceParams.content, insetEdge, insetFromStationaryEdge, itemsSizes[viewIndex]);

						// Maybe?
						//// Update to resolved size
						//size = getItemSizeFunction(vh);
						//vh.cachedSize = size;

						//itemsSizes[viewIndex] = size;
						//cumulatedSizesOfAllItemsUntilNow += size;
						//itemsSizesCumulative[viewIndex] = cumulatedSizesOfAllItemsUntilNow;
					}
				}

				OnTotalSizeOfAllItemsChanged(itemsCount > 0 ? itemsSizesCumulative[itemsCount - 1] : 0, itemEndEdgeStationary, true);
			}

			internal void UpdateLastProcessedNormalizedScrollPosition()
			{
				lastProcessedAbstractNormalizedScrollPosition = _SourceParams.GetAbstractNormalizedScrollPosition();
			}

			internal void ResetLastProcessedNormalizedScrollPositionToStart()
			{
				// Commented: not needed anymore, as now start=1, end=0, regardless of the scroll type
				//if (_SourceParams.scrollRect.horizontal)
				//    lastProcessedAbstractNormalizedScrollPosition = 0f;
				//else
				//lastProcessedAbstractNormalizedScrollPosition = 1f;

				lastProcessedAbstractNormalizedScrollPosition = 1f;
			}
			internal void ResetLastProcessedNormalizedScrollPositionToEnd()
			{
				// Commented: not needed anymore, as now start=1, end=0, regardless of the scroll type
				//if (_SourceParams.scrollRect.horizontal)
				//    lastProcessedAbstractNormalizedScrollPosition = 0f;
				//else
				//lastProcessedAbstractNormalizedScrollPosition = 1f;

				lastProcessedAbstractNormalizedScrollPosition = 0f;
			}

			internal float GetItemOffsetFromParentStartUsingItemIndexInView(int itemIndexInView)
			{
				// Commented: using a more efficient way of doing this by using cumulative sizes
				//float distanceFromParentStart = paddingContentStart;
				////float cumulatedSizesOfAllItems = 0f;
				//for (int i = 0; i < itemIndex; ++i)
				//    distanceFromParentStart += itemsSizes[i];
				//distanceFromParentStart += itemIndex * spacing;

				//return distanceFromParentStart;

				float cumulativeSizeOfAllItemsBeforePlusSpacing = 0f;
				if (itemIndexInView > 0)
					cumulativeSizeOfAllItemsBeforePlusSpacing = itemsSizesCumulative[itemIndexInView - 1] + itemIndexInView * spacing;

				return paddingContentStart + cumulativeSizeOfAllItemsBeforePlusSpacing;
			}
			internal float GetItemOffsetFromParentEndUsingItemIndexInView(int itemIndexInView)
			{
				return contentPanelSize - (GetItemOffsetFromParentStartUsingItemIndexInView(itemIndexInView) + itemsSizes[itemIndexInView]);

				//float cumulativeSizeOfAllItemsInclusiveThisOnePlusSpacing = itemsSizesCumulative[itemIndex] + itemIndex * spacing;

				//return paddingContentEnd + (cumulatedSizesOfAllItemsPlusSpacing - cumulativeSizeOfAllItemsInclusiveThisOnePlusSpacing);
			}

			internal int GetItemRealIndexFromViewIndex(int indexInView) { return (realIndexOfFirstItemInView + indexInView) % itemsCount; }
			internal int GetItemViewIndexFromRealIndex(int realIndex) { return (realIndex - realIndexOfFirstItemInView + itemsCount) % itemsCount; }

			internal void OnScrollViewLooped(int newValueOf_RealIndexOfFirstItemInView)
			{
				int oldValueOf_realIndexOfFirstItemInView = realIndexOfFirstItemInView;
				realIndexOfFirstItemInView = newValueOf_RealIndexOfFirstItemInView;

				int arrayRotateAmount = oldValueOf_realIndexOfFirstItemInView - realIndexOfFirstItemInView;
				if (arrayRotateAmount != 0)
				{
					itemsSizes = itemsSizes.GetRotatedArray(arrayRotateAmount);

					float cumulatedSizesOfAllItems = 0f;
					for (int i = 0; i < itemsCount; ++i)
					{
						cumulatedSizesOfAllItems += itemsSizes[i];
						itemsSizesCumulative[i] = cumulatedSizesOfAllItems;
					}
				}
			}

			void OnTotalSizeOfAllItemsChanged(float cumulatedSizeOfAllItems, bool contentPanelEndEdgeStationary, bool rebuild = true)
			{
				cumulatedSizesOfAllItemsPlusSpacing = cumulatedSizeOfAllItems + Mathf.Max(0, itemsCount - 1) * spacing;
				OnCumulatedSizesOfAllItemsPlusSpacingChanged(cumulatedSizesOfAllItemsPlusSpacing, contentPanelEndEdgeStationary, rebuild);
			}


			void OnCumulatedSizesOfAllItemsPlusSpacingChanged(float newValue, bool contentPanelEndEdgeStationary, bool rebuild = true)
			{
				contentPanelSize = cumulatedSizesOfAllItemsPlusSpacing + paddingStartPlusEnd;

				var edgeToUse = contentPanelEndEdgeStationary ? endEdge : startEdge;
				float insetToUse = _SourceParams.content.GetInsetFromParentEdge(_SourceParams.viewport, edgeToUse);

				_SourceParams.content.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(_SourceParams.viewport, edgeToUse, insetToUse, contentPanelSize);
				if (rebuild)
				{
					// This way, the content doesn't move, it only grows down
					_SourceParams.scrollRect.Rebuild(CanvasUpdate.PostLayout);
					Canvas.ForceUpdateCanvases();
				}
			}
		}
	}
}
