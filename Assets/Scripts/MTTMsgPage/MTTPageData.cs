using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MTTMsgPage
{
    public abstract class MTTPageData
    {
        public Type cachedType { get; private set; }

        public MTTPageData()
        { cachedType = GetType(); }

        public bool needbg;
    }

    public class AwardListGoData : MTTPageData 
    {
        public string Rank;
        public string Award;
    }

    public class TableListGoData : MTTPageData
    {
        public int Num;
        public int gamersCount;
        public int Min;
        public int Max;
    }

    public class BBLevelListGoData : MTTPageData 
    {
        public int Level;
        public int BB;
        public int Ante;
    }
}