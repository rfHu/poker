using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using EnhancedUI.EnhancedScroller;

namespace ScorePage {
    public class GuestRow : CellView
    {
        public GuestAvt[] Guests; 

        override public void SetData(Data data)
        {
            base.SetData(data);
            var dt = data as GuestRowData;

            for (var i = 0; i < 4; i++) {
                var guest = Guests[i];

                if (dt.PlayerList.Count > i) {
                    guest.SetData(dt.PlayerList[i]);
                } else {
                    guest.gameObject.SetActive(false);
                }
            }            
        }
    }
}
 