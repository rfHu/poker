using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;

namespace ScorePage {
    public class GuestHeader : CellView
    {
        public Text GuestNumberText;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as GuestHeadData;
            GuestNumberText.text = string.Format("旁观({0})", dt.Number.ToString());
        }
    }
}
 