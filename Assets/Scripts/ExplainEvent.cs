using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using MaterialUI;
using System.Linq;

public class ExplainEvent : MonoBehaviour {
    public GameObject NormalText;

	public TabPage[] Pages;

	private TabView tabView;

	void Awake()
	{
		tabView = GetComponent<TabView>();

		GameData.Shared.Type.Subscribe((type) => {
			// 先隐藏所有页面
			foreach(var page in Pages) {
				page.gameObject.SetActive(false);
			}

			SetList(type);
		}).AddTo(this);
	}

    private void SetList(GameType type)
    {
        bool isKingThree = type == GameType.KingThree;
        NormalText.SetActive(!isKingThree);

		// 6+隐藏保险
		if (type == GameType.SixPlus) {
			tabView.pages = new TabPage[]{
				Pages[1]
			};
		} else if (type == GameType.Omaha) { // 展示奥马哈
			tabView.pages = Pages.ToArray();
		} else { // 隐藏奥马哈
			tabView.pages = new TabPage[]{
				Pages[0],
				Pages[1]
			};
		}

		tabView.InitializeTabs();
		tabView.InitializePages();
		tabView.tabsContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
    }
}
