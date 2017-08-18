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
	[SerializeField]private Image cardBg;
	[SerializeField]private Transform cardContent;

	public AnimationCurve scaleCurve;

	public static float TurnCardDuration = 0.3f;

	private bool hasReShow = false;

	void Awake() {
        RxSubjects.CardStyleChange.Subscribe((num) => {
            setCardFace(_index);
        }).AddTo(this);

		cardBg.sprite = CardBack;

		scaleCurve = new AnimationCurve();
		scaleCurve.AddKey(0, 1);
		scaleCurve.AddKey(0.5f, 0);
		scaleCurve.AddKey(1, 1);
	}

	private void show(int index, bool anim = false, Action complete = null) {	
		var diffIndex = (_index != index);
		_index = index;
		gameObject.SetActive(true);

		cardBg.enabled = true;
		
		if (anim && diffIndex) { 
			StartCoroutine(flipCard(index, complete));
		} else {
            setCardFace(index);

			var rect = cardBg.GetComponent<RectTransform>();
			rect.localScale = new Vector2(1, 1);
		}
	}

	private void hideCardContents() {
		//消除原图
        foreach (Transform child in cardContent)
        {
            child.gameObject.SetActive(false);
        }
	}

    private void setCardFace(int index)
    {
		hideCardContents();

       	cardBg.sprite = Face;
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
            cardBg.color = Color.white;
            NumberPic.color = SuitPic.color = SuitSub % 2 == 0 ? Color.black : Color.red;
        }
        else 
        {
            string[] colors = new string[4] { "#000000", "#ff0000", "#00a221", "#0059ff" };
            if (GameSetting.cardColor == 1)
            {
                cardBg.color = Color.white;
                NumberPic.color = SuitPic.color = _.HexColor(colors[SuitSub]);
            }
            else if (GameSetting.cardColor == 2) 
            {
                cardBg.color = _.HexColor(colors[SuitSub]);
                NumberPic.color = SuitPic.color = Color.white;
            }
        }
    }

	public void Darken() {
		cardBg.color = new Color(150 / 255f ,150 / 255f, 150 / 255f, 1);
	}

	public void ReColor() {
		cardBg.color = new Color(1, 1, 1, 1);
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
		var rectTrans = cardContent.parent.GetComponent<RectTransform>();
		
		while(time < 1f) {
			time = Mathf.Min(time + Time.deltaTime / TurnCardDuration, 1);
			float scale = scaleCurve.Evaluate(time);

			Vector2 vector = rectTrans.localScale;
			vector.x = scale;
			rectTrans.localScale = vector;

			if (time >= 0.5) {
                setCardFace(index);
			}

			yield return new WaitForFixedUpdate();
		}

		rectTrans.localScale = new Vector2(1, 1);

		if (complete != null) {
			complete();
		}
	}

	public void Turnback(bool hide = false) {
		_index = -1;
		cardBg.sprite = CardBack;
		hideCardContents();
		ReColor();

		if (hide) {
			cardBg.enabled = false;
		}
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

	static public Vector2 Size = new Vector2(126, 172); 

	static private GameObject prefab; 

	static public Card LoadCard(Transform parent) {
		if (prefab == null) {
			prefab = Resources.Load<GameObject>("Prefab/Card");
		}

		var rect = parent.GetComponent<RectTransform>().rect;
		var card = GameObject.Instantiate(prefab).GetComponent<Card>();
		var scale = rect.width / Size.x;
		var cardTransform = card.GetComponent<RectTransform>();

		cardTransform.localScale = new Vector2(scale, scale);
		cardTransform.SetParent(parent);
		parent.SetAsFirstSibling();
		cardTransform.anchoredPosition = new Vector2(0, 0);

		return card;
	}
}
