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

        protected Image _Image;

        public virtual void SetData(MTTPageData data)
        {
            _data = data;
            _Image.enabled = data.needbg;
        }

        override public void CollectViews() 
        {
            base.CollectViews();
            _Image = root.GetComponent<Image>();
        }

        public abstract bool CanPresentModelType(Type modelType);


    }
}