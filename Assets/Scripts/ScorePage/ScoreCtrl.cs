﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using System;

namespace ScorePage {

    public class ScoreCtrl : MonoBehaviour 
    {
        public Text Hands;

        public Text Pot;

        public Text Time;

        public Text Buy;

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
            public RectTransform InsurancePrefab;
            public RectTransform PlayerPrefab;
            public RectTransform GuestHeaderPrefab;
            public RectTransform GuestPrefab;
            public RectTransform LeaveIconPrefab;
            public RectTransform Award27Prefab;
            public RectTransform ScoreHeaderPrefab;
            public RectTransform RankPrefab;

            public List<Data> rowData = new List<Data>();
        }

        public class MyScrollRectAdapter : ScrollRectItemsAdapter8<MyParams, CellView> 
        {
            protected override float GetItemHeight(int index)
            { 
                var data = _Params.rowData[index];

                if (data is InsuranceRowData || data is PlayerRowData || data is Data27 || data is RankRowData) {
                    return 90f;
                } else if (data is GuestHeadData || data is ScoreHeaderData) {
                    return 80f;
                } else if (data is LeaveIconData) {
                    return 44f;
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

                if (data is InsuranceRowData) {
                    instance = new InsuranceRow();
                    instance.Init(_Params.InsurancePrefab, itemIndex);
                } else if (data is PlayerRowData) {
                    instance = new PlayerRow();
                    instance.Init(_Params.PlayerPrefab, itemIndex);
                } else if (data is GuestHeadData) {
                    instance = new GuestHeader();
                    instance.Init(_Params.GuestHeaderPrefab, itemIndex);
                } else if (data is LeaveIconData) {
                    instance = new LeaveIcon();
                    instance.Init(_Params.LeaveIconPrefab, itemIndex);
                } else if (data is Data27) {
                    instance = new Award27Row();
                    instance.Init(_Params.Award27Prefab, itemIndex);
                } else if (data is ScoreHeaderData) {
                    instance = new ScoreHeader();
                    instance.Init(_Params.ScoreHeaderPrefab, itemIndex);
                } else if (data is RankRowData) {
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

                Hands.text = json.String("handid");
                Time.text = json.Int("hand_time").ToString() + "s";

                var PotParentText = Pot.transform.parent.GetComponent<Text>();
                var BuyParentText = Buy.transform.parent.GetComponent<Text>();

                if (GameData.Shared.IsMatch()) {
                    PotParentText.text = "人  数：";
                    BuyParentText.text = "均  筹：";

                    Pot.text = json.String("left_count");
                    Buy.text = json.String("avg_bankroll");

                    rowData.Add(new ScoreHeaderData() {
                        List = new List<string>() {
                            "排名",
                            "昵称",
                            "记分牌"
                        }
                    });
                } else {
                    PotParentText.text = "平均底池：";
                    BuyParentText.text = "平均买入：";

                    Pot.text = json.String("avg_pot");
                    Buy.text = json.String("avg_buy");

                    rowData.Add(new ScoreHeaderData() {
                        List = new List<string>() {
                            "昵称",
                            "带入",
                            "积分"
                        }
                    });
                }

                if (GameData.Shared.NeedInsurance.Value) {
                    rowData.Add(
                        new InsuranceRowData() {Number = json.Dict("insurance").Int("pay")}
                    );
                }

                var award27 = json.Int("award_27");
                if (award27 != 0) {
                    rowData.Add(new Data27() {
                        Number = award27
                    });
                }

                var guestList = new List<PlayerModel>();

                if (!GameData.Shared.IsMatch()) {
                    var playerList = new List<Data>();
                    var offScoreList = new List<Data>();

                    foreach(object item in json.List("list")) {
                        var dict = item as Dictionary<string, object>;
                        if (dict == null) {
                            continue;
                        }

                        var model = dict.ToObject<PlayerModel>();

                        if (model.takecoin > 0) {
                            var data = new PlayerRowData() {
                                TakeCoin = model.takecoin,
                                Nick = model.name,
                                Score = model.bankroll - model.takecoin,
                                HasSeat = model.seat >= 0,
                                Uid = model.uid
                            };
                            playerList.Add(data);
                        }

                        if (model.seat < 0) {
                            guestList.Add(model);
                        }

                        var list = dict.List("off_scores");
                        foreach(var o in list) {
                            var dd = o as Dictionary<string, object>;
                            offScoreList.Add(new PlayerRowData() {
                                TakeCoin = dd.Int("takecoin"),
                                Nick = model.name,
                                Score = dd.Int("bankroll") - dd.Int("takecoin"),
                                HasSeat = false
                            });
                        }
                    }

                    playerList.Sort((a, b) => {
                        var aa = a as PlayerRowData;
                        var bb = b as PlayerRowData;

                        if (aa.HasSeat != bb.HasSeat) {
                            return bb.HasSeat ? 1 : -1;
                        }

                        return bb.Score - aa.Score;
                    });

                    // 离开座位且排在第一位的显示已离桌标志
                    var index = playerList.FindIndex((dt) => {
                        var data = dt as PlayerRowData;
                        return !data.HasSeat; 
                    });

                    if (index >= 0) {
                        playerList.Insert(index, new LeaveIconData() {
                            Label = "已离桌"
                        });
                    }

                    rowData.AddRange(playerList);

                    if (offScoreList.Count > 0) {
                        rowData.Add(new LeaveIconData(){
                            Label = "已下分"
                        });
                        offScoreList.Sort((a, b) => {
                            var aa = a as PlayerRowData;
                            var bb = b as PlayerRowData;
                            return bb.Score - aa.Score;
                        });
                        rowData.AddRange(offScoreList);
                    }
                } else {
                    var rankList = new List<Data>();

                    foreach(object item in json.List("list")) {
                        var dict = item as Dictionary<string, object>;
                        if (dict == null) {
                            continue;
                        }

                        var model = dict.ToObject<PlayerModel>(); 
                    
                        if (model.seat < 0) {
                            guestList.Add(model);
                            continue;
                        }
                        
                        rankList.Add(new RankRowData() {
                            Score = model.bankroll,
                            Nick = model.name,
                            Uid = model.uid,
                            // SNG未开赛都是0，强制转为1
                            Rank = model.match_rank == 0 ? 1 : model.match_rank
                        });
                    }

                    rankList.Sort((a , b) => {
                        var aa = a as RankRowData;
                        var bb = b as RankRowData;
                        return aa.Rank - bb.Rank;
                    });

                    rowData.AddRange(rankList);
                }
                
                rowData.Add(new GuestHeadData() {
                    Number = guestList.Count
                });

                guestList.Sort((a, b) => {
                    var bin = b.in_room ? 1 : 0;
                    var ain = a.in_room ? 1 : 0;
                    return bin - ain;
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


