using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine.UI;

public class SelfCards: MonoBehaviour {
	public List<CardContainer> Containers;

	public List<Card> Cards {
		get {
			if (cards == null) {
				cards = Containers.Select((o) => o.CardInstance).ToList();
			}	

			return cards;
		}
	}
	private List<Card> cards;

	private Player player;
	
	public List<GameObject> Eyes;

	private bool hasShow = false;
	private bool gameover;

	void Awake() {
		for(var i = 0; i < Containers.Count; i++) {
			var j = i;
			var btn = Containers[j].GetComponent<Button>();
			btn.onClick.AddListener(() => {
				toggleEye(j);
			});
		}
	}

	void OnDespawned() {
		this.Dispose();

		hasShow = false;
		gameover = false;

		if (coroutine != null) {
			StopCoroutine(coroutine);
		}

		Turnback();	
	}

	private IEnumerator coroutine; 

	private void toggleEye(int index) {
		var value = new System.Text.StringBuilder(player.ShowCard.Value.PadLeft(4, '0'));

		// 这一手结束后，只能亮牌，不能关闭亮牌
		if (value[index] == '1' && gameover) {
			return ;
		}

		value[index] =  value[index] == '0' ? '1' : '0';

		player.ShowCard.Value = value.ToString();

		// 转换10进制
		var num = Convert.ToInt16(value.ToString(), 2); 

		// 发送请求
		Connect.Shared.Emit(new Dictionary<string, object> {
			{"f", "showcard"},
			{"args", new Dictionary<string, object> {
				{"showcard", num}
			}}
		});
	}

	public void Show(List<int> indexList) {
		if (coroutine != null) {
			StopCoroutine(coroutine);
		}

		coroutine = reShow(indexList);
		StartCoroutine(coroutine);
	}

	public void Show(List<int> indexList, bool anim) {
		iterateIndex((card, i) => {
			card.Show(indexList[i], anim);
		});
	}

	private IEnumerator reShow(List<int> indexList) {
		var i = 0;
		foreach(var card in Cards) {
			var index = indexList[i];
			if (index <= 0) {
				continue ;
			}

			show(card, index);

			if (!hasShow) {
				yield return new WaitForSeconds(0.1f);
			}

			i++;
		}

		hasShow = true;
		yield return null;
	}

	private void show(Card card, int index) {
		if (hasShow) {
			card.ReShow();
		} else {
			card.ShowWithSound(index, true);
		}
	}

	public void ShowIfDarken(List<int> indexList, bool darken) {
		iterateIndex((card, i) => {
			card.ShowIfDarken(indexList[i], darken);
		});
		hasShow = true;
	}

	public void Darken() {
		iterate((card) => {
			card.Darken();
		});
	}

	public void Turnback() {
		iterate((card) => {
			card.Turnback();
		});	
	}

	private void iterate(Action<Card> cb) {
		foreach(var card in Cards) {
			cb(card);
		}
	}

	private void iterateIndex(Action<Card, int> cb) {
		var i = 0;
		foreach(var card in Cards) {
			cb(card, i);
			i++;
		}
	}

	public void Despawn() {
		PoolMan.Despawn(transform);
	}

	private void addEvents() {
		RxSubjects.GameOver.Subscribe((_) => {
			gameover = true;
		}).AddTo(this);

		player.ShowCard.Subscribe((value) => {
			value = value.PadLeft(4, '0');

			for(var i = 0; i < value.Length; i++) {
				var c = value[i];

				if (i >= Eyes.Count) {
					break;
				}

				if (c == '1') {
					Eyes[i].SetActive(true); 
				} else {
					Eyes[i].SetActive(false);
				}
			}
		}).AddTo(this);	
	}

	public static SelfCards Create(GameObject prefab, Player player) {
		var transform = PoolMan.Spawn(prefab);
		var sc = transform.GetComponent<SelfCards>();
		sc.player = player;
		sc.addEvents();
		return sc;
	}
}