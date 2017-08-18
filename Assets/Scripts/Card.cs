using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DarkTonic.MasterAudio;
using MaterialUI;
using UniRx;

public class Card : MonoBehaviour {
	public Sprite Face;
	public Sprite CardBack;
    public VectorImage NumberPic;
    public VectorImage SuitPic;
    public Image FigurePic;
    public Sprite[] Figures;
	private int _index = -1;

	public AnimationCurve scaleCurve;

	public static float TurnCardDuration = 0.3f;

	private bool hasReShow = false;

	void Awake() {

        RxSubjects.CardStyleChange.Subscribe((num) => {
            SetCardFace(_index, GetComponent<Image>());
        }).AddTo(this);

		var img = GetComponent<Image>();
		img.sprite = CardBack;

		scaleCurve = new AnimationCurve();
		scaleCurve.AddKey(0, 1);
		scaleCurve.AddKey(0.5f, 0);
		scaleCurve.AddKey(1, 1);
	}

	private void show(int index, bool anim = false, Action complete = null) {	
		var diffIndex = (_index != index);
		_index = index;
		gameObject.SetActive(true);

		var image = GetComponent<Image>();
		image.enabled = true;
		
		if (anim && diffIndex) { 
			StartCoroutine(flipCard(index, complete));
		} else {
            SetCardFace(index, image);

			var rect = image.GetComponent<RectTransform>();
			rect.localScale = new Vector2(1, 1);
		}
	}

    private void SetCardFace(int index, Image image)
    {
        //消除原图
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        image.sprite = Face;
        int NumSub = (index + 1) % 13;
        NumSub = NumSub == 0 ? 13 : NumSub;
        int SuitSub = index / 13;

        NumberPic.gameObject.SetActive(true);
        NumberPic.vectorImageData = CustomIconHelper.GetIcon("poker_" + NumSub).vectorImageData;

        if (GameSetting.cardColor == 0 && NumSub > 10)
        {
            FigurePic.gameObject.SetActive(true);
            int figureSub = NumSub + SuitSub * 3 - 11;
            FigurePic.sprite = Figures[figureSub];
        }
        else 
        {
            SuitPic.gameObject.SetActive(true);
            SuitPic.vectorImageData = CustomIconHelper.GetIcon("pattern_" + SuitSub).vectorImageData;
        }

        //设置颜色
        if (GameSetting.cardColor == 0)
        {
            image.color = Color.white;
            NumberPic.color = SuitPic.color = SuitSub % 2 == 0 ? Color.black : Color.red;
        }
        else 
        {
            string[] colors = new string[4] { "#000000", "#ff0000", "#00a221", "#0059ff" };
            if (GameSetting.cardColor == 1)
            {
                image.color = Color.white;
                NumberPic.color = SuitPic.color = _.HexColor(colors[SuitSub]);
            }
            else if (GameSetting.cardColor == 2) 
            {
                image.color = _.HexColor(colors[SuitSub]);
                NumberPic.color = SuitPic.color = Color.white;
            }
        }
    }

	public void SetSize(Vector2 size) {
		var rectTrans = GetComponent<RectTransform>();
		rectTrans.sizeDelta = size;
	}

	public void Darken() {
		GetComponent<Image>().color = new Color(150 / 255f ,150 / 255f, 150 / 255f, 1);
	}

	public void ReColor() {
		GetComponent<Image>().color = new Color(1, 1, 1, 1);
	}

	public void Show(int index, bool anim = false, Action complete = null) {
		if (index <= 0) {
			Turnback();
			return ;
		}

		ReColor();

		var realIndex = Card.CardIndex(index);
		show(realIndex, anim, complete);
	}

	public void ReShow() {
		if (hasReShow) {
			return ;
		}

		hasReShow = true;
		StartCoroutine(flipCard(_index, null));
		G.PlaySound("fapai_1");
	}

	void OnDisable() {
		hasReShow = false;
	}

	public void ShowWithSound(int index, bool anim = false) {
		if (index == 0) {
			return ;
		}

		Show(index, anim);
		G.PlaySound("fapai_1");
	}

	IEnumerator flipCard(int index, Action complete = null) {
		float time = 0f;
		var image = GetComponent<Image>();
		var rectTrans = GetComponent<RectTransform>();
		
		while(time < 1f) {
			time = Mathf.Min(time + Time.deltaTime / TurnCardDuration, 1);
			float scale = scaleCurve.Evaluate(time);

			Vector2 vector = rectTrans.localScale;
			vector.x = scale;
			rectTrans.localScale = vector;

			if (time >= 0.5) {
                SetCardFace(index, image);
			}

			yield return new WaitForFixedUpdate();
		}

		rectTrans.localScale = new Vector2(1, 1);

		if (complete != null) {
			complete();
		}
	}

	public void Turnback() {
		_index = -1;
		GetComponent<Image>().sprite = CardBack;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
		ReColor();
	}

	public void Hide() {
		GetComponent<Image>().enabled = false;
	}

	public static int CardIndex(int number) {
		var pairs = Card.CardValues(number);
		int index;

		// 服务器数值为2~14
		if (pairs[1] ==  14) {
			index = 0;
		} else {
			index = pairs[1] - 1;
		}

		index = index + (4 - pairs[0]) * 13;

		return index;
	}

	public static int[] CardValues(int number) {
		var a = number >> 4;
		var b = number & 0x0f;

		// 第一个花色、第二个数值
		return new int[]{a, b};
	}

	static public string GetCardDesc(int val) {
		var value = val >> 20;

		var map = new Dictionary<int, string> {
			{1, "高牌"},
			{2, "一对"},
			{3, "两对"},
			{4, "三条"},
			{5, "顺子"},
			{6, "同花"},
			{7, "葫芦"},
			{8, "四条"},
			{9, "同花顺"},
			{10, "皇家同花顺"},
		};

		if (!map.ContainsKey(value)) {
			return "";
		}

		return map[value];
	}
}
