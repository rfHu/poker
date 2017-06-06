using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamerOption : MonoBehaviour {

    public Toggle EnterToggle;
    public Toggle SeatToggle;
    public Toggle TalkToggle;

    bool enterLimit;
    bool seatLimit;
    bool talkLimit;
    string uid;
    bool fromList;

    public void Init(bool enterLimit, bool seatLimit, bool talkLimit, string uid,bool fromList = false) 
    {
        this.enterLimit = EnterToggle.isOn = enterLimit;
        this.seatLimit = SeatToggle.isOn = seatLimit;
        this.talkLimit = TalkToggle.isOn = talkLimit;

        this.uid = uid;
        this.fromList = fromList; 
    }

    public void Enter() 
    {
        SendRequest(enterLimit, EnterToggle);
        SendRequest(seatLimit, SeatToggle);
        SendRequest(talkLimit, TalkToggle);

        GetComponent<DOPopup>().Close();
    }

    private void SendRequest(bool old, Toggle toggle)
    {
        if (old != toggle.isOn)
        {
            var data = new Dictionary<string, object>(){
			    {"uid", uid}
		    };

            if (toggle.isOn == false)
            {
                data.Add("cancel", 1);
            }

            Connect.Shared.Emit(new Dictionary<string, object>() {
			    {"f", toggle.gameObject.name.ToLower()},
			    {"args", data}
		    });
        }
    }

    public void Back() 
    {
        GetComponent<DOPopup>().Hide();

        if (fromList) { }
        else 
        {
            GetComponent<DOPopup>().Hide();

        }
    }
}
