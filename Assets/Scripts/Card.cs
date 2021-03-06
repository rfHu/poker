﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using MaterialUI;
using UniRx;
using Unity.Linq;
using System.Linq;

public abstract class CardType {
	internal List<int> extractSome(List<int> maxFive) {
		var list = new List<int>();
		var dict = new Dictionary<int, List<int>>();

		// 群组	
		foreach(var value in maxFive) {
			var key = Card.CardValues(value)[1];

			if (dict.ContainsKey(key)) {
				dict[key].Add(Card.CardIndex(value));
			} else {
				dict[key] = new List<int>{
					Card.CardIndex(value)
				};
			}
		}
		
		foreach(var item in dict) {
			if (item.Value.Count > 1) {
				list.AddRange(item.Value);
			}
		}

		return list;
	}

	public string GetCardDesc(int val) {
		var value = val >> Digit; 

		if (!TypeMap.ContainsKey(value)) {
			return "";
		}

		return TypeMap[value];
	}

	public abstract int Digit {get;}

	public abstract Dictionary<int, string> TypeMap {get;}

	abstract public List<int> ExtractHighlightCards(List<int> maxFive, int maxFiveRank);
}

public class NormalCardType: CardType {
	public enum Type {
		High = 1,
		Pair,
		TwoPair,
		Three,
		Straight,
		Flush,
		FullHouse,
		Four,
		StraightFlush,
		RoyalFluash 
	};

	public override int Digit {
		get {
			return 20;
		}
	}

	public override Dictionary<int, string> TypeMap {
		get {
			return new Dictionary<int, string>() {
				{(int)Type.High, "高牌"},
				{(int)Type.Pair, "一对"},
				{(int)Type.TwoPair, "两对"},
				{(int)Type.Three, "三条"},
				{(int)Type.Straight, "顺子"},
				{(int)Type.Flush, "同花"},
				{(int)Type.FullHouse, "葫芦"},
				{(int)Type.Four, "四条"},
				{(int)Type.StraightFlush, "同花顺"},
				{(int)Type.RoyalFluash, "皇家同花顺"},
			};
		}
	} 

	public override List<int> ExtractHighlightCards(List<int> maxFive, int maxFiveRank) {
		if (maxFive.Count != 5) {
			return new List<int>();	
		}

		var type = (Type)(maxFiveRank >> Digit);

		switch(type) {
			case Type.High:
				return new List<int>();
			case Type.TwoPair:  case Type.Four: case Type.Three: case Type.Pair:
				return extractSome(maxFive);
			default:
				return maxFive.Select(index => Card.CardIndex(index)).ToList() ;
		}	
	}
}

public class KingThreeCardType: CardType {
	public enum Type {
		High = 1,
		Pair,
		Fluash,
		Straight,
		StraightFlush,
		Three	
	}

	public override Dictionary<int, string> TypeMap {
		get {
			return new Dictionary<int, string>() {
				{(int)Type.High, "高牌"},
				{(int)Type.Pair, "一对"},
				{(int)Type.Fluash, "同花"},
				{(int)Type.Straight, "顺子"},
				{(int)Type.StraightFlush, "同花顺"},
				{(int)Type.Three, "豹子"},
			};
		}
	}	

	public override int Digit {
		get {
			return 12;
		}
	}

	public override List<int> ExtractHighlightCards(List<int> maxFive, int maxFiveRank) {
		if (maxFive.Count != 3) {
			return new List<int>();	
		}

		var type = (Type)(maxFiveRank >> Digit);

		switch(type) {
			case Type.High:
				return new List<int>();
			case Type.Pair:
				return extractSome(maxFive);
			default:
				return maxFive.Select(index => Card.CardIndex(index)).ToList() ;
		}	
	}	
}


public class Card : MonoBehaviour {
	public Sprite Face;
	public Sprite CardBack;
    public VectorImage NumberPic;
    public VectorImage SuitPic;
    public Image FigurePic;
    public Sprite[] Figures;
	private int _index = -1;

