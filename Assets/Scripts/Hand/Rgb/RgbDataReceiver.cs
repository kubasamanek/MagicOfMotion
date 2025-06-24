using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Receives UDP data packets on a specified port.
/// This class is designed to handle receiving hand tracking data from an external source asynchronously to avoid blocking the Unity main thread.
/// </summary>
public class RgbDataReceiver : MonoBehaviour
{
	private Thread _receiveThread;
	private UdpClient _client;
	private int _port = 5052;
	private bool _shouldListen = true;

	public string Data = "";
	public Action<string> OnDataReceived;

	void Start()
	{
		_receiveThread = new Thread(new ThreadStart(ListenForData));
		_receiveThread.IsBackground = true;
		_receiveThread.Start();
	}

	/// <summary>
	/// Listens for data on a separate thread to prevent blocking the Unity main thread.
	/// Data received over UDP is decoded and stored in the Data property.
	/// </summary>
	private void ListenForData()
	{
		_client = new UdpClient(_port);
		while (_shouldListen)
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] dataByte = _client.Receive(ref anyIP);
				Data = Encoding.UTF8.GetString(dataByte);
			}
			catch (Exception err)
			{
				Data = "";
				Debug.LogError(err.ToString());
			}
		}
	}

	/// <summary>
	/// Ensures the listening thread and UDP client are properly closed and disposed when the object is destroyed.
	/// </summary>
	private void OnDestroy()
	{
		Data = "";
		_shouldListen = false;
		_client?.Close();
		_receiveThread?.Join();
	}
}