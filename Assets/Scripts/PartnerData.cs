using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartnerData : MonoBehaviour {

    public Transform[] Partners;

    public Text Text_RecentRoomCount;
    public Text Text_RecentRoomProfit;
    public Text Text_TotalRoomProfit;

    public void Init(Dictionary<string, object> data)
    {

        var users = data.List("users");
        for (int i = 0; i < users.Count; i++)
        {
            var user = users[i] as Dictionary<string, object>;
            var userGo = Partners[i];
            userGo.GetChild(1).GetComponent<Text>().text = user.String("name");
            userGo.GetComponentInChildren<Avatar>().SetImage(user.String("avatar"));
            userGo.GetChild(2).GetComponent<Text>().text = user.String("total_count");
        }

        Text_RecentRoomCount.text = "近10个牌局两人同桌 <color=#18ffff>" + data.Int("recent_room_count") + "</color> 局";
        Text_RecentRoomProfit.text = "近10个同桌牌局盈利率为： <color=#ffca28>" + data.Int("recent_room_profit") * 100 + "%</color>";
        Text_TotalRoomProfit.text = "所有同桌牌局总盈利率为： <color=#ffca28>" + data.Int("total_room_profit") * 100 + "%</color>";
    }
}
