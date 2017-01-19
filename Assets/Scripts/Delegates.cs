using System;
using System.Collections.Generic;

public class DelegateArgs: EventArgs {
	public Dictionary<string, object> Data;
	public DelegateArgs(Dictionary<string, object> data) {
		Data = data;
	}

	public DelegateArgs(){}
}

public class Delegates {
	public static Delegates shared = new Delegates();

	public event EventHandler<DelegateArgs> TakeSeat;
	public event EventHandler<DelegateArgs> TakeCoin;
	public event EventHandler<DelegateArgs> TakeMore;
	public event EventHandler<DelegateArgs> UnSeat;
	public event EventHandler<DelegateArgs> Ready;

	public event EventHandler<DelegateArgs> GameStart;

	public event EventHandler<DelegateArgs> SeeCard;
	public event EventHandler<DelegateArgs> Look;
	public event EventHandler<DelegateArgs> Deal;
	public event EventHandler<DelegateArgs> MoveTurn;

    public event EventHandler<DelegateArgs> Fold;
    public event EventHandler<DelegateArgs> Check;
    public event EventHandler<DelegateArgs> Call;
    public event EventHandler<DelegateArgs> AllIn;
    public event EventHandler<DelegateArgs> Raise;
    public event EventHandler<DelegateArgs> GameOver;
    public event EventHandler<DelegateArgs> Paused;
    public event EventHandler<DelegateArgs> Started;

	// 登陆态互斥了
	public event EventHandler<DelegateArgs> Exclusion;
	public event EventHandler<DelegateArgs> GameEnd;
	public event EventHandler<DelegateArgs> Audit;
	

	public void OnTakeSeat(DelegateArgs e) {
		if (TakeSeat != null) {
			TakeSeat(this, e);
		}
	}

	public void OnTakeCoin(DelegateArgs e) {
		if (TakeCoin != null) {
			TakeCoin(this, e);
		}
	}

	public void OnUnSeat(DelegateArgs e) {
		if (UnSeat != null) {
			UnSeat(this, e);
		}
	}

	public void OnReady(DelegateArgs e) {
		if (Ready != null) {
			Ready(this, e);
		}
	}

	public void OnGameStart(DelegateArgs e) {
		if (GameStart != null) {
			GameStart(this, e);
		}
	}

	public void OnSeeCard(DelegateArgs e) {
		if (SeeCard != null) {
			SeeCard(this, e);
		}
	}

	public void OnLook(DelegateArgs e) {
		if (Look != null) {
			Look(this, e);
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

	public void OnExclusion(DelegateArgs e) {
		if (Exclusion != null) {
			Exclusion(this, e);
		}
	}

	public void OnGameOver(DelegateArgs e) {
		if (GameOver != null) {
			GameOver(this, e);
		}
	}

	public void OnPaused(DelegateArgs e) {
		if (Paused != null) {
			Paused(this, e);
		}
	}

	public void OnStart(DelegateArgs e) {
		if (Started != null) {
			Started(this, e);
		}
	}

	public void OnGameEnd(DelegateArgs e) {
		if (GameEnd != null) {
			GameEnd(this, e);
		}
	}

	public void OnTakeMore(DelegateArgs e) {
		if (TakeMore != null) {
			TakeMore(this, e);
		}
	}

	public void OnAudit(DelegateArgs e) {
		if (Audit != null) {
			Audit(this, e);
		}
	}
}

