using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using MaterialUI;

[RequireComponent(typeof(DOPopup))]
public class UserDetail : MonoBehaviour {
	public RawImage Avatar;
	public Text Name;
	public Text Coins;
	public Text Hands;
	public Text ShowHand;
	public Text Join;
	public Text JoinWin; 
	public Text Aggressive;
	public Text PreRaise;
	public Text ThreeBet;
	public Text CBet;
    public GameObject ButtonTeam;
    public GameObject EmoticonsTeam;
    public GameObject R1;
    public GameObject R2;
    public Button[] EmoticonButtons;
    public Button StandUpButton;
    public Text[] EmoticonPrice;

    string Uid;
    bool enterLimit;
    bool seatLimit;
    bool talkLimit;

    public void Init(string Uid)
    {
        this.Uid = Uid;

        if (GameData.Shared.Owner && Uid != GameData.Shared.Uid)
            ButtonTeam.SetActive(true);

        if (Uid == GameData.Shared.Uid || GameData.Shared.MySeat == -1 || GameData.Shared.FindPlayerIndex(Uid) == -1 || GameData.Shared.emoticonClose)
        {
            EmoticonsTeam.SetActive(false);
            GetComponent<VerticalLayoutGroup>().padding.bottom = 40;
        }


        foreach (var button in EmoticonButtons)
        {
            button.onClick.AddListener(delegate()
            {
                for (int i = 0; i < EmoticonButtons.Length; i++)
                {
                    if (button == EmoticonButtons[i])
                    {
                        OnEmoticonClick(i + 1);
                        break;
                    }
                }
            });
        }

        RequestById(Uid);
    }

	
    void RequestById(string id) {
		var d = new Dictionary<string, object>(){
			{"uid", id}
		};

        Connect.Shared.Emit(new Dictionary<string, object>() {
			{"f", "gamerdetail"},
			{"args", d}
		}, (json) =>
        {
            if (gameObject == null)
            {
                return;
            }

            var data  = json.Dict("ret");
			var profile = data.Dict("profile").ToObject<ProfileModel>();
			var achieve = data.Dict("achieve").ToObject<AchieveModel>();

            Name.text = profile.name;
            StartCoroutine(_.LoadImage(profile.avatar, (texture) => {
                Avatar.texture = _.Circular(texture);
            }));

            // 金币数
            Coins.text = _.Num2CnDigit<int>(int.Parse(achieve.coins));
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

            for (int i = 0; i < emotion.Count; i++)
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
        var go = (GameObject)Instantiate(Resources.Load("Prefab/GamerOption"));
        go.GetComponent<DOPopup>().Show();
        go.GetComponent<GamerOption>().Init(enterLimit, seatLimit, talkLimit, Uid);
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

        gameObject.GetComponent<DOPopup>().Close();
    }

    private void setStandUpButton(bool interactable)
    {
        StandUpButton.interactable = interactable;

        var image = StandUpButton.GetComponent<ProceduralImage>();
        var text = StandUpButton.transform.Find("Text").GetComponent<Text>();
        var icon = StandUpButton.transform.FindChild("Image").GetComponent<VectorImage>();
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
}
