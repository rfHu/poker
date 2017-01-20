using UnityEngine;
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

		var vector = transform.GetComponent<RectTransform>().sizeDelta;
		proImage.GetComponent<RectTransform>().sizeDelta = vector; 
		
		proImage.transform.SetParent(transform, false);
		proImage.enabled = false;

		numberText = go.transform.Find("Number").GetComponent<Text>();
		numberText.gameObject.SetActive(false);
	}

	public void Enable(float elaspe = 0) {
		if (elaspe > Duration || elaspe < 0) {
			return ;
		}

		proImage.enabled = true;
		numberText.gameObject.SetActive(true);
		StartCoroutine(run(elaspe));
	}

	public void Disable() {
		activated = false;
	}

	IEnumerator run(float elaspe = 0) {
		activated = true;

		float time = Duration - elaspe;
		while (time > 0 && activated) {
			time = time - Time.deltaTime;
			proImage.fillAmount = time / Duration;
			numberText.text =  ((int)time).ToString();
			yield return new WaitForFixedUpdate();
		}

		activated = false;
	}	
}
