using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;

    public class InsuranceRow : EnhancedScrollerCellView
    {
        public Text ScoreText;

        private int score;

        public void SetScore(int score)
        {
            this.score = score;
            ScoreText.text = _.Number2Text(score);
            ScoreText.color = _.GetTextColor(score);
        }
    }