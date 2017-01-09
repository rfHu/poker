using System;
using System.Collections.Generic;

public class DelegateArgs: EventArgs {
	public Dictionary<string, object> Data;
	public DelegateArgs(Dictionary<string, object> data) {
		Data = data;
	}
}

public class Delegates {
	public static Delegates shared = new Delegates();

	public event EventHandler<DelegateArgs> TakeSeat;
	public event EventHandler<DelegateArgs> TakeCoin;

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
}

