using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class LeaveIcon : CellView
    {
        public Text LabelText;

        override public void SetData(Data data)
        {
            base.SetData(data);
            LabelText.text = (data as LeaveIconData).Label;    
        }

        override public void CollectViews() {
            base.CollectViews();
            LabelText = root.Find("Icon").Find("Text").GetComponent<Text>();
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(LeaveIconData); }
    }
}
 