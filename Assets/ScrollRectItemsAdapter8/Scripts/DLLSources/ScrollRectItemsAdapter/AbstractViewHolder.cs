
using UnityEngine;
using UnityEngine.UI;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
    /// <summary>
    /// Class representing the concept of a View Holder, i.e. a class that references some views and the id of the data displayed by those views. 
    /// Usually, the root and its child views, once created, don't change, but <see cref="itemIndex"/> does, after which the views will change their data.
    /// </summary>
    public abstract class AbstractViewHolder
    {
        /// <summary> The index of the data model from which this viewholder's views take their display information </summary>
        public virtual int itemIndex { get; set; }

        /// <summary>The root of the view instance (which contains the actual views)</summary>
        public RectTransform root;


        /// <summary> Calls <see cref="Init(GameObject, int, bool, bool)"/> </summary>
        public virtual void Init(RectTransform rootPrefab, int itemIndex, bool activateRootGameObject = true, bool callCollectViews = true)
        { Init(rootPrefab.gameObject, itemIndex, activateRootGameObject, callCollectViews); }

        /// <summary>Instantiates rootPrefabGO, assigns it to root and sets its itemIndex to <paramref name="itemIndex"/>. Activates the new instance if <paramref name="activateRootGameObject"/> is true. Also calls CollectViews if <paramref name="callCollectViews"/> is true</summary>
        /// <param name="rootPrefabGO"></param>
        /// <param name="itemIndex"></param>
        public virtual void Init(GameObject rootPrefabGO, int itemIndex, bool activateRootGameObject = true, bool callCollectViews = true)
        {
            root = (GameObject.Instantiate(rootPrefabGO) as GameObject).transform as RectTransform;
            if (activateRootGameObject)
                root.gameObject.SetActive(true);
            this.itemIndex = itemIndex;

            if (callCollectViews)
                CollectViews();
        }

        /// <summary>If instead of calling <see cref="Init(GameObject, int, bool, bool)"/>, the initializaton is done manually, this should be called lastly as part of the initialization phase</summary>
        public virtual void CollectViews()
        { }

		public virtual void MarkForRebuild() { if (root) LayoutRebuilder.MarkLayoutForRebuild(root); }
    }
}
