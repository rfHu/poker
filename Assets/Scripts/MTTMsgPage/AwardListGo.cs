using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MTTMsgPage
{
    public class AwardListGo : MTTCellView
    {
        public Text RankText;
        public Text AwardText;

        override public void SetData(MTTPageData data)
        {
            base.SetData(data);
            var dt = data as AwardListGoData;
            RankText.text = dt.Rank.ToString();
            AwardText.text = dt.Award;
        }

        override public void CollectViews()
        {
            base.CollectViews();
            RankText = root.Find("Text").GetComponent<Text>();
            AwardText = root.Find("Text (1)").GetComponent<Text>();
        }

        public override bool CanPresentModelType(Type modelType)
        {
            return modelType == typeof(AwardListGoData);
        }
    }
}