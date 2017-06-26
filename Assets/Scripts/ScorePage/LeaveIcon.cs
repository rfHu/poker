using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class LeaveIcon : CellView
    {
        override public void SetData(Data data)
        {
            base.SetData(data);
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(LeaveIconData); }
    }
}
 