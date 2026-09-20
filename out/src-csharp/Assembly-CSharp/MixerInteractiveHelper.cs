using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

internal class MixerInteractiveHelper : MonoBehaviour
{
	public delegate void OnInternalWebRequestStateChangedEventHandler(object sender, _InternalWebRequestStateChangedEventArgs e);

	public delegate void OnInternalCheckAuthStatusCallbackEventHandler(object sender, InternalTimerCallbackEventArgs e);

	public delegate void OnInternalRefreshShortCodeCallbackEventHandler(object sender, InternalTimerCallbackEventArgs e);

	public delegate void OnInternalReconnectCallbackEventHandler(object sender, InternalTimerCallbackEventArgs e);

	public delegate void OnTryGetAuthTokensFromCacheCallbackEventHandler(object sender, TryGetAuthTokensFromCacheEventArgs e);

	internal struct _InteractiveWebRequestData
	{
		public string requestID;

		public string requestUrl;

		public Dictionary<string, string> headers;

		public string httpVerb;

		public string postData;

		public _InteractiveWebRequestData(string newRequestID, string newRequestUrl, Dictionary<string, string> newHeaders, string newHttpVerb, string newPostData)
		{
			requestID = newRequestID;
			requestUrl = newRequestUrl;
			headers = newHeaders;
			httpVerb = newHttpVerb;
			postData = newPostData;
		}
	}

	internal class _InternalWebRequestStateChangedEventArgs
	{
		public string RequestID { get; private set; }

		public bool Succeeded { get; private set; }

		public long ResponseCode { get; private set; }

		public string ResponseText { get; private set; }

		public string ErrorMessage { get; private set; }

		internal _InternalWebRequestStateChangedEventArgs(string requestID, bool succeeded, long responseCode, string responseText, string errorMessage)
		{
			RequestID = requestID;
			Succeeded = succeeded;
			ResponseCode = responseCode;
			ResponseText = responseText;
			ErrorMessage = errorMessage;
		}
	}

	internal enum InteractiveTimerType
	{
		CheckAuthStatus,
		RefreshShortCode,
		Reconnect
	}

	internal struct InteractiveTimerData
	{
		public InteractiveTimerType type;

		public float interval;

		public InteractiveTimerData(InteractiveTimerType newType, float newInterval)
		{
			type = newType;
			interval = newInterval;
		}
	}

	internal class InternalTimerCallbackEventArgs
	{
	}

	internal class TryGetAuthTokensFromCacheEventArgs
	{
		public string AuthToken;

		public string RefreshToken;
	}

	private struct CoRoutineInfo
	{
		public string name;

		public Coroutine coRoutine;

		public CoRoutineInfo(string newName, Coroutine newCoRoutine)
		{
			name = newName;
			coRoutine = newCoRoutine;
		}
	}

	internal bool _runInBackgroundIfInteractive = true;

	internal string _defaultSceneID;

	internal Dictionary<string, string> _groupSceneMapping = new Dictionary<string, string>();

	internal List<string> rpcOwningMonoBehaviorNames = new List<string>();

	internal List<string> rpcMethodNames = new List<string>();

	internal Dictionary<string, MixerInteractive.RpcCachedMethodInfo> cachedRPCMethods = new Dictionary<string, MixerInteractive.RpcCachedMethodInfo>();

	private List<_InteractiveWebRequestData> _queuedWebRequests;

	private List<InteractiveTimerData> _queuedStartTimerRequests;

	private List<InteractiveTimerType> _queuedStopTimerRequests;

	private List<CoRoutineInfo> _runningCoRoutines;

	private bool _queuedTryGetAuthTokensFromCacheRequest;

	private bool _queuedWriteAuthTokensToCacheRequest;

	private string _authTokenValueToWriteToCache;

	private string _refreshTokenValueToWriteToCache;

	private static MixerInteractiveHelper _singletonInstance;

	internal static MixerInteractiveHelper _SingletonInstance
	{
		get
		{
			if (_singletonInstance == null)
			{
				MixerInteractiveHelper[] array = Object.FindObjectsOfType<MixerInteractiveHelper>();
				if (array.Length != 0)
				{
					_singletonInstance = array[0];
				}
				_singletonInstance.Initialize();
			}
			return _singletonInstance;
		}
	}

	public event OnInternalWebRequestStateChangedEventHandler OnInternalWebRequestStateChanged;

	public event OnInternalCheckAuthStatusCallbackEventHandler OnInternalCheckAuthStatusTimerCallback;

	public event OnInternalRefreshShortCodeCallbackEventHandler OnInternalRefreshShortCodeTimerCallback;

	public event OnInternalReconnectCallbackEventHandler OnInternalReconnectTimerCallback;

	public event OnTryGetAuthTokensFromCacheCallbackEventHandler OnTryGetAuthTokensFromCacheCallback;

