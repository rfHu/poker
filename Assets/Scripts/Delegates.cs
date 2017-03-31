using System;
using System.Collections.Generic;
using UniRx;

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
	public static Subject<RxData> Started = new Subject<RxData>();
	public static Subject<RxData> Exclusion = new Subject<RxData>(); // 登陆互斥
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

	// 跟网络无关事件
	public static Subject<int> ChangeVectorsByIndex = new Subject<int>();
}


