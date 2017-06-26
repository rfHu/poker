using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class GuestHeader : CellView
    {
        public Text guestNumberText;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as GuestHeadData;
            guestNumberText.text = string.Format("旁观({0})", dt.Number.ToString());
        }

        override public void CollectViews() {
            base.CollectViews();
            guestNumberText = root.Find("Text").GetComponent<Text>();
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(GuestHeadData); }
    }
}
 