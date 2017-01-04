using BestHTTP.SocketIO;
using System;

public class Connect  {
	private SocketManager manager;
	private string url = "http://61.143.225.47:7001/"; 

	Connect() {
		// Sock
		manager = new SocketManager(new Uri("http://61.143.225.47:7001/"), );
		// manager = new SocketManager(new );
	}
}
