using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class PlayerRow : CellView
    {
        private Text nickText;
        private Text takeCoinText;
        private Text scoreText;
        private RawImage image;
        private CanvasGroup cvg;


        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as PlayerRowData;

            nickText.text = dt.Nick;
            takeCoinText.text = _.Num2CnDigit(dt.TakeCoin);
            scoreText.text = _.Number2Text(dt.Score); 

            if (dt.HasSeat) {
                scoreText.color = _.GetTextColor(dt.Score);
            }

            if (dt.HasSeat) {
                cvg.alpha = 1;
                image.enabled = false;
            } else {
                cvg.alpha = 0.6f;  
                image.enabled = true;
            }
        }

        override public void CollectViews() {
            base.CollectViews();

            nickText = root.Find("Name").GetComponent<Text>();
            takeCoinText = root.Find("Total").GetComponent<Text>();
            scoreText = root.Find("Score").GetComponent<Text>();

            image = root.GetComponent<RawImage>();
            cvg = root.GetComponent<CanvasGroup>();
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(PlayerRowData); }
    }
}
 