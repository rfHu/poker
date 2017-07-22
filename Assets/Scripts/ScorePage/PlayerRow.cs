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
            var white = new Color(1 ,1, 1);
            var yellow = _.HexColor("#ffd028");

            if (GameData.Shared.Uid == dt.Uid) {
                nickText.color = yellow;
                takeCoinText.color = yellow;
            } else {
                nickText.color = white;
                takeCoinText.color = white;
            }           

            nickText.text = dt.Nick;
            takeCoinText.text = _.Num2CnDigit(dt.TakeCoin);

            if (GameData.Shared.IsMatch()) {
                if (GameData.Shared.Uid == dt.Uid) {
                    scoreText.color = yellow;
                } else {
                    scoreText.color = white;
                }
                scoreText.text = dt.Score.ToString(); 
            } else {
                scoreText.color = _.GetTextColor(dt.Score);
                scoreText.text = _.Number2Text(dt.Score); 
            }

            if (GameData.Shared.IsMatch() && dt.Score == 0) {
                cvg.alpha = 0.7f;                
                image.enabled = false;
            } else if (dt.HasSeat) {
                cvg.alpha = 1;
                image.enabled = false;
            } else {
                cvg.alpha = 0.7f;  
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
 