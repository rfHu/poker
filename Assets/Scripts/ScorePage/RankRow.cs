using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class RankRow : CellView
    {
        private Text nickText;
        private Text scoreText;
        private CanvasGroup cvg;

        private SNGRank1 rank1;
        private GameObject rank2;
        private Text rankText;


        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as RankRowData;
            var white = new Color(1 ,1, 1);
            var yellow = _.HexColor("#ffd028");

            if (GameData.Shared.Uid == dt.Uid) {
                nickText.color = yellow;
                scoreText.color = yellow;
            } else {
                nickText.color = white;
                scoreText.color = white;
            }           

            nickText.text = dt.Nick;
            scoreText.text = _.Num2CnDigit(dt.Score);
            
            if (dt.Score == 0) {
                cvg.alpha = 0.7f;
            } else {
                cvg.alpha = 1;  
            }

            if (dt.Rank <= 0) {
                rank1.gameObject.SetActive(false);
                rank2.gameObject.SetActive(false);
            } else if (dt.Rank <= 3) {
                rank1.gameObject.SetActive(true);
                rank2.gameObject.SetActive(false);
                rank1.SetRank(dt.Rank);
            } else {
                rank1.gameObject.SetActive(false);
                rank2.gameObject.SetActive(true);
                rankText.text = dt.Rank.ToString();
            }
        }

        override public void CollectViews() {
            base.CollectViews();

            nickText = root.Find("Name").GetComponent<Text>();
            scoreText = root.Find("Score").GetComponent<Text>();
            rank1 = root.Find("Rank1").GetComponent<SNGRank1>();
            rank2 = root.Find("Rank2").gameObject;
            rankText = rank2.transform.Find("Text").GetComponent<Text>();

            cvg = root.GetComponent<CanvasGroup>();
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(RankRowData); }
    }
}
 