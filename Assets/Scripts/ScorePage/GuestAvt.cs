using UnityEngine.UI;
using UnityEngine;

namespace ScorePage {
     public class GuestAvt: MonoBehaviour {
        public Text NameText;
        public Avatar Avt;

        public void SetData(PlayerModel player) {
            gameObject.SetActive(true);
            
            NameText.text = player.name;
            Avt.Uid = player.uid;
            Avt.SetImage(player.avatar);            

            var cvg = GetComponent<CanvasGroup>();

            if (player.in_room) {
                cvg.alpha = 1;
            } else {
                cvg.alpha = 0.6f;
            }
        }
     }
}