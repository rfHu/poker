using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace ScorePage {
    public class SNGRank1: MonoBehaviour {
        [SerializeField]private Texture[] textures;

        public void SetRank(int rank) {
            var img = GetComponent<RawImage>();

            if (rank > 3 || rank <= 0) {
                return ;
            }

            img.texture = textures[rank - 1];
        }
    }
}