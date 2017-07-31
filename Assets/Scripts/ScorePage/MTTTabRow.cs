using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI.ProceduralImage;

namespace ScorePage {
    public class MTTTabRow : CellView
    {
        private List<Transform> tabs;

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as MTTTabData;
            var oppo = dt.SelectedIndex ^ 1;

            tabs[dt.SelectedIndex].GetComponent<ProceduralImage>().color = _.HexColor("#4a617e");
            tabs[oppo].GetComponent<ProceduralImage>().color = _.HexColor("#383F47");

            tabs[dt.SelectedIndex].Find("Text").GetComponent<Text>().color = MaterialUI.MaterialColor.cyan200;
            tabs[oppo].Find("Text").GetComponent<Text>().color = _.HexColor("#FFFFFFB2");
        }

        override public void CollectViews() {
            base.CollectViews();
            var tab = root.Find("Tab");
            tabs.Add(tab.Find("Room"));
            tabs.Add(tab.Find("Top20"));

            var btn1 = tabs[0].GetComponent<Button>();
            var btn2 = tabs[1].GetComponent<Button>();

            btn1.onClick.RemoveAllListeners();
            btn2.onClick.RemoveAllListeners();

            btn1.onClick.AddListener(() => {
                onClick(0);
            });
            btn2.onClick.AddListener(() => {
                onClick(1);
            });
        }

        private void  onClick(int index) {
            RxSubjects.MTTChangeTabIndex.OnNext(index);
        }

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(MTTTabData); }
    }
}
 