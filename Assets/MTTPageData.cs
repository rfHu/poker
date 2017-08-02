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
    }

    public class AwardListGoData : MTTPageData 
    {
        public int Rank;
        public string Award;
    }
}