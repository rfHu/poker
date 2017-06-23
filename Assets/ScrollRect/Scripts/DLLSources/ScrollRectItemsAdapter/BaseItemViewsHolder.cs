
namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
    /// <summary>The minimal implementation of a View Holder that can be used with <see cref="ScrollRectItemsAdapter8{TParams, TItemViewsHolder}"/></summary>
    public class BaseItemViewsHolder : AbstractViewHolder
    {
        /// <summary> Only used if the scroll rect is looping, otherwise is the same as <see cref="AbstractViewHolder.itemIndex"/>; See <see cref="BaseParams.loopItems"/></summary>
        public int itemIndexInView;

        internal float cachedSize; // used internally // formerly known as "cachedHeight", but since now the adapter works with horizontal scrolling also, we're calling it "size". it's height/width, depending on the scroll type

        // Commented: cannot call CollectViews from constructor; we didn't need the 2-parameter constructor anyway
        //public BaseItemViewsHolder() { }

        //public BaseItemViewsHolder(UnityEngine.RectTransform root, int itemIndex)
        //{
        //    this.root = root;
        //    this.itemIndex = itemIndex;
        //    CollectViews();
        //}
    }
}
