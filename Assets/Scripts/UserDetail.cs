using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using MaterialUI;
using UniRx;

[RequireComponent(typeof(DOPopup))]
public class UserDetail : MonoBehaviour {
	public RawImage Avatar;
	public Text Name;
    public Text RemarkText;
	public Text Hands;
	public Text ShowHand;
	public Text Join;
	public Text JoinWin; 
	public Text Aggressive;
	public Text PreRaise;
	public Text ThreeBet;
	public Text CBet;
    public GameObject GameOptionBtn;
    public GameObject EmoticonsTeam;
    public GameObject R1;
    public GameObject R2;
    public Button[] EmoticonButtons;
    public Button StandUpButton;
    public Text[] EmoticonPrice;
    public GameObject UserRemark;

    string Uid;
    private string remark;
    bool enterLimit;
    bool seatLimit;
    bool talkLimit;

    void Awake() 
    {
        for (var i = 0; i < EmoticonButtons.Length; i++)
        {
            var local = i;
            var button = EmoticonButtons[i];

            button.OnClickAsObservable().Subscribe((_) =>
            {
                OnEmoticonClick(local + 1);
            }).AddTo(this);
        }
    }

    public void Init(string Uid)
    {
        this.Uid = Uid;

        if (GameData.Shared.Owner)
        {
            GameOptionBtn.SetActive(true);
        }
        else 
        {
            GameOptionBtn.SetActive(false);
        }

        var parent = GameOptionBtn.transform.parent.gameObject;

        if (Uid == GameData.Shared.Uid) {
            parent.SetActive(false);
        } else {
            parent.SetActive(true);
        }

        if (Uid == GameData.Shared.Uid || GameData.Shared.FindPlayerIndex(Uid) == -1 || GameData.Shared.emoticonClose)
        {
            EmoticonsTeam.SetActive(false);
            GetComponent<VerticalLayoutGroup>().padding.bottom = 40;
        } else {
            EmoticonsTeam.SetActive(true);
            GetComponent<VerticalLayoutGroup>().padding.bottom = 0;
        }

        RequestById(Uid);
        GetComponent<DOPopup>().Show();
    }

	
    void RequestById(string id) {
		var d = new Dictionary<string, object>(){
			{"uid", id}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "gamerdetail"},
			{"args", d}
		}, (data) =>
        {
            if (gameObject == null)
            {
                return;
            }

			var profile = data.Dict("profile").ToObject<ProfileModel>();
			var achieve = data.Dict("achieve").ToObject<AchieveModel>();

            Name.text = profile.name;
            Avatar.GetComponent<Avatar>().SetImage(profile.avatar);

            remark = data.String("remark");

            if (string.IsNullOrEmpty(remark) && GameData.Shared.Uid != Uid) {
                RemarkText.text = "玩家备注";
            } else {
                RemarkText.text = remark;
            }

            // 手数
            Hands.text = achieve.total_hand_count.ToString();
            // 入池率
            Join.text = _.PercentStr(achieve.entry_hand_percent);
            // 摊牌率
            ShowHand.text = _.PercentStr(achieve.showdown_hand_percent);
            // 入池胜率
            JoinWin.text = _.PercentStr(achieve.entry_win_hand_percent);
            // 激进度
            Aggressive.text = achieve.aggressiveness.ToString();
            // 翻前加注
            PreRaise.text = _.PercentStr(achieve.pfr_hand_percent);
            // 再次加注
            ThreeBet.text = _.PercentStr(achieve.t_bet_percent);
            // 持续下注
            CBet.text = _.PercentStr(achieve.c_bet_round_percent);

            // 动态表情
            var emotion = data.List("emoticon");

            for (int i = 0; i < EmoticonPrice.Length && i < emotion.Count; i++)
            {
                var dict = emotion[i] as Dictionary<string, object>;
                EmoticonPrice[i].text = "" + dict.Int("coin");
            }

            enterLimit = data.Int("enter_limit") == 1;
            seatLimit = data.Int("seat_limit") == 1;
            talkLimit = data.Int("talk_limit") == 1;
        });
	}

    public void OnGamerOptionClick() 
    {
        var transform = PoolMan.Spawn("GamerOption");
        transform.GetComponent<DOPopup>().Show();
        transform.GetComponent<GamerOption>().Init(enterLimit, seatLimit, talkLimit, Uid);
    }

    public void OnEmoticonClick(int Pid) 
    {

        var data = new Dictionary<string, object>(){
			        {"uid", Uid},
                    {"pid", Pid}
		        };

        Connect.Shared.Emit(new Dictionary<string, object>() {
			        {"f", "emoticon"},
			        {"args", data}
		        });

        GetComponent<DOPopup>().Close();
    }

    private void setStandUpButton(bool interactable)
    {
        StandUpButton.interactable = interactable;

        var image = StandUpButton.GetComponent<ProceduralImage>();
        var text = StandUpButton.transform.Find("Text").GetComponent<Text>();
        var icon = StandUpButton.transform.Find("Image").GetComponent<VectorImage>();
        Color color;

        if (interactable)
        {
            color = MaterialUI.MaterialColor.cyanA200;
        }
        else
        {
            color = MaterialUI.MaterialColor.grey400;
        }

        image.color = color;
        text.color = color;
        icon.color = color;
    }

    public void OnRemark() {
        var transform = PoolMan.Spawn("UserRemark");
        transform.GetComponent<UserRemark>().Show(Uid, remark);
    }
}
