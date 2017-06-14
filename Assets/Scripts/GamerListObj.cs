using MaterialUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamerListObj : MonoBehaviour {

    public Text Name;

    public Text State;

    public VectorImage EnterLimitImg;

    public VectorImage SeatLimitImg;

    public VectorImage TalkLimitImg;

    public GameObject GamerAvatar;

    string uid;
    bool enterLimit;
    bool seatLimit;
    bool talkLimit;
    Color unlimitCol = new Color(138 / 255f, 138 / 255f, 138 / 255f);

    public void Init(Dictionary<string, object> gamerMes) 
    {
        uid = gamerMes.String("uid");
        enterLimit = gamerMes.Int("enter_limit") == 1;
        seatLimit = gamerMes.Int("seat_limit") == 1;
        talkLimit = gamerMes.Int("talk_limit") == 1;

        Name.text = gamerMes.String("name");

        if (gamerMes.Bool("in_room"))
        {
            if (gamerMes.Int("seat") > -1)
            {
                State.text = "游戏中";
            }
            else 
            {
                State.text = "旁观中";
            }
        }

        EnterLimitImg.color = enterLimit ? Color.white : unlimitCol;
        SeatLimitImg.color = seatLimit ? Color.white : unlimitCol;
        TalkLimitImg.color = talkLimit ? Color.white : unlimitCol;

        GamerAvatar.GetComponent<Avatar>().SetImage(gamerMes.String("avatar"));
    }


    public void OnGamerOptionClick()
    {
        var go = (GameObject)Instantiate(Resources.Load("Prefab/GamerOption"));
        go.GetComponent<DOPopup>().Show();
        go.GetComponent<GamerOption>().Init(enterLimit, seatLimit, talkLimit, uid, true);
    }
}
