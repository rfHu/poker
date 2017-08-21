using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using DarkTonic.MasterAudio;
using MaterialUI;
using UniRx;
using Unity.Linq;

public class Card : MonoBehaviour {
	public Sprite Face;
	public Sprite CardBack;
    public VectorImage NumberPic;
    public VectorImage SuitPic;
    public Image FigurePic;
    public Sprite[] Figures;
	private int _index = -1;
	[SerializeField]private Image cardBg;
	[SerializeField]private GameObject darkenCover;

	private RectTransform flipTransform {
		get {
			return cardBg.GetComponent<RectTransform>();
		}
	}

	private GameObject cardContent {
		get {
			return NumberPic.transform.parent.gameObject;
		}
	}

	public AnimationCurve scaleCurve;

	public static float TurnCardDuration = 0.3f;

	private bool hasReShow = false;

	private int cardColor {
		get {
			return GameSetting.CardColor.Value;
		}
	}

	void Awake() {
        GameSetting.CardColor.Subscribe((num) => {
			if (_index < 0) {
				return ;
			}
            setCardFace(_index, num);
        }).AddTo(this);

		cardBg.sprite = CardBack;

		scaleCurve = new AnimationCurve();
		scaleCurve.AddKey(0, 1);
		scaleCurve.AddKey(0.5f, 0);
		scaleCurve.AddKey(1, 1);
	}

	private void show(int index, bool anim = false, Action complete = null) {	
		gameObject.SetActive(true);
		ReColor();
		
		if (anim && _index != index) { 
			StartCoroutine(flipCard(index, complete));
		} else {
            setCardFace(index, cardColor);
			flipTransform.localScale = new Vector2(1, 1);
		}

		_index = index;
	}

    private void setCardFace(int index, int cardType)
    {
       	cardBg.sprite = Face;
        int NumSub = (index + 1) % 13;
        NumSub = NumSub == 0 ? 13 : NumSub;
        int SuitSub = index / 13;

        NumberPic.gameObject.SetActive(true);
        NumberPic.vectorImageData = CustomIconHelper.GetIcon("poker_" + NumSub).vectorImageData;

        if (cardType == 0 && NumSub > 10)
        {
            FigurePic.gameObject.SetActive(true);
			SuitPic.gameObject.SetActive(false);
            int figureSub = NumSub + SuitSub * 3 - 11;
            FigurePic.sprite = Figures[figureSub];
        }
        else 
        {
            SuitPic.gameObject.SetActive(true);
            FigurePic.gameObject.SetActive(false);
            SuitPic.vectorImageData = CustomIconHelper.GetIcon("pattern_" + SuitSub).vectorImageData;
        }

        //设置颜色
        if (cardType == 0)
        {
            cardBg.color = Color.white;
            NumberPic.color = SuitPic.color = SuitSub % 2 == 0 ? Color.black : _.HexColor("#d32f2f");
        }
        else 
        {
            string[] colors = new string[4] { "#000000", "#d32f2f", "#388e3c", "#1976d2" };
            if (cardType == 1)
            {
                cardBg.color = Color.white;
                NumberPic.color = SuitPic.color = _.HexColor(colors[SuitSub]);
            }
            else if (cardType == 2) 
            {
                cardBg.color = _.HexColor(colors[SuitSub]);
                NumberPic.color = SuitPic.color = Color.white;
            }
        }
    }

	public void Darken() {
		darkenCover.SetActive(true);
	}

	public void ReColor() {
		darkenCover.SetActive(false);
	}

	public void Show(int index, bool anim = false, Action complete = null) {
		if (index <= 0) {
			Turnback();
			return ;
		}

		var realIndex = Card.CardIndex(index);
		show(realIndex, anim, complete);
	}

    public void ShowInExplain(int index) 
    {
        show(index);
    }

	public void ReShow() {
		if (hasReShow) {
			return ;
		}

		hasReShow = true;
		StartCoroutine(flipCard(_index));
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
		var rectTrans = flipTransform;
		var hasSet = false;
		
		while(time < 1f) {
			time = Mathf.Min(time + Time.deltaTime / TurnCardDuration, 1);
			float scale = scaleCurve.Evaluate(time);

			Vector2 vector = rectTrans.localScale;
			vector.x = scale;
			rectTrans.localScale = vector;

			if (time >= 0.5 && !hasSet) {
                setCardFace(index, cardColor);
				hasSet = true;
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
		cardBg.color = Color.white;

		cardContent.SetChildrenActive(false);
		ReColor();

		if (hide) {
			gameObject.SetActive(false);
		} else {
			gameObject.SetActive(true);
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

		var rect = parent.GetComponent<RectTransform>().sizeDelta;

		var card = GameObject.Instantiate(prefab).GetComponent<Card>();
		var scale = rect.x / Size.x;
		var cardTransform = card.GetComponent<RectTransform>();

		cardTransform.SetParent(parent);
		cardTransform.SetAsFirstSibling();
		cardTransform.localScale = new Vector2(scale, scale);
		cardTransform.anchoredPosition = new Vector2(0, 0);

		return card;
	}
}
