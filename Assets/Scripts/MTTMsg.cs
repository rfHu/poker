using BestHTTP.JSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MTTMsg : MonoBehaviour {

    //标题内信息
    public Text MatchName;
    public Text PlayerNum;

    //P1页面
    public Text EntryFee;
    public Text LvUpTime;
    public Text InitalScore;
    public Text TableNum;

    //P4页面
    public GameObject RoomMsgPre;

    public Toggle[] Toggles;

    private Color SelectCol = new Color(33 / 255, 41 / 255, 50 / 255);
    private Color UnSelectCol = new Color(1, 1, 1, 0.6f);

    void Awake()
    {
        foreach (var item in Toggles)
        {
            item.onValueChanged.AddListener((isOn) => 
            {
                if (isOn)
                {
                    item.transform.Find("Label").GetComponent<Text>().color = SelectCol;
                }
                else 
                {
                    item.transform.Find("Label").GetComponent<Text>().color = UnSelectCol;
                }
            });
        }

        Toggles[3].onValueChanged.AddListener((isOn) => 
        {
            if (!isOn)
                return;
            

            HTTP.Post("/match-rooms", new Dictionary<string, object> {
                {"match_id", GameData.MatchData.ID },
            }, (data) =>
                {
                    var roomsMsg = Json.Decode(data) as List<object>;
                    foreach (var item in roomsMsg)
                    {
                        var msg = item as Dictionary<string, object>;

                        GameObject go = Instantiate(RoomMsgPre);
                        go.transform.GetChild(0).GetComponentInChildren<Text>().text = msg.Int("num").ToString();
                        go.transform.GetChild(1).GetComponentInChildren<Text>().text = msg.Int("gamers_count").ToString();
                        go.transform.GetChild(2).GetComponentInChildren<Text>().text = msg.Int("min") + "/" + msg.Int("max");
                    }
            });
        });
    }

    public void Init() 
    {
        MatchName.text = GameData.Shared.RoomName;
        PlayerNum.text = GameData.Shared.Players.Count + "/" + GameData.Shared.PlayerCount;

        var matchData = GameData.MatchData.Data;
        EntryFee.text = matchData[0] + "+" + matchData[2];
        LvUpTime.text = matchData[3] + "分钟";
        InitalScore.text = matchData[1].ToString();


    }
}
