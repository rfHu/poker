using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MTTP3ChildGo : MonoBehaviour {

    public Text Level;

    public Text SBBB;

    public Text Ante;

    public void SetText(int level, int BB, int AnteNum) 
    {
        Level.text = level.ToString();

        int SB = BB/2;
        SBBB.text = SetNumber(SB) + "/" + SetNumber(BB);

        Ante.text = SetNumber(AnteNum);

        GetComponent<Image>().enabled = (level % 2 == 1);
    }

    private string SetNumber(int num) 
    {
        return num >= 100000 ? num / 10000f + "万" : num.ToString();
    }
}
