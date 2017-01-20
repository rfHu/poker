using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using System.Collections;

public class CircleMask : MonoBehaviour {
	public int Duration = 15;
	bool activated = false;
	ProceduralImage proImage;

	void Start()
	{
		var go = (GameObject)Instantiate(Resources.Load("Prefab/CircleMask"));
		proImage = go.GetComponent<ProceduralImage>();

		var vector = transform.GetComponent<RectTransform>().sizeDelta;
		proImage.GetComponent<RectTransform>().sizeDelta = vector; 

		proImage.transform.SetParent(transform, false);
	}

	public void Enable(float elaspe = 0) {
		if (elaspe > Duration || elaspe < 0) {
			return ;
		}

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
			GetComponent<ProceduralImage>().fillAmount = time / Duration;
			yield return new WaitForFixedUpdate();
		}

		activated = false;
	}	
}
