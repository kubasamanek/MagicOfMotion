using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

/// <summary>
/// Manages the UDP connection for receiving RGB data, specifically handling data streaming status and ensuring continuous data reception.
/// </summary>
public class RgbConnectionManager : MonoBehaviour
{
	public static RgbConnectionManager Instance;

	public string Data { get; private set; }
	public bool IsStreaming { get; private set; }

	private Thread _receiveThread;
	private UdpClient _client;
	private int _port = 5052;
	private bool _shouldListen = true;
	private DateTime _lastReceiveTime;
	private float _timeout = 1.0f;

	void Start()
	{
		IsStreaming = false;
		Data = "";

		_receiveThread = new Thread(new ThreadStart(ListenForData));
		_receiveThread.IsBackground = true;
		_receiveThread.Start();
		_lastReceiveTime = DateTime.UtcNow;
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else if (Instance != this)
		{
			Destroy(gameObject);
		}
	}

	/// <summary>
	/// Listens for UDP data continuously until the listening is stopped. Updates data received and logs errors.
	/// </summary>
	private void ListenForData()
	{
		_client = new UdpClient(_port);
		Data = "";
		while (_shouldListen)
		{
			try
			{
				IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
				byte[] dataByte = _client.Receive(ref anyIP);
				Data = Encoding.UTF8.GetString(dataByte);
				_lastReceiveTime = DateTime.UtcNow;
			}
			catch (Exception err)
			{
				Data = "";
				Debug.LogError(err.ToString());
			}
		}
	}

	void Update()
	{
		if ((DateTime.UtcNow - _lastReceiveTime).TotalSeconds > _timeout)
		{
			if (Data != "")
			{
				Data = "";
			}
		}
		IsStreaming = Data != null && Data != "";
	}

	private void OnDestroy()
	{
		Data = "";
		_shouldListen = false;
		_client?.Close();
		_receiveThread?.Join();
	}
}
