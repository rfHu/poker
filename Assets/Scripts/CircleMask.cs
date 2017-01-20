﻿using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using System.Collections;
using UnityEngine.UI;

public class CircleMask : MonoBehaviour {
	public int Duration = 15;
	bool activated = false;
	ProceduralImage proImage;
	Text numberText;

	void Awake()
	{
		var go = (GameObject)Instantiate(Resources.Load("Prefab/CircleMask"));
		proImage = go.GetComponent<ProceduralImage>();
		
		proImage.transform.SetParent(transform, false);
		proImage.enabled = false;

		numberText = go.transform.Find("Number").GetComponent<Text>();
		numberText.enabled = false;	
	}

	public void Enable(float elaspe = 0) {
		if (elaspe > Duration || elaspe < 0) {
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

		float time = Duration - elaspe;
		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			SetFillAmount(time);
			yield return new WaitForFixedUpdate();
		}

		activated = false;
	}

	public void SetTextColor(Color color) {
		numberText.color = color;
	}

	public void SetFillAmount(float left) {
		if (!proImage.enabled) {
			proImage.enabled = true;
			numberText.enabled = true;
		}

		proImage.fillAmount = left / Duration;
		numberText.text =  ((int)left).ToString();
	}
}