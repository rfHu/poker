using UnityEngine;
using MaterialUI;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;
using System.Collections.Generic;

public class PlayerActGo: MonoBehaviour {
    public  VectorImage Arrow;
    public Text ActText;

    public void SetAct(ActionState act) {
       var color = GetColor(act); 
    
    //    gameObject.GetComponent<ProceduralImage>().color = color;
    //    Arrow.color = color;
       ActText.color = color;
       ActText.text = GetText(act);
    }

    public Color GetColor(ActionState act) {
        var map = new Dictionary<ActionState, Color>() {
            {ActionState.Fold, _.HexColor("#BDBDBD")},
            {ActionState.Check, MaterialUI.MaterialColor.cyanA200},
            {ActionState.Raise, _.HexColor("#FFAB40")},
            {ActionState.Call, _.HexColor("#40C4FF")},
            {ActionState.Allin, _.HexColor("#FFFF00")}
        };

        return map[act];
    }

    public string GetText(ActionState act) {
        var map = new Dictionary<ActionState, string>() {
            {ActionState.Fold, "弃牌"},
            {ActionState.Check, "看牌"},
            {ActionState.Raise, "加注"},
            {ActionState.Call, "跟注"},
            {ActionState.Allin, "全下"}
        };

        return map[act];
    }
}