using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class SNGRank1: MonoBehaviour {
        [SerializeField]private Sprite[] sprites;

        public void SetRank(int rank) {
            var img = GetComponent<Image>();

            if (rank > 3 || rank <= 0) {
                return ;
            }

            img.sprite = sprites[rank - 1];
        }
    }
}