using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;


namespace ScorePage {
    public class Award27Row: CellView
    {
        private Text numberText;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as Data27;
            SetNumber(numberText, dt.Number);
        }

        override public void CollectViews() {
            base.CollectViews();
            numberText = root.Find("Score").GetComponent<Text>();
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(Data27); }
    }
}
 