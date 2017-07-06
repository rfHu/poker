using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.UI.ProceduralImage;
using System;
using DG.Tweening;
using UniRx;
using DarkTonic.MasterAudio;
using System.Linq;
using SimpleJSON;

public class PlayerObject : MonoBehaviour {
	public int Index;
	public GameObject WinImageGo;
	public GameObject Avt;
	public bool activated = false;
	public string Uid = "";
	public GameObject Cardfaces;
	public List<Transform> MyCards;
    public GameObject WinStars;
	public Text WinNumber;
	public PlayerActGo PlayerAct;
	public Sprite[] ActSprites;
	public GameObject AllinGo;

	public Text ScoreLabel;
	public GameObject Countdown;
	public GameObject Circle;
	public GameObject AutoArea;
	public GameObject[] AutoOperas; 
	

	private Transform OPTransform;
	private ChipsGo cgo; 
	private Player player;
	private ActionState lastState;
	private bool gameover = false;


	void OnSpawned() {
		Countdown.SetActive(false);
		Avt.GetComponent<CanvasGroup>().alpha = 1;	
		Avt.GetComponent<CircleMask>().Disable();
		Cardfaces.GetComponent<Image>().color = new Color(1, 1, 1);
		Cardfaces.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -20);
		AutoArea.SetActive(false);
		MyCards[0].parent.gameObject.SetActive(false);
	}

	// void OnDespawned()
	// {
	// 	if (OPTransform != null && GameData.Shared.MySeat == -1) {
	// 		PoolMan.Despawn(OPTransform);
	// 	}

    //     if (isSelf && GameData.Shared.MySeat == -1)
    //     {
    //         RxSubjects.Seating.OnNext(false);
    //     }
	// }


	private bool isSelf {
		get {
			return Uid == GameData.Shared.Uid;
		}
	}

	private void hideAnim() {
		hideGo(WinStars, () => {
			ScoreLabel.transform.parent.gameObject.SetActive(true);
		});
		hideGo(WinImageGo);	
		// hideGo(getShowCard());	
		// hideGo(getOtherCardGo());
		hideGo(WinNumber.transform.parent.gameObject);
	}

	private void hideGo(GameObject go, Action callback = null) {
		if (!go.activeSelf) {
			return ;
		}
		go.GetComponent<CanvasGroup>().DOFade(0, 0.3f).OnComplete(() => {
			go.SetActive(false);

			if (callback != null) {
				callback();
			}
		});
	}
}
