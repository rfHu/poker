using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;


namespace ScorePage {
    public class ScoreHeader: CellView
    {
        private Text T1;
        private Text T2;
        private Text T3;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as ScoreHeaderData;

            T1.text = dt.List[0];
            T2.text = dt.List[1];
            T3.text = dt.List[2];
        }

        override public void CollectViews() {
            base.CollectViews();
            T1 = root.Find("T1").GetComponent<Text>();
            T2 = root.Find("T2").GetComponent<Text>();
            T3 = root.Find("T3").GetComponent<Text>();
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(ScoreHeaderData); }
    }
}
 