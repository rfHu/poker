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
            Avt.SetAlpha(player.in_room);

            if (player.in_room) {
                NameText.color = new Color(1, 1, 1);
            } else {
                NameText.color = new Color(1, 1, 1, 0.6f);
            }
        }
     }
}