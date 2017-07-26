using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class RxData: EventArgs {
	public Dictionary<string, object> Data;
	public string E;
	public RxData(Dictionary<string, object> data, string e) {
		Data = data;
		E = e;
	}

	public RxData(){}
}

public class RxSubjects {
	public static Subject<RxData> TakeSeat = new Subject<RxData>();
	public static Subject<RxData> TakeCoin = new Subject<RxData>();
	public static Subject<RxData> UnSeat = new Subject<RxData>();
	public static Subject<RxData> Ready = new Subject<RxData>();
	public static Subject<RxData> GameStart = new Subject<RxData>();
	public static Subject<RxData> SeeCard = new Subject<RxData>();
	public static Subject<RxData> Look = new Subject<RxData>();
	public static Subject<RxData> Deal = new Subject<RxData>();
	public static Subject<RxData> MoveTurn = new Subject<RxData>();
	public static Subject<RxData> Fold = new Subject<RxData>();
	public static Subject<RxData> Check = new Subject<RxData>();
	public static Subject<RxData> Call = new Subject<RxData>();
	public static Subject<RxData> AllIn = new Subject<RxData>();
	public static Subject<RxData> Raise = new Subject<RxData>();
	public static Subject<RxData> GameOver = new Subject<RxData>();
	public static Subject<RxData> Paused = new Subject<RxData>();
	public static Subject<RxData> Pausing = new Subject<RxData>();
	public static Subject<RxData> Started = new Subject<RxData>();
	public static Subject<RxData> Bye = new Subject<RxData>(); // 登陆互斥
	public static Subject<RxData> GameEnd = new Subject<RxData>();
	public static Subject<RxData> Audit = new Subject<RxData>();
	public static Subject<RxData> UnAuditCD = new Subject<RxData>();
	public static Subject<RxData> Pass  = new Subject<RxData>();
	public static Subject<RxData> UnPass = new Subject<RxData>();
	public static Subject<RxData> TakeMore = new Subject<RxData>();
	public static Subject<RxData> Ending = new Subject<RxData>();
    public static Subject<RxData> Modify = new Subject<RxData>();
    public static Subject<RxData> Emoticon = new Subject<RxData>();
    public static Subject<RxData> KickOut = new Subject<RxData>();
    public static Subject<RxData> StandUp = new Subject<RxData>();
    public static Subject<RxData> Insurance = new Subject<RxData>();
    public static Subject<RxData> ToInsurance = new Subject<RxData>();
    public static Subject<RxData> Moretime = new Subject<RxData>();
	public static Subject<RxData> ShowCard = new Subject<RxData>();
	public static Subject<RxData> SomeOneSeeCard = new Subject<RxData>();
    public static Subject<RxData> Expression = new Subject<RxData>();
	public static Subject<RxData> GamerState = new Subject<RxData>();
    public static Subject<RxData> NoTalking = new Subject<RxData>();
    public static Subject<RxData> ShowInsurance = new Subject<RxData>();
    public static Subject<RxData> RsyncInsurance = new Subject<RxData>();
    public static Subject<RxData> Award27 = new Subject<RxData>();
	public static Subject<RxData> OffScore = new Subject<RxData>();
    public static Subject<RxData> MatchRank = new Subject<RxData>();
	public static Subject<RxData> RaiseBlind = new Subject<RxData>();
	public static Subject<RxData> MatchLook = new Subject<RxData>();
    public static Subject<RxData> Rebuy = new Subject<RxData>();
    public static Subject<RxData> AddOn = new Subject<RxData>();

	// 跟网络无关事件
	public static Subject<int> ChangeVectorsByIndex = new Subject<int>();
	public static Subject<string> SendChat = new Subject<string>();
	public static Subject<string> ShowAudio = new Subject<string>();
	public static Subject<string> HideAudio = new Subject<string>();
    public static Subject<bool> Seating = new Subject<bool>();
	public static Subject<bool> Connecting = new Subject<bool>();
	public static Subject<GainChip> GainChip = new Subject<GainChip>();
}

public struct GainChip {
	public ChipsGrp Grp;
	public string Uid;

	public GainChip(ChipsGrp chipsGrp, string uid) 
   {
      this.Grp = chipsGrp;
      this.Uid = uid;
   }
}


