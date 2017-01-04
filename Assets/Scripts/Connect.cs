using BestHTTP.SocketIO;
using System;
using UnityEngine;

public class Connect  {
	private SocketManager manager;
	private string url = "http://61.143.225.47:7001/"; 

	public Connect() {
		SocketOptions options = new SocketOptions();
		manager = new SocketManager(new Uri("http://61.143.225.47:7001/"), options);
		manager.Socket.On("connect", OnConnect);
		manager.Open();	
	}

	void OnConnect(Socket socket, Packet packet, params object[] args) {
		Debug.Log("123");
	}

	void Close() {
		manager.Close();
	}

	public static Connect shared;

	public static void Setup() {
		shared = new Connect();
	}
}