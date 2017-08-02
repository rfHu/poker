using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace MTTMsgPage
{
    public class TableListGo : MTTCellView
    {
        public Text NumText;
        public Text GamersCountText;
        public Text MinMaxText;

        override public void SetData(MTTPageData data)
        {
            base.SetData(data);
            var dt = data as TableListGoData;
            NumText.text = dt.Num == 0? "决赛桌": dt.Num.ToString();
            GamersCountText.text = dt.gamersCount.ToString();
            MinMaxText.text = dt.Min + "/" + dt.Max;
        }

        override public void CollectViews()
        {
            base.CollectViews();
            NumText = root.Find("Text").GetComponent<Text>();
            GamersCountText = root.Find("Text (1)").GetComponent<Text>();
            MinMaxText = root.Find("Text (2)").GetComponent<Text>();
        }

        public override bool CanPresentModelType(Type modelType)
        {
            return modelType == typeof(TableListGoData);
        }
    }
}