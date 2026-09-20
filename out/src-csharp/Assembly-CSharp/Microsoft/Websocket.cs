using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using UnityEngine;

namespace Microsoft;

internal class Websocket : MonoBehaviour, IDisposable
{
	private delegate void OnConnectDelegate(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string connectMessage, uint connectMessageSize);

	private delegate void OnMessageDelegate(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string message, uint messageSize);

	private delegate void OnErrorDelegate(IntPtr websocketHandle, ushort errorCode, [MarshalAs(UnmanagedType.LPStr)] string errorMessage, uint errorMessageSize);

	private delegate void OnCloseDelegate(IntPtr websocketHandle, ushort code, [MarshalAs(UnmanagedType.LPStr)] string reason, uint reasonSize);

	private bool disposedValue;

	private Uri uri;

	private IntPtr websocketHandle = IntPtr.Zero;

	private Thread websocketThread;

	private object socketLock = new object();

	private static Dictionary<IntPtr, Websocket> socketsByHandle = new Dictionary<IntPtr, Websocket>();

	private Queue<object> websocketEvents = new Queue<object>();

	private Queue<object> processingEvents = new Queue<object>();

	private bool open;

	public event EventHandler OnOpen;

	public event EventHandler<MessageEventArgs> OnMessage;

	public event EventHandler<ErrorEventArgs> OnError;

	public event EventHandler<CloseEventArgs> OnClose;

	private void Update()
	{
		if (websocketEvents.Count <= 0)
		{
			return;
		}
		lock (websocketEvents)
		{
			processingEvents = websocketEvents;
			websocketEvents = new Queue<object>();
		}
		while (processingEvents.Count > 0)
		{
			object obj = processingEvents.Dequeue();
			if (obj is string)
			{
				if (!open)
				{
					open = true;
					RaiseConnect(obj as string);
				}
				else
				{
					RaiseMessage(obj as string);
				}
			}
			else if (obj is Reason)
			{
				Reason reason = obj as Reason;
				if (reason.isClose)
				{
					RaiseClose(reason.code, reason.reason);
				}
				else
				{
					RaiseError(reason.code, reason.reason);
				}
			}
		}
	}

	private void OnDestroy()
	{
		Close();
	}

	public void Open(Uri uri)
	{
		Open(uri, null);
	}

	public void Open(Uri uri, Dictionary<string, string> headers)
	{
		this.uri = uri;
		int ret = create_websocket(ref websocketHandle);
		if (ret != 0)
		{
			throw new WebsocketException("Failed to create websocket.");
		}
		socketsByHandle.Add(websocketHandle, this);
		if (headers != null)
		{
			foreach (KeyValuePair<string, string> header in headers)
			{
				ret = add_header(websocketHandle, header.Key, header.Value);
				if (ret != 0)
				{
					throw new WebsocketException($"Failed to add header [{header.Key}]: {header.Value}");
				}
			}
		}
		websocketThread = new Thread((ThreadStart)delegate
		{
			try
			{
				ret = open_websocket(websocketHandle, this.uri.AbsoluteUri, OnConnectHandler, OnMessageHandler, OnErrorHandler, OnCloseHandler);
				if (ret != 0)
				{
					throw new WebsocketException("Failed to open websocket.");
				}
			}
			catch (Exception ex)
			{
				if (ex is WebsocketException)
				{
					throw;
				}
				Debug.LogException(ex);
			}
		});
		websocketThread.Start();
	}

	public void Send(string message)
	{
		lock (socketLock)
		{
			if (websocketHandle == IntPtr.Zero)
			{
				throw new WebsocketException("Socket closed.");
			}
			int num = write_websocket(websocketHandle, message);
			if (num != 0)
			{
				throw new WebsocketException("Send failed: " + num);
			}
		}
	}

	public void Close()
	{
		Dispose();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (websocketHandle != IntPtr.Zero)
		{
			if (close_websocket(websocketHandle) != 0)
			{
				websocketThread.Abort();
			}
			if (disposing)
			{
				socketsByHandle.Remove(websocketHandle);
			}
			websocketHandle = IntPtr.Zero;
			websocketThread.Join();
		}
		disposedValue = true;
	}

