using frame8.Logic.Misc.Other.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
    /// <summary>
    /// <para>Input params to be passed to <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.Init(TParams)"/></para>
    /// <para>This can be used Monobehaviour's field and exposed via inspector (most common case)</para>
    /// <para>Or can be manually constructed, depending on what's easier in your context</para>
    /// </summary>
    [System.Serializable]
    public class BaseParams
    {
        /// <summary>
        /// <para>How much objects to always keep in memory, no matter what. This includes visible items + items in the recycle bin. </para>
        /// <para>The recycle bin will always have at least one item in it, regardless of this setting. Set to -1 or 0 to Detect automatically (Recommended!). </para>
        /// <para>Change it only if you know what you're doing (usually, it's the estimated number of visible items + 1)</para>
        /// <para>Last note: this field will only be considered after the number &lt;visible+in the bin&gt; grows past it</para>
        /// </summary>
        [Header("Optimizing Process")]
        [Tooltip("How much objects to always keep in memory, no matter what. "+
            "This includes visible items + items in the recycle bin. "+
            " The recycle bin will always have at least one item in it, "+
            "regardless of this setting. Set to -1 or 0 to Detect automatically (Recommended!). "+
            "Change it only if you know what you're doing (usually, it's the estimated number of visible items + 1)"+
            ". Last note: this field will only be considered after the number <visible+in the bin> grows past it")]
        public int minNumberOfObjectsToKeepInMemory = -1;

        /// <summary>See <see cref="BaseParams.UpdateMode"/> enum for full description. The default is ON_SCROLL_THEN_MONOBEHAVIOUR_UPDATE and if the framerate is acceptable, it should be leaved this way</summary>
        [Tooltip("See BaseParams.UpdateMode enum for full description. The default is ON_SCROLL_THEN_MONOBEHAVIOUR_UPDATE and if the framerate is acceptable, it should be leaved this way")]
        public UpdateMode updateMode = UpdateMode.ON_SCROLL_THEN_MONOBEHAVIOUR_UPDATE;

        /// <summary>
        /// <para>If true: When the last item is reached, the first one appears after it, basically allowing you to scroll infinitely.</para>
        /// <para>Initially intended for things like spinners, but it can be used for anything alike. It may interfere with other functionalities in some very obscure/complex contexts/setups, so be sure to test the hell out of it.</para>
        /// <para>Also please note that sometimes during dragging the content, the actual looping changes the Unity's internal PointerEventData for the current click/touch pointer id, </para>
        /// <para>so if you're also externally tracking the current click/touch, in this case only 'PointerEventData.pointerCurrentRaycast' and 'PointerEventData.position'(current position) are </para>
        /// <para>preserved, the other ones are reset to defaults to assure a smooth loop transition</para>
        /// </summary>
        [Tooltip("If true: When the last item is reached, the first one appears after it, basically allowing you to scroll infinitely.\n" +
            " Initially intended for things like spinners, but it can be used for anything alike.\n" +
            " It may interfere with other functionalities in some very obscure/complex contexts/setups, so be sure to test the hell out of it.\n" +
            " Also please note that sometimes during dragging the content, the actual looping changes the Unity's internal PointerEventData for the current click/touch pointer id, so if you're also externally tracking the current click/touch, in this case only 'PointerEventData.pointerCurrentRaycast' and 'PointerEventData.position'(current position) are preserved, the other ones are reset to defaults to assure a smooth loop transition. Sorry for the long decription. Here's an ASCII potato: (@)")]
        public bool loopItems;

        /// <summary>The ScrollRect that'll be optimized</summary>
        [Space(10, order = 0)]
        public ScrollRect scrollRect;

        /// <summary>If null, <see cref="BaseParams.scrollRect"/> is considered to be the viewport</summary>
        [Tooltip("If null, the scrollRect is considered to be the viewport")]
        public RectTransform viewport;

        /// <summary>If null, will be the same as <see cref="ScrollRect.content"/></summary>
        [Tooltip("If null, will be the same as scrollRect.content")]
        public RectTransform content;

        /// <summary>Padding for the 4 edges of the content panel</summary>
        [Tooltip("This is used instead of the old way of putting a disabled LayoutGroup component on the content")]
        public RectOffset contentPadding = new RectOffset();

        /// <summary>Spacing between items (horizontal is the ScrollRect is horizontal. else, vertical)</summary>
        [Tooltip("This is used instead of the old way of putting a disabled LayoutGroup component on the content")]
        public float contentSpacing;

        /// <summary>Don't use it. It's here just so the class can be serialized by Unity when used as a MonoBehaviour's field. Use <see cref="BaseParams.BaseParams(ScrollRect)"/> or <see cref="BaseParams.BaseParams(ScrollRect, RectTransform, RectTransform)"/> instead</summary>
        public BaseParams() { }

        public BaseParams(BaseParams other)
        {
            this.minNumberOfObjectsToKeepInMemory = other.minNumberOfObjectsToKeepInMemory;
            this.updateMode = other.updateMode;
            this.scrollRect = other.scrollRect;
            this.viewport = other.viewport;
            this.content = other.content;
            this.contentPadding = other.contentPadding == null ? new RectOffset() : new RectOffset(contentPadding.left, contentPadding.right, contentPadding.top, contentPadding.bottom);
            this.contentSpacing = other.contentSpacing;
        }

        public BaseParams(ScrollRect scrollRect)
            :this(scrollRect, scrollRect.transform as RectTransform, scrollRect.content)
        {}

        public BaseParams(ScrollRect scrollRect, RectTransform viewport, RectTransform content)
        {
            this.scrollRect = scrollRect;
            this.viewport = viewport ?? scrollRect.transform as RectTransform;
            this.content = content ?? scrollRect.content;
        }

        /// <summary>Called internally. This makes sure to the content and viewport have valid values</summary>
        internal void InitIfNeeded()
        {
            // Commented: null-coalescing operator doesn't work with unity's Object
            //content = content ?? scrollRect.content;
            if (viewport == null)
                viewport = scrollRect.transform as RectTransform;
            if (content == null)
                content = scrollRect.content;

                //cachedWorldCornersArray1 = new float[4];
                //cachedWorldCornersArray2 = new float[4];
        }

        /// <summary>
        /// <para>Used internally. Returns values in [0f, 1f] interval, 1 meaning the scrollrect is at start, and 0 meaning end. The problem (for the adapter's purposes) with <see cref="ScrollRect.normalizedPosition"/> is </para>
        /// <para>that it behaves the same way for the vertical scroll position, but opposite for the horizontal scroll position.</para>
        /// <para>It also uses a different approach when content size is smaller than viewport's size, so it can yield consistent results for <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}.ComputeVisibility(float)"/></para>
        /// </summary>
        /// <returns>1 Meaning Start And 0 Meaning End</returns> 
        internal float GetAbstractNormalizedScrollPosition()
        {
            if (scrollRect.horizontal)
            {
                float vpw = viewport.rect.width;
                float cw = content.rect.width;
                if (vpw > cw)
                {
                    //viewport.GetWorldCorners(cachedWorldCornersArray1);
                    //content.GetWorldCorners(cachedWorldCornersArray2);

                    return content.GetInsetFromParentLeftEdge(viewport) / vpw;
                }

                return 1f - scrollRect.horizontalNormalizedPosition;
            }

            float vph = viewport.rect.height;
            float ch = content.rect.height;
            if (vph > ch)
            {
                //viewport.GetWorldCorners(cachedWorldCornersArray1);
                //content.GetWorldCorners(cachedWorldCornersArray2);

                return content.GetInsetFromParentTopEdge(viewport) / vph;
            }

            return scrollRect.verticalNormalizedPosition;
        }

        internal float TransformVelocityToAbstract(Vector2 rawVelocity)
        {
            if (scrollRect.horizontal)
            {
                rawVelocity.x = -rawVelocity.x;

                return rawVelocity.x;
            }

            return rawVelocity.y;
        }

        internal void ScrollToStart()
        {
            if (scrollRect.horizontal)
                scrollRect.horizontalNormalizedPosition = 0f;
            else
                scrollRect.verticalNormalizedPosition = 1f;
        }

        internal void ScrollToEnd()
        {
            if (scrollRect.horizontal)
                scrollRect.horizontalNormalizedPosition = 1f;
            else
                scrollRect.verticalNormalizedPosition = 0f;
        }

        internal void ClampScroll01()
        {
            if (scrollRect.horizontal)
                scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
            else
                scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
        }

        /// <summary> Represents how often or when the optimizer does his core loop: checking for any items that need to be created, destroyed, disabled, displayed, recycled</summary>
        public enum UpdateMode
        {
            /// <summary>
            /// <para>Updates are triggered by a MonoBehaviour.Update() (i.e. each frame the ScrollView is active) and at each OnScroll event</para>
            /// <para>Moderate performance when scrolling, but works in all cases</para>
            /// </summary>
            ON_SCROLL_THEN_MONOBEHAVIOUR_UPDATE,

            /// <summary>
            /// <para>Updates ar triggered by each OnScroll event</para>
            /// <para>Experimental. However, if you use it and see no issues, it's recommended over ON_SCROLL_THEN_MONOBEHAVIOUR_UPDATE.</para>
            /// <para>This is also useful if you don't want the optimizer to use CPU when idle.</para>
            /// <para>A bit better performance when scrolling</para>
            /// </summary>
            ON_SCROLL,

            /// <summary>
            /// <para>Update is triggered by a MonoBehaviour.Update() (i.e. each frame the ScrollView is active)</para>
            /// <para>In this mode, some temporary gaps appear when fast-scrolling. If this is not acceptable, use other modes.</para>
            /// <para>Best performance when scrolling, items appear a bit delayed when fast-scrolling</para>
            /// </summary>
            MONOBEHAVIOUR_UPDATE
        }
    }
}
