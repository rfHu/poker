using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;

namespace ScorePage {
    public class PlayerRow : CellView
    {
        public Text NickText;
        public Text TakeCoinText;
        public Text ScoreText;
        public RawImage LeaveIcon;
        public CanvasGroup Cvg;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as PlayerRowData;

            NickText.text = dt.Nick;
            TakeCoinText.text = _.Num2CnDigit(dt.TakeCoin);
            ScoreText.text = _.Num2CnDigit(dt.Score); 

            if (dt.HasSeat) {
                ScoreText.color = _.GetTextColor(dt.Score);
            }

            if (dt.HasSeat) {
                Cvg.alpha = 1;
            } else {
                Cvg.alpha = 0.6f;  
            }

            if (dt.LeaveFlag) {
                LeaveIcon.enabled = true;
            } else {
                LeaveIcon.enabled = false;
            }
        }
    }
}
 