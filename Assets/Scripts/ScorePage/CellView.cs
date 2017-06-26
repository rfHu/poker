using UnityEngine;
using System.Collections;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine.UI;
using System;

namespace ScorePage
{
       public abstract class CellView : BaseItemViewsHolder  
    {
        protected Data _data;

        public virtual void SetData(Data data)
        {
            _data = data;
        }

        public abstract bool CanPresentModelType(Type modelType);

        public void SetNumber(Text text, int num) {
            text.text = _.Number2Text(num);
            text.color = _.GetTextColor(num);
        }
    }
}