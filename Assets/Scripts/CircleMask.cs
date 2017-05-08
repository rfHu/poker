using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using System.Collections;
using UnityEngine.UI;
using System;
using DarkTonic.MasterAudio;
using UniRx;
using Extensions;

public class CircleMask : MonoBehaviour {
	bool activated = false;
	ProceduralImage proImage;
	public Text numberText;

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

	public void Enable(float left, bool enableTick) {
		this.EnableTick = enableTick;

		if (left < 0) {
			return ;
		}

		proImage.enabled = true;
		numberText.enabled = true;
        runCoroutine = run(left);
        StartCoroutine(runCoroutine);
	}

    IEnumerator runCoroutine;

	public void Reset(float left) {
        StopCoroutine(runCoroutine);

        proImage.enabled = true;
        numberText.enabled = true;
        numberText.gameObject.SetActive(true);
        runCoroutine = run(left);
		StartCoroutine(runCoroutine);
	}

	public void Disable() {
		activated = false;
		proImage.enabled = false;
		numberText.enabled = false;
	}

	IEnumerator run(float left) {
		activated = true;

		float time = left;
		float total = left.GetThinkTime();
		var flag = false;

		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			SetFillAmount(time / total, time);

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
		if (!activated || int.Parse(numberText.text) > 8) {
			return ;
		}

		MasterAudio.PlaySound("time");

		Invoke("tickSound", 1f);
	}

	public void SetTextColor(Color color) {
		numberText.color = color;
	}

	public void SetFillAmount(float percent, float left) {
		if (!proImage.enabled) {
			proImage.enabled = true;
			numberText.enabled = true;
		}

		if (percent <= 0) {
			numberText.gameObject.SetActive(false);
		} 

		proImage.fillAmount = percent;
		numberText.text =  (Math.Ceiling(left)).ToString();
	}
}
