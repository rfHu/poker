using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScoreCtrl : MonoBehaviour {
	public GameObject scoreEntry;
	public GameObject viewport;
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
			 entry.transform.Find("Total").GetComponent<Text>().text = ((int)player["buy"]).ToString(); 
			 entry.transform.Find("Score").GetComponent<Text>().text = ((int)player["gain"]).ToString();

			 entry.transform.SetParent(viewport.transform, false);
		}
	}
}
