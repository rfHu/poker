using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.Models;
using frame8.ScrollRectItemsAdapter.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample.ViewHolders
{
    /// <summary>The view holder that can preset an <see cref="ExpandableModel"/>. It demonstrates the flow of data both from the view to the model and vice-versa</summary>
    public class ExpandableVH : BaseVH
    {
        public RemoteImageBehaviour remoteImageBehaviour;
        public ExpandCollapseOnClick expandCollapseOnClickBehaviour;


        public override void CollectViews()
        {
            base.CollectViews();

            remoteImageBehaviour = root.Find("IconRawImage").GetComponent<RemoteImageBehaviour>();
            expandCollapseOnClickBehaviour = root.GetComponent<ExpandCollapseOnClick>();
        }

        /// <summary>Can only preset models of type <see cref="ExpandableModel"/></summary>
        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(ExpandableModel); }

        internal override void UpdateViews(BaseModel model)
        {
            base.UpdateViews(model);

            var modelAsExpandable = model as ExpandableModel;
            remoteImageBehaviour.Load(modelAsExpandable.imageURL);

            // Modify the recycled expand behavior script so it's up-to-date with the model. 
            if (expandCollapseOnClickBehaviour)
            {
                expandCollapseOnClickBehaviour.expanded = modelAsExpandable.expanded;

                // If the model 'knows' that it's not expanded, then its visualSize field represents the non-expanded size, so let the expand behavior also be aware of this
                if (!modelAsExpandable.expanded)
                    expandCollapseOnClickBehaviour.nonExpandedSize = model.visualSize;
            }
        }
    }
}
