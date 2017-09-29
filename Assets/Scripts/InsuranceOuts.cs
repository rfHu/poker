using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UnityEngine.UI.ProceduralImage;
using System.Linq;

public class InsuranceOuts : MonoBehaviour {
	public Text SelectedText;
	public Text TotalText;
	
	public Transform CardList;

	private List<int> outs;

	private Dictionary<int, Toggle> togglesDict; // 保存一个冗余的数据结构，方便查询

	[SerializeField]private Toggle checkAllToggle;
	[SerializeField]private ProceduralImage checkAllImage;

	private InsuranceStruct caller;

	public HashSet<int> SelectedOuts; 

	public void SetOuts(List<int> outs, InsuranceStruct caller) {
		SelectedOuts = new HashSet<int>(outs);
		togglesDict = new Dictionary<int, Toggle>();
		this.caller  = caller;

		if (outs.Count == 0) {
			gameObject.SetActive(false);
			CardList.gameObject.SetActive(false);
			return ;
		}

		gameObject.SetActive(true);
		CardList.gameObject.SetActive(true);

		SetCheckAllToggle(true);
		SelectedText.text = outs.Count.ToString();
		TotalText.text = "/ " + outs.Count.ToString();

		checkAllToggle.gameObject.SetActive(!caller.mustBuy);

		renderCards(outs);

		 RxSubjects.RsyncInsurance.Subscribe((e) => {
			 HashSet<int> selected ;

			 if (e.Data.ContainsKey("isoff")) {
				var off = e.Data.IL("isoff");
				selected = new HashSet<int>(outs);

				foreach(var value in off) {
					if (selected.Contains(value)) {
						selected.Remove(value); 
					}
				}
			 } else {
				 if (e.Data.ContainsKey("selectedOuts")) {
            		selected = new HashSet<int>(e.Data.IL("selectedOuts"));
				 } else {
					selected = new HashSet<int>(outs);
				 }
			 }

            foreach(var toggle in togglesDict) {
				if (selected.Contains(toggle.Key)) {
					toggle.Value.isOn = true;
				} else {
					toggle.Value.isOn = false;
				}
            }

        }).AddTo(this);
	}

	void OnDespawned() {
		this.Dispose();

		if (togglesDict == null) {
			return ;
		}

        foreach(var toggle in togglesDict)
		{
			toggle.Value.onValueChanged.RemoveAllListeners();
			PoolMan.Despawn(toggle.Value.transform);
		}
	}

	private void renderCards(List<int> outs) {
        foreach (var cardNum in outs)
        {
            var transform = PoolMan.Spawn("InsureCard", CardList);
			transform.SetAsLastSibling();
			var card = transform.GetComponent<CardContainer>().CardInstance;

            card.Show(cardNum);

            var toggle = transform.GetComponent<Toggle>();
			toggle.onValueChanged.RemoveAllListeners(); // 先移除事件

            transform.GetComponent<Toggle>().isOn = true;

			togglesDict.Add(cardNum, toggle);

            if (caller.mustBuy || !caller.isBuyer)
            {
                toggle.interactable = false;
            }
            else 
            {
                toggle.interactable = true;
            }

			toggle.onValueChanged.AddListener((isOn) => {
				if (isOn) {
					SelectedOuts.Add(cardNum);
				} else {
					SelectedOuts.Remove(cardNum);
				}

				SelectedText.text = SelectedOuts.Count.ToString();

				if (SelectedOuts.Count >= togglesDict.Count) {
					SetCheckAllToggle(true);
				} else {
					SetCheckAllToggle(false);
				}

				if (caller.SelectedCount == 0) {
					toggle.isOn = true;
				}

				caller.OnSelectedChange(this);
			});
        }
	}

	private void SetCheckAllToggle(bool isOn) {
       checkAllToggle.isOn = isOn;

        if (isOn) {
            checkAllImage.color = MaterialUI.MaterialColor.cyanA200;
        } else {
            checkAllImage.color = MaterialUI.MaterialColor.grey400;
        }
    }


	public void OnSLButtonClick()
    {
        if (caller.mustBuy || !caller.isBuyer)
        {
            return;
        }

		var isOn = !checkAllToggle.isOn;
		checkAllToggle.isOn = isOn;

		foreach(var toggle in togglesDict) {
			toggle.Value.isOn = isOn;
		}
    }
}
