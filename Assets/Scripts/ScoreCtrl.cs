using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScoreCtrl : MonoBehaviour {
	public GameObject scoreEntry;
	public GameObject lookHeader;
	public GameObject viewport;
	public GameObject lookerPrefab;

	public GameObject lookerGridLayout;

	List<Dictionary<string, object>> playerScoreList = new List<Dictionary<string, object>>();



	void Start()
	{
		foreach(int i in Enumerable.Range(0, 7)) {
			playerScoreList.Add(
			new Dictionary<string, object>(){
				{"buy", 100},
				{"gain", 1000},
				{"name", "singno"}
			}
			);
		}

		foreach(Dictionary<string, object> player in playerScoreList) {
			 GameObject  entry = Instantiate(scoreEntry);
			 entry.transform.Find("Name").GetComponent<Text>().text = (string)player["name"];
			 entry.transform.Find("Total").GetComponent<Text>().text = player["buy"].ToString(); 
			 entry.transform.Find("Score").GetComponent<Text>().text = player["gain"].ToString();
			 entry.transform.SetParent(viewport.transform, false);
		}

		Instantiate(lookHeader).transform.SetParent(viewport.transform, false);

		Debug.Log(viewport.GetComponent<RectTransform>().rect.width);

		// 每个item相距30，两边留20
		float width = 150;
		float height = 170;
		GridLayoutGroup gridLayout = Instantiate(lookerGridLayout).GetComponent<GridLayoutGroup>();
		gridLayout.cellSize = new Vector2(width, height);
		gridLayout.transform.SetParent(viewport.transform, false);

		for (var i = 0; i < 10; i++) {
			GameObject obj = Instantiate(lookerPrefab);
			
			RawImage img = obj.transform.Find("RawImage").GetComponent<RawImage>();
			StartCoroutine(DownloadImage(img));
			obj.transform.Find("Text").GetComponent<Text>().text = "我是大番薯";

			obj.transform.SetParent(gridLayout.transform, false);
		}
	}

	IEnumerator<WWW> DownloadImage(RawImage img) {
		string url = "https://ss1.bdstatic.com/70cFvXSh_Q1YnxGkpoWK1HF6hhy/it/u=3081053742,1983158129&fm=116&gp=0.jpg";
		WWW www = new WWW(url);
		yield return www;
		img.texture = Extension.Circular(www.texture);
	}
}
