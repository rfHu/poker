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
            ScoreText.text = _.Num2CnDigit(dt.Score); 

            var cvg = GetComponent<CanvasGroup>();
            if (dt.HasSeat) {
                cvg.alpha = 1;
            } else {
                cvg.alpha = 0.6f;  
            }
        }
    }
}
 