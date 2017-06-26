using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class InsuranceRow : CellView
    {
        public Text scoreText;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as InsuranceRowData;
            SetNumber(scoreText, dt.Number);
        }

        override public void CollectViews() {
            base.CollectViews();
            scoreText = root.Find("Score").GetComponent<Text>();
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(InsuranceRowData); }
    }
}
 