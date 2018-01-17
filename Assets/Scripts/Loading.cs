using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Loading : MonoBehaviour {
	public Text LoadingText;
	public string Txt;

	public List<string> TextList = new List<string>{
		"只要你不是最好的牌，就要认真思索对方下重注的理由",
		"多总结下你输大钱的那些牌，比一直不停的打下去更重要",
		"唯一比凶猛的对手更可怕的，是你错误的打牌方法",
		"当你把对手套进去的时候，通常你自己也被套进去了",
		"如果你有一手大牌，别忘了对手可能有一手更大的牌",
		"当对手给你一个合适的彩池比例时，通常你已经输了",
		"那个一直弃牌的傻瓜其实才是能一口吞掉你的巨鲨",
		"不要坐在两个比你更有攻击性的对手中间",
		"没必要花所有的筹码去验证对手是否偷鸡",
		"沉闷的赢钱总比畅快的输钱要好"		
	};

	void Awake()
	{
		SetRndText();	
	}

	public void SetRndText() {
		if (string.IsNullOrEmpty(Txt)) {
			var rnd = new System.Random();
			int index = rnd.Next(10);
			LoadingText.text = TextList[index];
		} else {
			LoadingText.text = Txt;	
		}
	}
}
