using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;

namespace ScorePage {
    public class Award27Row: CellView
    {
        public Text NumberText;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as Data27;
            SetNumber(NumberText, dt.Number);
        }
    }
}
 