	private void Update()
	{
		if (!(_singletonInstance != null))
		{
			return;
		}
		foreach (_InteractiveWebRequestData queuedWebRequest in _queuedWebRequests)
		{
			_runningCoRoutines.Add(new CoRoutineInfo("MakeWebRequestCoRoutine", StartCoroutine(MakeWebRequestCoRoutine(queuedWebRequest.requestID, queuedWebRequest.requestUrl, queuedWebRequest.headers, queuedWebRequest.httpVerb, queuedWebRequest.postData))));
		}
		_queuedWebRequests.Clear();
		foreach (InteractiveTimerData queuedStartTimerRequest in _queuedStartTimerRequests)
		{
			InteractiveTimerType type = queuedStartTimerRequest.type;
			float interval = queuedStartTimerRequest.interval;
			switch (type)
			{
			case InteractiveTimerType.CheckAuthStatus:
				StopCoroutineByName("CheckAuthStatusCoRoutine");
				_runningCoRoutines.Add(new CoRoutineInfo("CheckAuthStatusCoRoutine", StartCoroutine(CheckAuthStatusCoRoutine(interval))));
				break;
			case InteractiveTimerType.RefreshShortCode:
				StopCoroutineByName("RefreshShortCodeCoRoutine");
				_runningCoRoutines.Add(new CoRoutineInfo("RefreshShortCodeCoRoutine", StartCoroutine(RefreshShortCodeCoRoutine(interval))));
				break;
			case InteractiveTimerType.Reconnect:
				StopCoroutineByName("ReconnectCodeCoRoutine");
				_runningCoRoutines.Add(new CoRoutineInfo("ReconnectCodeCoRoutine", StartCoroutine(ReconnectCodeCoRoutine(interval))));
				break;
			}
		}
		_queuedStartTimerRequests.Clear();
		using (List<InteractiveTimerType>.Enumerator enumerator3 = _queuedStopTimerRequests.GetEnumerator())
		{
			while (enumerator3.MoveNext())
			{
				switch (enumerator3.Current)
				{
				case InteractiveTimerType.CheckAuthStatus:
					StopCoroutineByName("CheckAuthStatusCoRoutine");
					break;
				case InteractiveTimerType.RefreshShortCode:
					StopCoroutineByName("RefreshShortCodeCoRoutine");
					break;
				case InteractiveTimerType.Reconnect:
					StopCoroutineByName("ReconnectCodeCoRoutine");
					break;
				}
			}
		}
		_queuedStopTimerRequests.Clear();
		if (_queuedTryGetAuthTokensFromCacheRequest)
		{
			TryGetAuthTokensFromCacheEventArgs tryGetAuthTokensFromCacheEventArgs = new TryGetAuthTokensFromCacheEventArgs();
			tryGetAuthTokensFromCacheEventArgs.AuthToken = PlayerPrefs.GetString("MixerInteractive-AuthToken");
			tryGetAuthTokensFromCacheEventArgs.RefreshToken = PlayerPrefs.GetString("MixerInteractive-RefreshToken");
			if (this.OnTryGetAuthTokensFromCacheCallback != null)
			{
				this.OnTryGetAuthTokensFromCacheCallback(this, tryGetAuthTokensFromCacheEventArgs);
			}
			_queuedTryGetAuthTokensFromCacheRequest = false;
		}
		if (_queuedWriteAuthTokensToCacheRequest)
		{
			WriteAuthTokensToCacheImpl();
		}
	}

	private void StopCoroutineByName(string name)
	{
		foreach (CoRoutineInfo runningCoRoutine in _runningCoRoutines)
		{
			if (runningCoRoutine.name == name)
			{
				StopCoroutine(runningCoRoutine.coRoutine);
			}
		}
	}

	private void Initialize()
	{
		_queuedWebRequests = new List<_InteractiveWebRequestData>();
		_queuedStartTimerRequests = new List<InteractiveTimerData>();
		_queuedStopTimerRequests = new List<InteractiveTimerType>();
		_runningCoRoutines = new List<CoRoutineInfo>();
	}

	internal void _MakeWebRequest(string requestID, string requestUrl, Dictionary<string, string> headers = null, string httpVerb = "", string postData = "")
	{
		_queuedWebRequests.Add(new _InteractiveWebRequestData(requestID, requestUrl, headers, httpVerb, postData));
	}

	internal void StartTimer(InteractiveTimerType type, float interval)
	{
		_queuedStartTimerRequests.Add(new InteractiveTimerData(type, interval));
	}

	internal void StopTimer(InteractiveTimerType type)
	{
		_queuedStopTimerRequests.Add(type);
	}

	internal void StartTryGetAuthTokensFromCache()
	{
		_queuedTryGetAuthTokensFromCacheRequest = true;
	}

