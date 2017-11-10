using UnityEngine;
using MaterialUI;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Collections.Generic;
using DG.Tweening;

public class PlayerActGo: MonoBehaviour {
    public Text ActText;

    public int AnimID;

    public void SetAct(ActionState act) {

        var color = GetColor(act); 
    
        var tab = transform.Find("Tab").GetComponent<RectTransform>();
        tab.GetComponent<VectorImage>().color = color;

        ActText.text = GetText(act);
    }

    public Color GetColor(ActionState act) {
		var orange = _.HexColor("#FFAB40");
		var grey = _.HexColor("#BDBDBD");

        var map = new Dictionary<ActionState, Color>() {
            {ActionState.Fold, grey},
            {ActionState.Check, MaterialUI.MaterialColor.cyanA200},
            {ActionState.Raise, orange},
            {ActionState.Call, _.HexColor("#40C4FF")},
            {ActionState.Allin, orange},
			{ActionState.Straddle, orange},
            {ActionState.TonicBlind, orange},
            {ActionState.BuryCard, grey}
        };

        return map[act];
    }

    public void SetActive(bool active, bool anim = true) {
        var cvg = GetComponent<CanvasGroup>();
        var targetValue = active ? 1 : 0;
        var duration = 0.1f;

        if (anim) {
            cvg.DOFade(targetValue, duration).SetId(AnimID);
        } else {
            cvg.alpha = targetValue;
        }
    }

    public void ChangePos(SeatPosition pos) {
        var rect = GetComponent<RectTransform>();
        var tab = rect.transform.Find("Tab").GetComponent<RectTransform>();

        if (pos == SeatPosition.Left || pos == SeatPosition.TopRight) {
            tab.rotation = Quaternion.Euler(new Vector2(0, 180));
            rect.anchoredPosition = new Vector2(64, -48);
        } else {
            tab.rotation = Quaternion.Euler(new Vector2(0, 0));       
            rect.anchoredPosition = new Vector2(-64, -48);        
        }
    }

    public string GetText(ActionState act) {
        var map = new Dictionary<ActionState, string>() {
            {ActionState.Fold, "弃牌"},
            {ActionState.Check, "看牌"},
            {ActionState.Raise, "加注"},
            {ActionState.Call, "跟注"},
            {ActionState.Allin, "全下"},
			{ActionState.Straddle, "抓头"},
            {ActionState.TonicBlind, "补盲"},
            {ActionState.BuryCard, "埋牌"},
        };

        return map[act];
    }
}