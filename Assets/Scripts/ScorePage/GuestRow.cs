using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

namespace ScorePage {
    public class GuestRow : CellView
    {
        public List<GuestAvt> Guests; 

        public override bool CanPresentModelType(Type modelType) { return modelType == typeof(GuestRowData); }

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

        override public void CollectViews() {
            base.CollectViews();
            Guests = new List<GuestAvt>(); 
            Guests.Add(find("Guest1")); 
            Guests.Add(find("Guest2")); 
            Guests.Add(find("Guest3")); 
            Guests.Add(find("Guest4")); 
        }

        private GuestAvt find(string name) {
            return root.Find(name).GetComponent<GuestAvt>();
        }
    }
}
 