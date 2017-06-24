using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;

namespace ScorePage {
    public class InsuranceRow : CellView
    {
        public Text ScoreText;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as InsuranceRowData;

            ScoreText.text = _.Number2Text(dt.Number);
            ScoreText.color = _.GetTextColor(dt.Number);
        }
    }
}
 