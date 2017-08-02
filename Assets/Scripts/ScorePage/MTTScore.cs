using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using System;
using UniRx;

namespace ScorePage {

    public class MTTScore : MonoBehaviour 
    {
        public MyParams adapterParams;

        MyScrollRectAdapter _Adapter; 
        private int currentIndex = 0;
        private List<PlayerModel> guestList;
        private ScoreHeaderData headerData = new ScoreHeaderData() {
            List = new List<string>() {
                "昵称",
                "人头数",
                "记分牌"
            } 
        };

        void Start()
        {
            _Adapter = new MyScrollRectAdapter();
            _Adapter.Init(adapterParams);

            RxSubjects.MTTChangeTabIndex.Subscribe((idx) => {
                if (currentIndex == idx) {
                    return ;
                }

                currentIndex = idx;

                if (idx == 0) {
                    requestData();
                } else {
                    requestTop20();
                }
           }).AddTo(this);
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
            public RectTransform TabPrefab;

            public List<Data> rowData = new List<Data>();
        }

        public class MyScrollRectAdapter : ScrollRectItemsAdapter8<MyParams, CellView> 
        {
            protected override float GetItemHeight(int index)
            { 
                var data = _Params.rowData[index];

                if (data is MTTRankRowData) {
                    if (GameData.MatchData.IsHunter) {
                        return 128f;
                    } else {
                        return 90f;
                    }
                } else if (data is GuestHeadData || data is ScoreHeaderData) {
                    return 80f;
                } else if (data is MTTTabData) {
                    return 128f;
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
                    instance =  new MTTRankRow();
                    instance.Init(_Params.RankPrefab, itemIndex);
                } else if (data is MTTTabData) {
                    instance = new MTTTabRow();
                    instance.Init(_Params.TabPrefab, itemIndex);
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

        void OnDespawned() {
            currentIndex = 0;
        }

        private void requestTop20() {
            Connect.Shared.Emit(new Dictionary<string, object>(){
                {"f", "gamerrank"},
                {"for_match", 1}
            }, (json) => {
                var rowData = new List<Data>();
                rowData.Add(headerData);

                var list = json.DL("list");
                foreach(var item in list) {
                    rowData.Add(new MTTRankRowData() {
                        RankData = new RankRowData() 
                        {
                            Score = item.Int("bankroll"),
                            Rank = item.Int("rank"),
                            Uid = item.String("uid"),
                            Nick = item.Dict("profile").String("name")
                        },

                        HeadCount = item.Int("head_count"),
                        HeadAward = item.Int("award_value") 
                    });
                }

                rowData.AddRange(getLeftData(1));
                adapterParams.rowData.Clear();
                adapterParams.rowData.AddRange(rowData);
                _Adapter.ChangeItemCountTo(rowData.Count);
            });
        }

        private void requestData() {
            Connect.Shared.Emit(new Dictionary<string, object>(){
                {"f", "gamerlist"}
            }, (json) =>
            {
                var rowData = new List<Data>();
                guestList = new List<PlayerModel>();

                var rankList = new List<Data>();

                rowData.Add(headerData);

                foreach(object item in json.List("list")) {
                    var dict = item as Dictionary<string, object>;
                    if (dict == null) {
                        continue;
                    }

                    var model = dict.ToObject<PlayerModel>(); 
                
                    // 小于0不应该展示在排行榜
                    if (model.seat < 0) {
                        if (model.in_room) { // 在线的游客才展示
                            guestList.Add(model);
                        }
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
                rowData.AddRange(getLeftData(0));               

                adapterParams.rowData.Clear();
                adapterParams.rowData.AddRange(rowData);
                _Adapter.ChangeItemCountTo(rowData.Count);
            });
        }

        private List<Data> getLeftData(int selecetdIndex) {
            var rowList = new List<Data>();

            rowList.Add(new MTTTabData() {
                SelectedIndex = selecetdIndex
            });

            rowList.Add(new GuestHeadData() {
                Number = guestList.Count
            });

            var chunkList = guestList.ChunkBy(4);
            foreach(var list in chunkList) {
                rowList.Add(new GuestRowData() {
                    PlayerList = list
                });
            }

            return rowList;
        }
    }
}


