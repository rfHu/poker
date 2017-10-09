using BestHTTP.JSON;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MTTMsgPage
{
    public class MTTMsgP2 : MonoBehaviour
    {
        public Text JackpotTotal;
        public Text Count;

        public MyParams adapterParams;

        MyScrollRectAdapter _Adapter;

        void OnEnable()
        {
            _Adapter = new MyScrollRectAdapter();
            _Adapter.Init(adapterParams);
        }

        void OnDisable()
        {
            if (_Adapter != null)
                _Adapter.Dispose();
        }

        [Serializable]
        public class MyParams : BaseParams
        {
            public RectTransform AwardListGo;

            public List<MTTPageData> rowData = new List<MTTPageData>();
        }

        public class MyScrollRectAdapter : ScrollRectItemsAdapter8<MyParams,MTTCellView>
        {
            protected override float GetItemWidth(int index)
            { return 780; }

            protected override float GetItemHeight(int index)
            { return 80; }

            protected override MTTCellView CreateViewsHolder(int itemIndex)
            {
                var data = _Params.rowData[itemIndex];
                MTTCellView instance;

                instance = new AwardListGo();
                instance.Init(_Params.AwardListGo, itemIndex);
                
                return instance;
            }

            protected override void UpdateViewsHolder(MTTCellView newOrRecycled)
            {
                MTTPageData model = _Params.rowData[newOrRecycled.itemIndex];
                newOrRecycled.SetData(model);
            }

            protected override bool IsRecyclable(MTTCellView potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
            { return potentiallyRecyclable.CanPresentModelType(_Params.rowData[indexOfItemThatWillBecomeVisible].cachedType); }
        }

        public void requestData() 
        {

            HTTP.Get("/match-award", new Dictionary<string, object> {
                {"match_id", GameData.Shared.MatchID },
            }, (data) =>
            {
                StartCoroutine(SetData(data));
            });
        }

        IEnumerator SetData(string data)
        {
            var rowData = new List<MTTPageData>();
            var awardMsg = Json.Decode(data) as Dictionary<string, object>;

            var total = awardMsg.Int("total");
            JackpotTotal.text = total == -1 ? "固定奖池" : total.ToString();
            Count.text = awardMsg.Int("count").ToString();

            var roomsMsg = awardMsg.List("list");

            bool needWaiting = transform.parent.parent.GetComponent<MTTMsg>().SetGoSize(roomsMsg.Count > 5);

            if (needWaiting)
            {
                yield return new WaitForSeconds(0.2f);
            }

            _Adapter.Init(adapterParams);

            for (int i = 0; i < roomsMsg.Count; i++)
            {
                var msg = roomsMsg[i] as Dictionary<string, object>;

                var awardData = new AwardListGoData()
                {
                    Rank = msg.String("rank"),
                    Award = msg.String("award"),
                    needbg = (i % 2 == 0),
                };
                rowData.Add(awardData);
            }

            adapterParams.rowData.Clear();
            adapterParams.rowData.AddRange(rowData);
            _Adapter.ChangeItemCountTo(rowData.Count);
        }
    }
}