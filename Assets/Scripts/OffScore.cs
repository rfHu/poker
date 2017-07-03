using UnityEngine;
using UnityEngine.UI;

public class OffScore: MonoBehaviour {
    public Avatar Avt;
    public Text TakeCoinText;
    public Text ProfitText;

    public void Show(string avatar, int takeCoin, int profit) {
        Avt.SetImage(avatar);
        TakeCoinText.text = takeCoin.ToString();
        ProfitText.text = _.Number2Text(profit);
        ProfitText.color = _.GetTextColor(profit);

        GetComponent<DOPopup>().Show();
    }
}