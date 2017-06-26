using UnityEngine;
using System.Collections;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

namespace ScorePage {

    public class ScoreCtrl : MonoBehaviour, IEnhancedScrollerDelegate
    {
        public EnhancedScroller Scroller;

        public Text Hands;

        public Text Pot;

        public Text Time;

        public Text Buy;


        public EnhancedScrollerCellView InsurancePrefab;
        public EnhancedScrollerCellView PlayerPrefab;
        public EnhancedScrollerCellView GuestHeaderPrefab;
        public EnhancedScrollerCellView GuestPrefab;
        public EnhancedScrollerCellView LeaveIconPrefab;
        public EnhancedScrollerCellView Award27Prefab;

        private List<Data> rowData = new List<Data>();

        void Awake()
        {
        }

        void OnSpawned()
        {
            requestData();
        }

        void OnDespawned() {
            Scroller.Delegate = null;
        }

        private void requestData() {
            Connect.Shared.Emit(new Dictionary<string, object>(){
                {"f", "gamerlist"}
            }, (json) =>
            {
                rowData.Clear();

                Hands.text = json.String("handid");
                Pot.text = json.String("avg_pot");
                Time.text = json.Int("hand_time").ToString() + "s";
                Buy.text = json.String("avg_buy");

                if (GameData.Shared.NeedInsurance) {
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

                var playerList = new List<Data>();
                var guestList = new List<PlayerModel>();

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
                            HasSeat = (model.seat >= 0)
                        };
                        playerList.Add(data);
                    }

                    if (model.seat < 0) {
                        guestList.Add(model);
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
                    playerList.Insert(index, new LeaveIconData());
                }

                rowData.AddRange(playerList);
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
                
                Scroller.Delegate = this;
                Scroller.ReloadData();
            });
        }

        #region EnhancedScroller Handlers

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return rowData.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            var data = rowData[dataIndex];

            if (data is InsuranceRowData || data is PlayerRowData || data is GuestHeadData || data is Data27) {
                return 60f;
            } else if (data is LeaveIconData) {
                return 44f;
            } 
            else {
                return 200f;
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var data = rowData[dataIndex];
            CellView cellView;

            if (data is InsuranceRowData) {
                cellView = scroller.GetCellView(InsurancePrefab) as InsuranceRow;
            } else if (data is PlayerRowData) {
                cellView = scroller.GetCellView(PlayerPrefab) as PlayerRow;
            } else if (data is GuestHeadData) {
                cellView = scroller.GetCellView(GuestHeaderPrefab) as GuestHeader;
            } else if (data is LeaveIconData) {
                cellView = scroller.GetCellView(LeaveIconPrefab) as LeaveIcon;
            } else if (data is Data27) {
                cellView = scroller.GetCellView(Award27Prefab) as Award27Row;
            }
            else  {
                cellView = scroller.GetCellView(GuestPrefab) as GuestRow;    
            } 

            if (cellView == null) {
                return null;
            }
        
            cellView.SetData(data);
            return cellView; 
        }

        #endregion
    }
}


