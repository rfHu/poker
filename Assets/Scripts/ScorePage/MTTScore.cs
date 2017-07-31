using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using System;

namespace ScorePage {

    public class MTTScore : MonoBehaviour 
    {
        public MyParams adapterParams;

        MyScrollRectAdapter _Adapter; 

        void Start()
        {
            _Adapter = new MyScrollRectAdapter();
            _Adapter.Init(adapterParams);
        }

          void OnDestroy()
        {
            if (_Adapter != null)
                _Adapter.Dispose();
        }

        [Serializable]
        public class MyParams: BaseParams {
            public RectTransform GuestHeaderPrefab;
            public RectTransform GuestPrefab;
            public RectTransform ScoreHeaderPrefab;
            public RectTransform RankPrefab;

            public List<Data> rowData = new List<Data>();
        }

        public class MyScrollRectAdapter : ScrollRectItemsAdapter8<MyParams, CellView> 
        {
            protected override float GetItemHeight(int index)
            { 
                var data = _Params.rowData[index];

                if (data is MTTRankRowData) {
                    return 90f;
                } else if (data is GuestHeadData || data is ScoreHeaderData) {
                    return 80f;
                } 
                else {
                    return 220f;
                }
             }

            protected override float GetItemWidth(int index)
            { return 800; }

            protected override CellView CreateViewsHolder(int itemIndex)
            {
                var data = _Params.rowData[itemIndex];
                CellView instance; 

                if (data is GuestHeadData) {
                    instance = new GuestHeader();
                    instance.Init(_Params.GuestHeaderPrefab, itemIndex);
                } else if (data is ScoreHeaderData) {
                    instance = new ScoreHeader();
                    instance.Init(_Params.ScoreHeaderPrefab, itemIndex);
                } else if (data is MTTRankRowData) {
                    instance =  new RankRow();
                    instance.Init(_Params.RankPrefab, itemIndex);
                } else  {
                    instance = new GuestRow();    
                    instance.Init(_Params.GuestPrefab, itemIndex);
                } 

                return instance;
            }

            protected override void UpdateViewsHolder(CellView newOrRecycled)
            {
                Data model = _Params.rowData[newOrRecycled.itemIndex];
                newOrRecycled.SetData(model);
            }

            protected override bool IsRecyclable(CellView potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
            { return potentiallyRecyclable.CanPresentModelType(_Params.rowData[indexOfItemThatWillBecomeVisible].cachedType); }
        }
      
        void OnSpawned()
        {
            requestData();
        }

        private void requestData() {
            Connect.Shared.Emit(new Dictionary<string, object>(){
                {"f", "gamerlist"}
            }, (json) =>
            {
                var rowData = new List<Data>();
                var guestList = new List<PlayerModel>();
                var rankList = new List<Data>();

                foreach(object item in json.List("list")) {
                    var dict = item as Dictionary<string, object>;
                    if (dict == null) {
                        continue;
                    }

                    var model = dict.ToObject<PlayerModel>(); 
                
                    if (model.seat < 0 && model.in_room) {
                        guestList.Add(model);
                        continue;
                    }
                    
                    rankList.Add(new MTTRankRowData() {
                        RankData = new RankRowData() {
                            Score = model.bankroll,
                            Nick = model.name,
                            Uid = model.uid,
                            Rank = model.match_rank
                        },
                        
                        HeadCount = model.head_count,
                        HeadAward = model.award_value
                    });
                }

                rankList.Sort((a , b) => {
                    var aa = a as MTTRankRowData;
                    var bb = b as MTTRankRowData;
                    return aa.RankData.Rank - bb.RankData.Rank;
                });

                rowData.AddRange(rankList);
                
                rowData.Add(new GuestHeadData() {
                    Number = guestList.Count
                });

                var chunkList = guestList.ChunkBy(4);
                foreach(var list in chunkList) {
                    rowData.Add(new GuestRowData() {
                        PlayerList = list
                    });
                }

                adapterParams.rowData.Clear();
                adapterParams.rowData.AddRange(rowData);

                _Adapter.ChangeItemCountTo(rowData.Count);
            });
        }
    }
}


