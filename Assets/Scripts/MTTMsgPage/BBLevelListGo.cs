using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MTTMsgPage
{
    public class BBLevelListGo : MTTCellView
    {
        public Text LevelText;
        public Text SBBBText;
        public Text AnteText;

        override public void SetData(MTTPageData data)
        {
            base.SetData(data);
            var dt = data as BBLevelListGoData;
            LevelText.text = dt.Level.ToString();
            int SB = dt.BB/2;
            SBBBText.text = SetNumber(SB) + "/" + SetNumber(dt.BB);
            AnteText.text = dt.Ante.ToString();

            if (dt.Level - 1 == GameData.Shared.BlindLv)
            {
                SBBBText.color = AnteText.color = new Color(24 / 255, 1, 1);
            }
            else 
            {
                SBBBText.color = AnteText.color = new Color(1, 1, 1, 0.4f);
            }
        }

        override public void CollectViews()
        {
            base.CollectViews();
            LevelText = root.Find("Text").GetComponent<Text>();
            SBBBText = root.Find("Text (1)").GetComponent<Text>();
            AnteText = root.Find("Text (2)").GetComponent<Text>();
        }

        public override bool CanPresentModelType(Type modelType)
        {
            return modelType == typeof(BBLevelListGoData);
        }

        private string SetNumber(int num) 
        {
            return num >= 100000 ? num / 10000f + "万" : num.ToString();
        }
    }
}