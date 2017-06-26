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

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as PlayerRowData;

            NickText.text = dt.Nick;
            TakeCoinText.text = _.Num2CnDigit(dt.TakeCoin);
            ScoreText.text = _.Number2Text(dt.Score); 

            if (dt.HasSeat) {
                ScoreText.color = _.GetTextColor(dt.Score);
            }

            var cvg = GetComponent<CanvasGroup>();
            var img = GetComponent<RawImage>();

            if (dt.HasSeat) {
                cvg.alpha = 1;
                img.enabled = false;
            } else {
                cvg.alpha = 0.6f;  
                img.enabled = true;
            }
        }
    }
}
 