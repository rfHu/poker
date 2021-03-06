using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class MTTRankRow: RankRow {
        protected Text headCount;
        protected Text headAward;

         override public void CollectViews() {
            base.CollectViews();
            headCount = root.Find("HeadCount").GetComponent<Text>();
            headAward = root.Find("Hunter").GetComponent<Text>();
        }

        override public void SetData(Data data) {
            var dt = data as MTTRankRowData;
            base.SetData(dt.RankData);

            headCount.text = dt.HeadCount.ToString();

            var go = headAward.gameObject;
            var rect = scoreText.GetComponent<RectTransform>();

            var white = new Color(1 ,1, 1);
            var yellow = _.HexColor("#ffd028");

            if (GameData.MatchData.IsHunter) {
                go.SetActive(true); 
                headAward.text = _.Num2CnDigit(dt.HeadAward);
                rect.anchoredPosition = new Vector2(-32, 24);
            } else {
                go.SetActive(false);
                rect.anchoredPosition = new Vector2(-32, 0);
            }

            if (dt.RankData.Uid == GameData.Shared.Uid) {
                headAward.color = yellow;
                headCount.color = yellow;
            } else {
                headAward.color = white;
                headCount.color = white;
            }
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(MTTRankRowData); }
    }
}