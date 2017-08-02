using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using System.Collections;
using UnityEngine.UI;
using System;
using DarkTonic.MasterAudio;
using UniRx;

public class CircleMask : MonoBehaviour {
	bool activated = false;
	private float time = 0;
	private int tickTime = 8; 
	private int vibraTime = 3;
	ProceduralImage proImage;
	public Text numberText;

	public bool EnableTick = false;

	static private GameObject maskCache;

	void Awake()
	{
		if (maskCache == null) {
			maskCache = Resources.Load<GameObject>("Prefab/CircleMask");
		}

		var go = Instantiate(maskCache);
		proImage = go.GetComponent<ProceduralImage>();
		
		proImage.transform.SetParent(transform, false);
		proImage.enabled = false;

		numberText = go.transform.Find("Number").GetComponent<Text>();
		numberText.enabled = false;	
	}

	public void Enable(float left, bool enableTick) {
		if (runCoroutine != null) {
			StopCoroutine(runCoroutine);
		}

		this.EnableTick = enableTick;

		if (left < 0) {
			return ;
		}

		proImage.enabled = true;
		numberText.enabled = true;
		numberText.gameObject.SetActive(true);
        runCoroutine = run(left);
        StartCoroutine(runCoroutine);
	}

    IEnumerator runCoroutine;

	public void Disable() {
		activated = false;
		proImage.enabled = false;
		numberText.enabled = false;
	}

	IEnumerator run(float left) {
		activated = true;
		time = left;

		float total = left.GetThinkTime();
		var flag = false;

		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			SetFillAmount(time / total, time);

			// 最后8秒出倒计时的声音
			if (!flag && time <= tickTime && EnableTick) {
				flag = true;
				tickSound();
				clickSound();
			}
			
			yield return new WaitForFixedUpdate(); 
		}

		activated = false;
	}

	private void tickSound() {
		if (!activated) {
			return ;
		}

		G.PlaySound("half_time");
	}

	private void clickSound() {
		if (!activated || time > tickTime) {
			return ;
		}

		G.PlaySound("time");

		if (time <= vibraTime) {
			Vibration.Vibrate(500);
		}

		Invoke("clickSound", 1f);
	}

	void OnDisable() {
		activated = false;
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
