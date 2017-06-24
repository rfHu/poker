using UnityEngine;
using System.Collections;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using System.Collections.Generic;
using SimpleJSON;

namespace ScorePage {

    public class ScoreCtrl : MonoBehaviour, IEnhancedScrollerDelegate
    {
        public EnhancedScroller scroller;

        public EnhancedScrollerCellView insuranceRow;
        // public EnhancedScrollerCellView rowCellViewPrefab;
        // public EnhancedScrollerCellView footerCellViewPrefab;

        /// <summary>
        /// The base path to the resources folder where sprites are located
        /// </summary>
        // public string resourcePath;

        private List<Data> datas = new List<Data>();

        void OnSpawned()
        {
            // tell the scroller that this script will be its delegate
            scroller.Delegate = this;
            requestData();
        }

        private void requestData() {
            Connect.Shared.Emit(new Dictionary<string, object>(){
                {"f", "gamerlist"}
            }, (json) =>
            {
                datas.Clear();

                // var N = new JSONObject(json);
            });
        }

        /// <summary>
        /// Populates the data with a lot of records
        /// </summary>
        private void LoadData()
        {

            scroller.ReloadData();
        }

        #region EnhancedScroller Handlers

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return 100;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return 100;

            if (datas[dataIndex] is InsuranceRowData) {
                return 96f;
            }

            // if (_data[dataIndex] is HeaderData)
            // {
            //     // header views
            //     return 96f;
            // }
            // else if (_data[dataIndex] is RowData)
            // {
            //     // row views
            //     return 100f;
            // }
            // else
            // {
            //     // footer views
            //     return 90f;
            // }
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            return scroller.GetCellView(insuranceRow) as InsuranceRow; 

            // CellView cellView;

            // // determin what cell view to get based on the type of the data row

            // if (_data[dataIndex] is HeaderData)
            // {
            //     // get a header cell prefab from the scroller, recycling old cells if possible
            //     cellView = scroller.GetCellView(headerCellViewPrefab) as CellViewHeader;

            //     // optional for clarity: set the cell's name to something to indicate this is a header row
            //     cellView.name = "[Header] " + (_data[dataIndex] as HeaderData).category;
            // }
            // else if (_data[dataIndex] is RowData)
            // {
            //     // get a row cell prefab from the scroller, recycling old cells if possible
            //     cellView = scroller.GetCellView(rowCellViewPrefab) as CellViewRow;

            //     // optional for clarity: set the cell's name to something to indicate this is a row
            //     cellView.name = "[Row] " + (_data[dataIndex] as RowData).userName;
            // }
            // else
            // {
            //     // get a footer cell prefab from the scroller, recycling old cells if possible
            //     cellView = scroller.GetCellView(footerCellViewPrefab) as CellViewFooter;

            //     // optional for clarity: set the cell's name to something to indicate this is a footer row
            //     cellView.name = "[Footer]";
            // }

            // // set the cell view's data. We can do this because we declared a single SetData function
            // // in the CellView base class, saving us from having to call this for each cell type
            // cellView.SetData(_data[dataIndex]);

            // // return the cellView to the scroller
            // return cellView;
        }

        #endregion
    }
}











// using UnityEngine.UI;
// using System.Collections.Generic;
// using UnityEngine;

// public class ScoreCtrl : MonoBehaviour {
// 	public GameObject viewport;

//     public GameObject InGameViewport;

//     public GameObject OutGameViewport;

// 	public Text Hands;

//     public Text Pot;

//     public Text Time;

//     public Text Buy;

// 	public GameObject PlayerScore;

// 	public GameObject GuestHeader;

// 	public GameObject GridLayout;

// 	public GameObject Guest;

//     public GameObject Insurance;

//     public Text InsuranceData;

// 	private Color offlineColor = new Color(1, 1, 1, 0.6f);

// 	void OnSpawned() {
// 		return ;

// 		Connect.Shared.Emit(new Dictionary<string, object>(){
//         	{"f", "gamerlist"}
//         }, (json) =>
//         {
// 			var ret = json.Dict("ret");
//             // var insurance = ret.Dict("insurance");
// 			Hands.text = ret.Int("handid").ToString();
//             Pot.text = ret.Int("avg_pot").ToString();
//             Time.text = ret.Int("hand_time").ToString() + "s";
//             Buy.text = ret.Int("avg_buy").ToString();

// 			var list = ret.List("list");
// 			var guestList = new List<Dictionary<string, object>>();
// 			var playerList = new List<Dictionary<string, object>>();

// 			foreach(object item in list) {
// 				var dict = item as Dictionary<string, object>;
// 				if (dict == null) {
// 					continue;
// 				}

// 				if (dict.Int("takecoin") > 0) {
// 					playerList.Add(dict);
//                     if (dict.Int("seat") < 0)
//                     {
//                         if (dict.Bool("in_room"))
//                             guestList.Insert(0, dict);
//                         else
//                             guestList.Add(dict);
//                     }
// 				} else {
//                     if (dict.Bool("in_room"))
//                         guestList.Insert(0, dict);
//                     else
//                         guestList.Add(dict);
// 				}
// 			}

// 			playerList.Sort((a, b) => {
// 				var ar = a.Int("bankroll") - a.Int("takecoin");
// 				var br = b.Int("bankroll") - b.Int("takecoin");

// 				return br - ar;
// 			});

// 			foreach(Dictionary<string, object> player in playerList) {
// 				GameObject  entry = Instantiate(PlayerScore);
// 				entry.SetActive(true);
// 				var all = player.Int("takecoin");

//                 Text name = entry.transform.Find("Name").GetComponent<Text>();
//                 Text total = entry.transform.Find("Total").GetComponent<Text>();
//                 Text score = entry.transform.Find("Score").GetComponent<Text>();

// 				name.text = player.String("name");
// 				total.text = all.ToString();

// 				var profit = player.Int("bankroll") - all; 

//                 if (player.Int("seat") < 0)
//                 {
//                     name.color = offlineColor;
//                     total.color = offlineColor;
//                     score.text = _.Number2Text(profit);
//                     score.color = offlineColor;
//                     entry.transform.SetParent(OutGameViewport.transform, false);
//                 }
//                 else 
//                 {
// 				    score.text = _.Number2Text(profit);
// 				    score.color = _.GetTextColor(profit);
// 				    entry.transform.SetParent(InGameViewport.transform, false);
//                 }
//         	}

//             if (OutGameViewport.transform.childCount < 2)
//             {
//                 OutGameViewport.SetActive(false);
//             }

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
