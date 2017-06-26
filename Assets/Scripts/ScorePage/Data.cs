using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ScorePage
{
    public class Data
    {
    }

    public class InsuranceRowData : Data
    {
        public int Number;
    }

    public class PlayerRowData: Data {
        public int TakeCoin;
        public string Nick;
        public int Score;
        public bool HasSeat;
    }

    public class GuestHeadData: Data {
        public int Number;
    }

    public class GuestRowData: Data {
        public List<PlayerModel> PlayerList;
    }

    public class LeaveIconData: Data{}

    public class Data27: Data {
        public int Number;
    }
}