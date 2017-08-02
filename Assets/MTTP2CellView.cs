using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MTTMsgPage
{
    public abstract class MTTCellView : BaseItemViewsHolder
    {
        protected MTTPageData _data;

        public virtual void SetData(MTTPageData data)
        {
            _data = data;
        }

        public abstract bool CanPresentModelType(Type modelType);
    }
}