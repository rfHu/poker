using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UserCards: MonoBehaviour {
	public List<CardContainer> Containers;
	public List<Card> Cards;

	private bool hasShow = false;

	public void Initial() {
		Cards = Containers.Select(o => o.CardInstance).ToList();
	}

	void OnDespawned() {
		hasShow = false;

		if (coroutine != null) {
			StopCoroutine(coroutine);
		}

		iterate((card) => {
			card.Turnback();
		});	
	}

	private IEnumerator coroutine; 

	public void Show(List<int> indexList) {
		if (coroutine != null) {
			StopCoroutine(coroutine);
		}

		coroutine = reShow(indexList);
		StartCoroutine(coroutine);
		hasShow = true;
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
				yield return new WaitForSeconds(0.2f);
			}

			i++;
		}

		yield return null;
	}

	private void show(Card card, int index) {
		if (hasShow) {
			card.ReShow();
		} else {
			card.ShowWithSound(index, true);
		}
	}

	public void ShowIfDarken(List<int> indexList, bool inGame) {
		iterateIndex((card, i) => {
			card.ShowIfDarken(indexList[i], inGame);
		});
		hasShow = true;
	}

	public void Darken() {
		iterate((card) => {
			card.Darken();
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
}