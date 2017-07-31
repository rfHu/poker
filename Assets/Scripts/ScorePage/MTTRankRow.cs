using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class MTTRankRow: RankRow {
        protected Text HeadCount;

         override public void CollectViews() {
            base.CollectViews();
            HeadCount = root.Find("HeadCount").GetComponent<Text>();
        }

        override public void SetData(Data data) {
            var dt = data as MTTRankRowData;
            base.SetData(dt.RankData);

            HeadCount.text = dt.HeadCount.ToString();
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(MTTRankRowData); }
    }
}