using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ScorePage
{
    public abstract class Data
    {
        public Type cachedType { get; private set; }
        
        public Data()
        { cachedType = GetType(); }
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

    public class LeaveIconData: Data{
        public string Label;
    }

    public class Data27: Data {
        public int Number;
    }
}