using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using System.Collections;
using UnityEngine.UI;
using System;
using DarkTonic.MasterAudio;
using UniRx;

public class CircleMask : MonoBehaviour {
	bool activated = false;
	ProceduralImage proImage;
	Text numberText;

	public bool EnableTick = false;

	void Awake()
	{
		var go = (GameObject)Instantiate(Resources.Load("Prefab/CircleMask"));
		proImage = go.GetComponent<ProceduralImage>();
		
		proImage.transform.SetParent(transform, false);
		proImage.enabled = false;

		numberText = go.transform.Find("Number").GetComponent<Text>();
		numberText.enabled = false;	
	}

	public void Enable(float elaspe, bool enableTick) {
		this.EnableTick = enableTick;

		if (elaspe > GameData.Shared.ThinkTime || elaspe < 0) {
			return ;
		}

		proImage.enabled = true;
		numberText.enabled = true;
		StartCoroutine(run(elaspe));
	}

	public void Disable() {
		activated = false;
		proImage.enabled = false;
		numberText.enabled = false;
	}

	IEnumerator run(float elaspe = 0) {
		activated = true;

		float time = GameData.Shared.ThinkTime - elaspe;
		var flag = false;

		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			SetFillAmount(time);

			// 最后8秒出倒计时的声音
			if (!flag && time <= 8) {
				flag = true;
				tickSound();
			}
			
			yield return new WaitForFixedUpdate(); 
		}

		activated = false;
	}

	private void tickSound() {
		if (!activated) {
			return ;
		}

		MasterAudio.PlaySound("time");

		Invoke("tickSound", 1f);
	}

	public void SetTextColor(Color color) {
		numberText.color = color;
	}

	public void SetFillAmount(float left) {
		if (!proImage.enabled) {
			proImage.enabled = true;
			numberText.enabled = true;
		}

		proImage.fillAmount = left / GameData.Shared.ThinkTime;
		numberText.text =  (Math.Ceiling(left)).ToString();
	}
}