	~Websocket()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	[MonoPInvokeCallback(typeof(OnConnectDelegate))]
	private static void OnConnectHandler(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string connectMessage, uint connectMessageSize)
	{
		Websocket value = null;
		socketsByHandle.TryGetValue(websocketHandle, out value);
		if (null == value)
		{
			Debug.LogError("Failed to find Websocket instance for this callback.");
			return;
		}
		lock (value.websocketEvents)
		{
			value.websocketEvents.Enqueue(connectMessage);
		}
	}

	[MonoPInvokeCallback(typeof(OnMessageDelegate))]
	private static void OnMessageHandler(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string message, uint messageSize)
	{
		Websocket value = null;
		socketsByHandle.TryGetValue(websocketHandle, out value);
		if (null == value)
		{
			Debug.LogError("Failed to find Websocket instance for this callback.");
			return;
		}
		lock (value.websocketEvents)
		{
			value.websocketEvents.Enqueue(message);
		}
	}

	[MonoPInvokeCallback(typeof(OnErrorDelegate))]
	private static void OnErrorHandler(IntPtr websocketHandle, ushort code, [MarshalAs(UnmanagedType.LPStr)] string message, uint messageSize)
	{
		Websocket value = null;
		socketsByHandle.TryGetValue(websocketHandle, out value);
		if (null == value)
		{
			Debug.LogError("Failed to find Websocket instance for this callback.");
			return;
		}
		lock (value.websocketEvents)
		{
			value.websocketEvents.Enqueue(new Reason(code, message, isClose: false));
		}
	}

	[MonoPInvokeCallback(typeof(OnCloseDelegate))]
	private static void OnCloseHandler(IntPtr websocketHandle, ushort code, [MarshalAs(UnmanagedType.LPStr)] string reason, uint reasonSize)
	{
		Websocket value = null;
		socketsByHandle.TryGetValue(websocketHandle, out value);
		if (null == value)
		{
			Debug.LogError("Failed to find Websocket instance for this callback.");
			return;
		}
		lock (value.websocketEvents)
		{
			value.websocketEvents.Enqueue(new Reason(code, reason, isClose: false));
		}
	}

	private void RaiseConnect([MarshalAs(UnmanagedType.LPStr)] string connectMessage)
	{
		if (!disposedValue)
		{
			this.OnOpen?.Invoke(this, null);
		}
	}

	private void RaiseMessage([MarshalAs(UnmanagedType.LPStr)] string message)
	{
		if (!disposedValue)
		{
			this.OnMessage?.Invoke(this, new MessageEventArgs(message));
		}
	}

	private void RaiseError(ushort errorCode, string errorMessage)
	{
		if (!disposedValue)
		{
			this.OnError?.Invoke(this, new ErrorEventArgs(errorCode, errorMessage));
		}
	}

	private void RaiseClose(ushort code, [MarshalAs(UnmanagedType.LPStr)] string reason)
	{
		if (!disposedValue)
		{
			this.OnClose?.Invoke(this, new CloseEventArgs(code, reason));
		}
	}

	[DllImport("simplewebsocket")]
	private static extern int create_websocket(ref IntPtr websocketHandlePtr);

	[DllImport("simplewebsocket")]
	private static extern int add_header(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string key, [MarshalAs(UnmanagedType.LPStr)] string value);

	[DllImport("simplewebsocket")]
	private static extern int open_websocket(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string uri, OnConnectDelegate onConnect, OnMessageDelegate onMessage, OnErrorDelegate onError, OnCloseDelegate onClose);

	[DllImport("simplewebsocket")]
	private static extern int write_websocket(IntPtr websocketHandle, [MarshalAs(UnmanagedType.LPStr)] string message);

	[DllImport("simplewebsocket")]
	private static extern int read_websocket(IntPtr websocketHandle, OnMessageDelegate onMessage);

	[DllImport("simplewebsocket")]
	private static extern int close_websocket(IntPtr websocketHandle);
}
