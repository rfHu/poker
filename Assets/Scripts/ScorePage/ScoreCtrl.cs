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

        private List<Data> datas = new List<Data>();

        void OnSpawned()
        {
            // tell the scroller that this script will be its delegate
            Scroller.Delegate = this;
            requestData();
        }

        private void requestData() {
            Connect.Shared.Emit(new Dictionary<string, object>(){
                {"f", "gamerlist"}
            }, (json) =>
            {
                datas.Clear();

                Hands.text = json.String("handid");
                Pot.text = json.String("avg_pot");
                Time.text = json.String("hand_time") + "s";
                Buy.text = json.String("avg_buy");

                if (GameData.Shared.NeedInsurance) {
                    datas.Add(
                        new InsuranceRowData() {number = json.Dict("insurance").Int("Pay")}
                    );
                }

                var inPlayerList = new List<PlayerModel>();
                var outPlayerList = new List<PlayerModel>();
                var guestList = new List<PlayerModel>();

                foreach(object item in json.List("list")) {
                    var dict = item as Dictionary<string, object>;
                    if (dict == null) {
                        continue;
                    }

                    var model = dict.ToObject<PlayerModel>();

                    if (model.takecoin > 0) {
                        if (model.seat >= 0) {
                            inPlayerList.Add(model);
                        } else {
                            outPlayerList.Add(model);
                        }
                    }

                    if (model.seat < 0) {
                        guestList.Add(model);
                    }
                }

                inPlayerList.Sort((a, b) => {
                    var bp = b.bankroll - b.takecoin;
                    var ap = a.bankroll - a.takecoin;
                    return bp - ap;
                });

                outPlayerList.Sort((a, b) => {
                    var bp = b.bankroll - b.takecoin;
                    var ap = a.bankroll - a.takecoin;
                    return bp - ap;
                });

                guestList.Sort((a, b) => {
                    var bin = b.in_room ? 1 : 0;
                    var ain = a.in_room ? 1 : 0;
                    return bin - ain;
                });
                
                LoadData();
            });
        }
     
        private void LoadData()
        {

            Scroller.ReloadData();
        }

        #region EnhancedScroller Handlers

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return datas.Count;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            var data = datas[dataIndex];

            if (data is InsuranceRowData || data is PlayerRowData) {
                return 96f;
            } else {
                return 0f;
            }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var data = datas[dataIndex];
            CellView cellView;

            if (data is InsuranceRowData) {
                cellView = scroller.GetCellView(InsurancePrefab) as InsuranceRow;
            } else if (data is PlayerRowData) {
                cellView = scroller.GetCellView(PlayerPrefab) as PlayerRow;
            } 
            else {
                cellView = scroller.GetCellView(InsurancePrefab) as InsuranceRow;    
            }
        
            cellView.SetData(data);
            return cellView; 
        }

        #endregion
    }
}

// using UnityEngine.UI;
// using System.Collections.Generic;
// using UnityEngine;

// public class ScoreCtrl : MonoBehaviour {
// 	public GameObject viewport;

// 	public GameObject PlayerScore;

// 	public GameObject GuestHeader;

// 	public GameObject GridLayout;

// 	public GameObject Guest;


// 	private Color offlineColor = new Color(1, 1, 1, 0.6f);

// 	void OnSpawned() {
// 		return ;

// 		Connect.Shared.Emit(new Dictionary<string, object>(){
//         	{"f", "gamerlist"}
//         }, (json) =>
//         {

// 			foreach(Dictionary<string, object> player in playerList) {
// 				GameObject  entry = Instantiate(PlayerScore);
// 				entry.SetActive(true);

//         	}


// 			// 游客
// 			var header = (GameObject)Instantiate(GuestHeader);
// 			header.SetActive(true);
// 			header.transform.Find("Text").GetComponent<Text>().text = string.Format("游客（{0}）", guestList.Count);
//         	header.transform.SetParent(viewport.transform, false);

// 			if (guestList.Count < 1) {
// 				return ;
// 			}

// 			GameObject grid = Instantiate(GridLayout);
// 			grid.SetActive(true);
// 			grid.transform.SetParent(viewport.transform, false);

// 			foreach(Dictionary<string, object> guest in guestList) {
// 				var guestObj = Instantiate(Guest);
// 				guestObj.SetActive(true);

// 				Avatar avatar = guestObj.transform.Find("Avatar").GetComponent<Avatar>();
// 				avatar.Uid = guest.String("uid");
// 				avatar.SetImage(guest.String("avatar"));

//                 Text name = guestObj.transform.Find("Text").GetComponent<Text>();
// 				name.text = guest.String("name");
// 				guestObj.transform.SetParent(grid.transform, false);

//                 if (!guest.Bool("in_room"))
//                 {
//                     guestObj.GetComponent<CanvasGroup>().alpha = 0.6f;
//                 }
// 			} 
//         });
// 	}	
// }
