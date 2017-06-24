using UnityEngine;
using System.Collections;

namespace ScorePage
{
    public class Data
    {
    }

    public class InsuranceRowData : Data
    {
        public int number;
    }

    public class PlayerRowData: Data {
        public int TakeCoin;
        public string Nick;
        public int Score;
        public bool HasSeat;
    }
}