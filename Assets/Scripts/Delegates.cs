using System;
using System.Collections.Generic;
using UniRx;

public class DelegateArgs: EventArgs {
	public Dictionary<string, object> Data;
	public DelegateArgs(Dictionary<string, object> data) {
		Data = data;
	}

	public DelegateArgs(){}
}

public class RxData: EventArgs {
	public Dictionary<string, object> Data;
	public RxData(Dictionary<string, object> data) {
		Data = data;
	}

	public RxData(){}
}

public class RxSubjects {
	public static Subject<RxData> TakeSeat = new Subject<RxData>();
	public static Subject<RxData> TakeCoin = new Subject<RxData>();
	public static Subject<RxData> TakeMore = new Subject<RxData>();
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
}

public class Delegates {
	public static Delegates shared = new Delegates();

	public event EventHandler<DelegateArgs> TakeMore;


	public event EventHandler<DelegateArgs> SeeCard;
	public event EventHandler<DelegateArgs> Deal;
	public event EventHandler<DelegateArgs> MoveTurn;

    public event EventHandler<DelegateArgs> Fold;
    public event EventHandler<DelegateArgs> Check;
    public event EventHandler<DelegateArgs> Call;
    public event EventHandler<DelegateArgs> AllIn;
    public event EventHandler<DelegateArgs> Raise;

	
	public void OnSeeCard(DelegateArgs e) {
		if (SeeCard != null) {
			SeeCard(this, e);
		}
	}

	public void OnDeal(DelegateArgs e) {
		if (Deal != null) {
			Deal(this, e);
		}
	}

	public void OnMoveTurn(DelegateArgs e) {
		if (MoveTurn != null) {
			MoveTurn(this, e);
		}
	}

	public void OnFold(DelegateArgs e) {
		if (Fold != null) {
			Fold(this, e);
		}
	}

	public void OnCheck(DelegateArgs e) {
		if (Check != null) {
			Check(this, e);
		}
	}

	public void OnRaise(DelegateArgs e) {
		if (Raise != null) {
			Raise(this, e);
		}
	}

	public void OnCall(DelegateArgs e) {
		if (Call != null) {
			Call(this, e);
		}
	}

	public void OnAllIn(DelegateArgs e) {
		if (MoveTurn != null) {
			MoveTurn(this, e);
		}
	}

	public void OnTakeMore(DelegateArgs e) {
		if (TakeMore != null) {
			TakeMore(this, e);
		}
	}
}

