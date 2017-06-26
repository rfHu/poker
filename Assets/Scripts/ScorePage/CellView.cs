using UnityEngine;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using UnityEngine.UI;

namespace ScorePage
{
       public class CellView : EnhancedScrollerCellView
    {
        protected Data _data;

        public virtual void SetData(Data data)
        {
            _data = data;
        }

        public void SetNumber(Text text, int num) {
            text.text = _.Number2Text(num);
            text.color = _.GetTextColor(num);
        }
    }
}