	internal void WriteAuthTokensToCache(string authToken, string refreshToken)
	{
		_queuedWriteAuthTokensToCacheRequest = true;
		_authTokenValueToWriteToCache = authToken;
		_refreshTokenValueToWriteToCache = refreshToken;
	}

	private void WriteAuthTokensToCacheImpl()
	{
		_queuedWriteAuthTokensToCacheRequest = false;
		PlayerPrefs.SetString("MixerInteractive-AuthToken", _authTokenValueToWriteToCache);
		PlayerPrefs.SetString("MixerInteractive-RefreshToken", _refreshTokenValueToWriteToCache);
		PlayerPrefs.Save();
	}

	private IEnumerator MakeWebRequestCoRoutine(string requestID, string requestUrl, Dictionary<string, string> headers, string httpVerb, string postData)
	{
		UnityWebRequest request;
		if (httpVerb == "POST")
		{
			UploadHandlerRaw uploadHandler = new UploadHandlerRaw(new ASCIIEncoding().GetBytes(postData));
			request = UnityWebRequest.Post(requestUrl, postData);
			request.uploadHandler = uploadHandler;
			request.SetRequestHeader("Content-Type", "application/json");
		}
		else
		{
			request = UnityWebRequest.Get(requestUrl);
		}
		if (headers != null)
		{
			foreach (string key in headers.Keys)
			{
				request.SetRequestHeader(key, headers[key]);
			}
		}
		yield return request.SendWebRequest();
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		backgroundWorker.DoWork -= WebRequestBackgroundWorkerDoWork;
		backgroundWorker.DoWork += WebRequestBackgroundWorkerDoWork;
		backgroundWorker.RunWorkerAsync(new _InternalWebRequestStateChangedEventArgs(requestID, !request.isNetworkError, request.responseCode, request.downloadHandler.text, request.error));
		request.Dispose();
	}

	private void WebRequestBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
	{
		if (sender is BackgroundWorker backgroundWorker)
		{
			backgroundWorker.DoWork -= WebRequestBackgroundWorkerDoWork;
			if (e.Argument is _InternalWebRequestStateChangedEventArgs e2 && this.OnInternalWebRequestStateChanged != null)
			{
				this.OnInternalWebRequestStateChanged(this, e2);
			}
		}
	}

	private IEnumerator CheckAuthStatusCoRoutine(float interval)
	{
		while (true)
		{
			yield return new WaitForSeconds(interval);
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			backgroundWorker.DoWork -= CheckAuthStatusBackgroundWorkerDoWork;
			backgroundWorker.DoWork += CheckAuthStatusBackgroundWorkerDoWork;
			backgroundWorker.RunWorkerAsync();
		}
	}

	private IEnumerator RefreshShortCodeCoRoutine(float interval)
	{
		while (true)
		{
			yield return new WaitForSeconds(interval);
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			backgroundWorker.DoWork -= RefreshShortCodeBackgroundWorkerDoWork;
			backgroundWorker.DoWork += RefreshShortCodeBackgroundWorkerDoWork;
			backgroundWorker.RunWorkerAsync();
		}
	}

	private IEnumerator ReconnectCodeCoRoutine(float interval)
	{
		while (true)
		{
			yield return new WaitForSeconds(interval);
			BackgroundWorker backgroundWorker = new BackgroundWorker();
			backgroundWorker.DoWork -= OnInternalReconnectBackgroundWorkerDoWork;
			backgroundWorker.DoWork += OnInternalReconnectBackgroundWorkerDoWork;
			backgroundWorker.RunWorkerAsync();
		}
	}

	private void CheckAuthStatusBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
	{
		if (sender is BackgroundWorker backgroundWorker)
		{
			backgroundWorker.DoWork -= CheckAuthStatusBackgroundWorkerDoWork;
			if (this.OnInternalCheckAuthStatusTimerCallback != null)
			{
				this.OnInternalCheckAuthStatusTimerCallback(this, new InternalTimerCallbackEventArgs());
			}
		}
	}

	private void RefreshShortCodeBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
	{
		if (sender is BackgroundWorker backgroundWorker)
		{
			backgroundWorker.DoWork -= RefreshShortCodeBackgroundWorkerDoWork;
			if (this.OnInternalRefreshShortCodeTimerCallback != null)
			{
				this.OnInternalRefreshShortCodeTimerCallback(this, new InternalTimerCallbackEventArgs());
			}
		}
	}

	private void OnInternalReconnectBackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
	{
		if (sender is BackgroundWorker backgroundWorker)
		{
			backgroundWorker.DoWork -= OnInternalReconnectBackgroundWorkerDoWork;
			if (this.OnInternalReconnectTimerCallback != null)
			{
				this.OnInternalReconnectTimerCallback(this, new InternalTimerCallbackEventArgs());
			}
		}
	}

	public void Dispose()
	{
		StopAllCoroutines();
	}
}