	public int Index {
		get {
			return _index;
		}
	}

	[SerializeField]private Image cardBg;
	[SerializeField]private GameObject darkenCover;
	[SerializeField]private GameObject highlight;

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
		reColor();
		
		if (anim && _index != index) { 
			startShow(index, complete);
		} else {
            setCardFace(index, cardColor);
			flipTransform.localScale = Vector3.one;
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

	public void reColor() {
		darkenCover.SetActive(false);
	}

	public void Show(int index, bool anim = false, Action complete = null) {
		if (index < 0) {
			return ;
		}

		if (index == 0) {
			Turnback();
		} else {
			var realIndex = Card.CardIndex(index);
			show(realIndex, anim, complete);
		}
	}

	public void ShowIfDarken(int index, bool darken) {
		Show(index);
		if (darken) {
			Darken();
		}
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
		gameObject.SetActive(true);
		startShow(_index);
		G.PlaySound("fapai_1");
	}

	private IEnumerator flipCoroutine; 

	private void startShow(int index, Action complete = null) {
		if (flipCoroutine != null) {
			StopCoroutine(flipCoroutine);
		}
		flipCoroutine = flipCard(index, complete);
		StartCoroutine(flipCoroutine);
	}

	void OnDisable() {
		hasReShow = false;
	}

	public void Highlight() {
		highlight.SetActive(true);
	}

	public void Unhighlight() {
		highlight.SetActive(false);
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
		var transform = flipTransform;
		var hasSet = false;
		
		while(time < 1f) {
			time = Mathf.Min(time + Time.deltaTime / TurnCardDuration, 1);
			float scale = scaleCurve.Evaluate(time);

			Vector3 vector = transform.localScale;
			vector.x = scale;
			transform.localScale = vector;

			if (time >= 0.5 && !hasSet) {
                setCardFace(index, cardColor);
				hasSet = true;
			}

			yield return new WaitForFixedUpdate();
		}

		transform.localScale = Vector3.one;

		if (complete != null) {
			complete();
		}
	}

	public void Turnback(bool hide = false) {
		_index = -1;
		cardBg.sprite = CardBack;
		cardBg.color = Color.white;

		highlight.SetActive(false);
		cardContent.SetChildrenActive(false);
		reColor();

		flipTransform.localScale = Vector3.one;

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

	static private CardType cardType {
		get {
			if (GameData.Shared.Type.Value == GameType.KingThree){
				return new KingThreeCardType();
			}

			return new NormalCardType();
		}
	}

	static public string GetCardDesc(int val) {
		return cardType.GetCardDesc(val);	
	}

	static public Vector2 Size = new Vector2(126, 172); 

	static private GameObject prefab; 

	static public Card LoadCard(Transform parent) {
		if (prefab == null) {
			prefab = Resources.Load<GameObject>("Prefab/Card");
		}

		var width = parent.GetComponent<RectTransform>().sizeDelta.x;

		var scale = width / Size.x;

		var go = GameObject.Instantiate(prefab, parent, false);
		var card = go.GetComponent<Card>();
		var transform = card.GetComponent<RectTransform>();

		transform.SetAsFirstSibling();
		transform.anchoredPosition = new Vector2(0, 0);		
		transform.localScale = new Vector3(scale, scale, 1);

		return card;
	}

	static public List<int> ExtractHighlightCards(List<int> maxFive, int maxFiveRank) {
		return cardType.ExtractHighlightCards(maxFive, maxFiveRank);	
	}

	static public void HighlightCards(List<Card> cards, List<int> highlightIndex) {
		for (var i = 0; i < cards.Count; i++) {
			var card = cards[i];

			if (highlightIndex.Contains(card.Index)) {
				card.Highlight();
			} else {
				card.Unhighlight();
			}			
		}	
	}
}
