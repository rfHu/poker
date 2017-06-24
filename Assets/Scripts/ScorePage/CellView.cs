using UnityEngine;
using System.Collections;
using EnhancedUI.EnhancedScroller;

namespace ScorePage
{
       public class CellView : EnhancedScrollerCellView
    {
        protected Data _data;

        public virtual void SetData(Data data)
        {
            _data = data;
        }
    }
}