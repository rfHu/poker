using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace MTTMsgPage
{
    public abstract class MTTCellView : BaseItemViewsHolder
    {
        protected MTTPageData _data;

        protected ProceduralImage _produralImage;

        public virtual void SetData(MTTPageData data)
        {
            _data = data;
            _produralImage.enabled = data.needbg;
        }

        override public void CollectViews() 
        {
            base.CollectViews();
            _produralImage = root.GetComponent<ProceduralImage>();
        }

        public abstract bool CanPresentModelType(Type modelType);


    }
}