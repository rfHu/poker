using UnityEngine.UI;
using UnityEngine;

namespace ScorePage {
     public class GuestAvt: MonoBehaviour {
        public Text NameText;
        public Avatar Avt;

        public void SetData(PlayerModel player) {
            NameText.text = player.name;
            Avt.Uid = player.uid;
            Avt.SetImage(player.avatar);            


            Avt.SetAlpha(player.in_room);
        }
     }
}