using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

namespace Microsoft.Mixer;

public class InteractivityManager : IDisposable
{
	public delegate void OnErrorEventHandler(object sender, InteractiveEventArgs e);

	public delegate void OnInteractivityStateChangedHandler(object sender, InteractivityStateChangedEventArgs e);

	public delegate void OnParticipantStateChangedHandler(object sender, InteractiveParticipantStateChangedEventArgs e);

	public delegate void OnInteractiveButtonEventHandler(object sender, InteractiveButtonEventArgs e);

	public delegate void OnInteractiveJoystickControlEventHandler(object sender, InteractiveJoystickEventArgs e);

	public delegate void OnInteractiveMouseButtonEventHandler(object sender, InteractiveMouseButtonEventArgs e);

	public delegate void OnInteractiveCoordinatesChangedHandler(object sender, InteractiveCoordinatesChangedEventArgs e);

	internal delegate void OnInteractiveTextControlEventHandler(object sender, InteractiveTextEventArgs e);

	public delegate void OnInteractiveMessageEventHandler(object sender, InteractiveMessageEventArgs e);

	public delegate void OnInteractiveDoWorkEventHandler(object sender, InteractiveEventArgs e);

	internal struct _InputEvent
	{
		internal string ControlID;

		internal string Kind;

		internal string Event;

		internal InteractiveEventType Type;

		internal uint Cost;

		internal bool IsPressed;

		internal string TransactionID;

		internal float X;

		internal float Y;

		internal string TextValue;

		internal _InputEvent(string controlID, string kind, string eventName, InteractiveEventType type, bool isPressed, float x, float y, uint cost, string transactionID, string textValue)
		{
			ControlID = controlID;
			Kind = kind;
			Event = eventName;
			Type = type;
			Cost = cost;
			TransactionID = transactionID;
			IsPressed = isPressed;
			X = x;
			Y = y;
			TextValue = textValue;
		}
	}

	private static InteractivityManager _singletonInstance;

	private List<InteractiveEventArgs> _queuedEvents = new List<InteractiveEventArgs>();

	private Dictionary<uint, string> _outstandingMessages = new Dictionary<uint, string>();

	private WebSocket _websocket;

	private string _interactiveWebSocketUrl = string.Empty;

	private uint _currentmessageID = 1u;

	private bool _disposed;

	private string _authShortCodeRequestHandle;

	internal string _authToken;

	private string _oauthRefreshToken;

	private bool _initializedGroups;

	private bool _initializedScenes;

	private bool _pendingConnectToWebSocket;

	private bool _websocketConnected;

	private bool _shouldStartInteractive = true;

	private string _streamingAssetsPath = string.Empty;

	private List<InteractiveGroup> _groups;

	private List<InteractiveScene> _scenes;

	private List<InteractiveParticipant> _participants;

	private List<InteractiveControl> _controls;

	private List<InteractiveButtonControl> _buttons;

	private List<InteractiveJoystickControl> _joysticks;

	private List<string> _websocketHosts;

	private int _activeWebsocketHostIndex;

	private MixerInteractiveHelper mixerInteractiveHelper;

	private const string API_BASE = "https://mixer.com/api/v1/";

	private const string WEBSOCKET_DISCOVERY_URL = "https://mixer.com/api/v1/interactive/hosts";

	private const string API_CHECK_SHORT_CODE_AUTH_STATUS_PATH = "https://mixer.com/api/v1/oauth/shortcode/check/";

	private const string API_GET_SHORT_CODE_PATH = "https://mixer.com/api/v1/oauth/shortcode";

	private const string API_GET_OAUTH_TOKEN_PATH = "https://mixer.com/api/v1/oauth/token";

	private const string INTERACTIVE_DATA_FILE_NAME = "interactivedata.json";

	private const string CONFIG_FILE_NAME = "interactiveconfig.json";

	private const float POLL_FOR_SHORT_CODE_AUTH_INTERVAL = 0.5f;

	private const float WEBSOCKET_RECONNECT_INTERVAL = 0.5f;

	private const string INTERACTIVE_CONFIG_FILE_NAME = "interactiveconfig.json";

	private const string WS_MESSAGE_KEY_ACCESS_TOKEN_FROM_FILE = "AuthToken";

	private const string WS_MESSAGE_KEY_APPID = "appid";

	private const string WS_MESSAGE_KEY_CHANNEL_GROUPS = "channelGroups";

	private const string WS_MESSAGE_KEY_CODE = "code";

	private const string WS_MESSAGE_KEY_COOLDOWN = "cooldown";

	private const string WS_MESSAGE_KEY_CONNECTED_AT = "connectedAt";

	private const string WS_MESSAGE_KEY_CONTROLS = "controls";

	private const string WS_MESSAGE_KEY_CONTROL_ID = "controlID";

	internal const string _WS_MESSAGE_KEY_COST = "cost";

	private const string WS_MESSAGE_KEY_DISABLED = "disabled";

	private const string WS_MESSAGE_KEY_ERROR_CODE = "code";

	private const string WS_MESSAGE_KEY_ERROR_MESSAGE = "message";

	private const string WS_MESSAGE_KEY_ERROR_PATH = "path";

	private const string WS_MESSAGE_KEY_ETAG = "etag";

	private const string WS_MESSAGE_KEY_EVENT = "event";

	private const string WS_MESSAGE_KEY_EXPIRATION = "expires_in";

	private const string WS_MESSAGE_KEY_GROUP = "group";

	private const string WS_MESSAGE_KEY_GROUPS = "groups";

	private const string WS_MESSAGE_KEY_GROUP_ID = "groupID";

	private const string WS_MESSAGE_KEY_LAST_INPUT_AT = "lastInputAt";

	private const string WS_MESSAGE_KEY_HANDLE = "handle";

	private const string WS_MESSAGE_KEY_ID = "id";

	private const string WS_MESSAGE_KEY_INPUT = "input";

	private const string WS_MESSAGE_KEY_INTENSITY = "intensity";

	private const string WS_MESSAGE_KEY_ISREADY = "isReady";

	private const string WS_MESSAGE_KEY_KIND = "kind";

	private const string WS_MESSAGE_KEY_LEVEL = "level";

	private const string WS_MESSAGE_KEY_REFRESH_TOKEN = "refresh_token";

	private const string WS_MESSAGE_KEY_REFRESH_TOKEN_FROM_FILE = "RefreshToken";

	private const string WS_MESSAGE_KEY_META = "meta";

	private const string WS_MESSAGE_KEY_PARTICIPANT_ID = "participantID";

	private const string WS_MESSAGE_KEY_PARTICIPANTS = "participants";

	private const string WS_MESSAGE_KEY_PARAMETERS = "params";

	internal const string _WS_MESSAGE_KEY_PROGRESS = "progress";

	private const string WS_MESSAGE_KEY_PROJECT_VERSION_ID = "projectversionid";

	private const string WS_MESSAGE_KEY_RESULT = "result";

	private const string WS_MESSAGE_KEY_SCENE_ID = "sceneID";

	private const string WS_MESSAGE_KEY_SCENES = "scenes";

	private const string WS_MESSAGE_KEY_SCHEME = "scheme";

	private const string WS_MESSAGE_KEY_SESSION_ID = "sessionID";

	private const string WS_MESSAGE_KEY_PROJECT_SHARE_CODE = "sharecode";

	internal const string _WS_MESSAGE_KEY_TEXT = "text";

	private const string WS_MESSAGE_KEY_TRANSACTION_ID = "transactionID";

	private const string WS_MESSAGE_KEY_TYPE = "type";

	private const string WS_MESSAGE_KEY_USER_ID = "userID";

	private const string WS_MESSAGE_KEY_USERNAME = "username";

	private const string WS_MESSAGE_KEY_VALUE = "value";

	private const string WS_MESSAGE_KEY_WEBSOCKET_ACCESS_TOKEN = "access_token";

	private const string WS_MESSAGE_KEY_WEBSOCKET_ADDRESS = "address";

	private const string WS_MESSAGE_KEY_X = "x";

	private const string WS_MESSAGE_KEY_Y = "y";

	internal const string _WS_MESSAGE_VALUE_CONTROL_TYPE_BUTTON = "button";

	internal const string _WS_MESSAGE_VALUE_DISABLED = "disabled";

	internal const string _WS_MESSAGE_VALUE_DEFAULT_GROUP_ID = "default";

	internal const string _WS_MESSAGE_VALUE_DEFAULT_SCENE_ID = "default";

	internal const string _WS_MESSAGE_VALUE_CONTROL_TYPE_JOYSTICK = "joystick";

	internal const string _WS_MESSAGE_VALUE_CONTROL_TYPE_LABEL = "label";

	internal const string _WS_MESSAGE_VALUE_CONTROL_TYPE_TEXTBOX = "textbox";

	private const bool WS_MESSAGE_VALUE_TRUE = true;

	private const string WS_MESSAGE_TYPE_METHOD = "method";

	private const string WS_MESSAGE_TYPE_REPLY = "reply";

	private const string WS_MESSAGE_METHOD_CREATE_GROUPS = "createGroups";

	private const string WS_MESSAGE_METHOD_GET_ALL_PARTICIPANTS = "getAllParticipants";

	private const string WS_MESSAGE_METHOD_GET_GROUPS = "getGroups";

	private const string WS_MESSAGE_METHOD_GET_SCENES = "getScenes";

	private const string WS_MESSAGE_METHOD_GIVE_INPUT = "giveInput";

	private const string WS_MESSAGE_METHOD_HELLO = "hello";

	private const string WS_MESSAGE_METHOD_PARTICIPANT_JOIN = "onParticipantJoin";

	private const string WS_MESSAGE_METHOD_PARTICIPANT_LEAVE = "onParticipantLeave";

	private const string WS_MESSAGE_METHOD_PARTICIPANT_UPDATE = "onParticipantUpdate";

	private const string WS_MESSAGE_METHOD_READY = "ready";

	private const string WS_MESSAGE_METHOD_ON_CONTROL_UPDATE = "onControlUpdate";

	private const string WS_MESSAGE_METHOD_ON_CONTROL_CREATE = "onControlCreate";

	private const string WS_MESSAGE_METHOD_ON_GROUP_CREATE = "onGroupCreate";

	private const string WS_MESSAGE_METHOD_ON_GROUP_UPDATE = "onGroupUpdate";

	private const string WS_MESSAGE_METHOD_ON_READY = "onReady";

	private const string WS_MESSAGE_METHOD_ON_SCENE_CREATE = "onSceneCreate";

	private const string WS_MESSAGE_METHOD_SET_CAPTURE_TRANSACTION = "capture";

	private const string WS_MESSAGE_METHOD_SET_COMPRESSION = "setCompression";

	private const string WS_MESSAGE_METHOD_SET_CONTROL_FIRED = "setControlFired";

	private const string WS_MESSAGE_METHOD_SET_JOYSTICK_COORDINATES = "setJoystickCoordinates";

	private const string WS_MESSAGE_METHOD_SET_JOYSTICK_INTENSITY = "setJoystickIntensity";

	private const string WS_MESSAGE_METHOD_SET_BUTTON_CONTROL_PROPERTIES = "setButtonControlProperties";

	private const string WS_MESSAGE_METHOD_SET_CONTROL_TEXT = "setControlText";

	private const string WS_MESSAGE_METHOD_SET_CURRENT_SCENE = "setCurrentScene";

	private const string WS_MESSAGE_METHOD_UPDATE_CONTROLS = "updateControls";

	private const string WS_MESSAGE_METHOD_UPDATE_GROUPS = "updateGroups";

	private const string WS_MESSAGE_METHOD_UPDATE_PARTICIPANTS = "updateParticipants";

	private const string WS_MESSAGE_METHOD_UPDATE_SCENES = "updateScenes";

	private const string WS_MESSAGE_ERROR = "error";

	internal const string _CONTROL_TYPE_BUTTON = "button";

	internal const string _CONTROL_TYPE_JOYSTICK = "joystick";

	internal const string _CONTROL_KIND_LABEL = "label";

	internal const string _CONTROL_KIND_TEXTBOX = "textbox";

	internal const string _CONTROL_KIND_SCREEN = "screen";

	private const string EVENT_NAME_MOUSE_DOWN = "mousedown";

	private const string EVENT_NAME_MOUSE_UP = "mouseup";

	private const string EVENT_NAME_KEY_DOWN = "keydown";

	private const string EVENT_NAME_KEY_UP = "keyup";

	private const string EVENT_NAME_MOVE = "move";

	private const string EVENT_NAME_SUBMIT = "submit";

	private const string BOOLEAN_TRUE_VALUE = "true";

	private const string COMPRESSION_TYPE_GZIP = "gzip";

	private const string READY_PARAMETER_IS_READY = "isReady";

	private int ERROR_FAIL = 83;

	private const string PROTOCOL_VERSION = "2.0";

	internal static Dictionary<string, _InternalButtonCountState> _buttonStates;

	internal static Dictionary<uint, Dictionary<string, _InternalButtonState>> _buttonStatesByParticipant;

	internal static Dictionary<string, _InternalJoystickState> _joystickStates;

	internal static Dictionary<uint, Dictionary<string, _InternalJoystickState>> _joystickStatesByParticipant;

	internal static Dictionary<uint, Dictionary<string, string>> _textboxValuesByParticipant;

	internal static Dictionary<uint, _InternalMouseButtonState> _mouseButtonStateByParticipant;

	internal static Dictionary<uint, Vector2> _mousePositionsByParticipant;

	internal static Dictionary<string, Dictionary<uint, Dictionary<string, object>>> _giveInputControlDataByParticipant;

	internal static Dictionary<string, Dictionary<string, object>> _giveInputControlData;

	internal static Dictionary<string, object> _giveInputKeyValues;

	internal static Dictionary<string, _InternalParticipantTrackingState> _participantsWhoTriggeredGiveInput;

	private static Dictionary<string, Dictionary<string, _InternalControlPropertyUpdateData>> _queuedControlPropertyUpdates;

	internal static Dictionary<string, InternalTransactionIDState> _transactionIDsState;

	public static bool useMockData;

	public static InteractivityManager SingletonInstance
	{
		get
		{
			if (_singletonInstance == null)
			{
				_singletonInstance = new InteractivityManager();
				_singletonInstance.InitializeInternal();
			}
			return _singletonInstance;
		}
	}

	public LoggingLevel LoggingLevel { get; set; }

	private string ProjectVersionID { get; set; }

	private string AppID { get; set; }

	private string ShareCode { get; set; }

	public InteractivityState InteractivityState { get; private set; }

	public IList<InteractiveGroup> Groups => new List<InteractiveGroup>(_groups);

	public IList<InteractiveScene> Scenes => new List<InteractiveScene>(_scenes);

	public IList<InteractiveParticipant> Participants => new List<InteractiveParticipant>(_participants);

	internal IList<InteractiveControl> _Controls => new List<InteractiveControl>(_controls);

	public IList<InteractiveButtonControl> Buttons => new List<InteractiveButtonControl>(_buttons);

	public IList<InteractiveJoystickControl> Joysticks => new List<InteractiveJoystickControl>(_joysticks);

	public string ShortCode { get; private set; }

	public event OnErrorEventHandler OnError;

	public event OnInteractivityStateChangedHandler OnInteractivityStateChanged;

	public event OnParticipantStateChangedHandler OnParticipantStateChanged;

	public event OnInteractiveButtonEventHandler OnInteractiveButtonEvent;

	public event OnInteractiveJoystickControlEventHandler OnInteractiveJoystickControlEvent;

	public event OnInteractiveMouseButtonEventHandler OnInteractiveMouseButtonEvent;

	public event OnInteractiveCoordinatesChangedHandler OnInteractiveCoordinatesChangedEvent;

	internal event OnInteractiveTextControlEventHandler OnInteractiveTextControlEvent;

	public event OnInteractiveMessageEventHandler OnInteractiveMessageEvent;

	public event OnInteractiveDoWorkEventHandler OnInteractiveDoWorkEvent;

	public InteractiveGroup GetGroup(string groupID)
	{
		foreach (InteractiveGroup group in _groups)
		{
			if (group.GroupID == groupID)
			{
				return group;
			}
		}
		return null;
	}

	public InteractiveScene GetScene(string sceneID)
	{
		foreach (InteractiveScene scene in Scenes)
		{
			if (scene.SceneID == sceneID)
			{
				return scene;
			}
		}
		return null;
	}

	public void Initialize(bool goInteractive = true, string authToken = "")
	{
		if (InteractivityState == InteractivityState.NotInitialized)
		{
			ResetInternalState();
			UpdateInteractivityState(InteractivityState.Initializing);
			if (goInteractive)
			{
				_shouldStartInteractive = true;
			}
			if (!string.IsNullOrEmpty(authToken))
			{
				_authToken = authToken;
			}
			InitiateConnection();
		}
	}

	private void CreateStorageDirectoryIfNotExists()
	{
	}

	private void getWebsocketHosts(string potentialWebsocketUrlsJson)
	{
		_websocketHosts.Clear();
		_activeWebsocketHostIndex = 0;
		string empty = string.Empty;
		using (StringReader reader = new StringReader(potentialWebsocketUrlsJson))
		{
			using JsonTextReader jsonTextReader = new JsonTextReader(reader);
			while (jsonTextReader.Read())
			{
				if (jsonTextReader.Value != null && jsonTextReader.Value.ToString() == "address")
				{
					jsonTextReader.Read();
					empty = jsonTextReader.Value.ToString();
					_websocketHosts.Add(empty);
				}
			}
		}
		_interactiveWebSocketUrl = _websocketHosts[_activeWebsocketHostIndex];
	}

	internal void SetWebsocketInstance(Websocket newWebsocket)
	{
	}

	private void InitiateConnection()
	{
		try
		{
			mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestWebSocketHostsCompleted;
			mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestWebSocketHostsCompleted;
			mixerInteractiveHelper._MakeWebRequest("OnRequestWebSocketHostsCompleted", "https://mixer.com/api/v1/interactive/hosts");
		}
		catch (Exception ex)
		{
			_LogError("Error: Could not retrieve the URL for the websocket. Exception details: " + ex.Message);
		}
	}

	private void CompleteInitiateConnection(string websocketHostsResponseString)
	{
		getWebsocketHosts(websocketHostsResponseString);
		if (string.IsNullOrEmpty(ProjectVersionID) || (string.IsNullOrEmpty(AppID) && string.IsNullOrEmpty(ShareCode)))
		{
			PopulateConfigData();
		}
		if (!string.IsNullOrEmpty(_authToken))
		{
			VerifyAuthToken();
			return;
		}
		mixerInteractiveHelper.OnTryGetAuthTokensFromCacheCallback -= OnTryGetAuthTokensFromCacheCallback;
		mixerInteractiveHelper.OnTryGetAuthTokensFromCacheCallback += OnTryGetAuthTokensFromCacheCallback;
		mixerInteractiveHelper.StartTryGetAuthTokensFromCache();
	}

	private void OnTryGetAuthTokensFromCacheCallback(object sender, MixerInteractiveHelper.TryGetAuthTokensFromCacheEventArgs e)
	{
		mixerInteractiveHelper.OnTryGetAuthTokensFromCacheCallback -= OnTryGetAuthTokensFromCacheCallback;
		OnTryGetAuthTokensFromCacheCompleted(e);
	}

	private void OnTryGetAuthTokensFromCacheCompleted(MixerInteractiveHelper.TryGetAuthTokensFromCacheEventArgs e)
	{
		_authToken = e.AuthToken;
		_oauthRefreshToken = e.RefreshToken;
		if (!string.IsNullOrEmpty(_authToken))
		{
			VerifyAuthToken();
		}
		else
		{
			RefreshShortCode();
		}
	}

	private void OnRequestWebSocketHostsCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
	{
		if (!(e.RequestID != "OnRequestWebSocketHostsCompleted"))
		{
			mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestWebSocketHostsCompleted;
			if (e.Succeeded)
			{
				CompleteInitiateConnection(e.ResponseText);
			}
			else
			{
				_LogError("Error: Could not retrieve the URL for the websocket. Exception details: " + e.ErrorMessage);
			}
		}
	}

	private void PopulateConfigData()
	{
		string empty = string.Empty;
		empty = _streamingAssetsPath + "/interactiveconfig.json";
		if (File.Exists(empty))
		{
			string s = File.ReadAllText(empty);
			try
			{
				using StringReader reader = new StringReader(s);
				using JsonTextReader jsonTextReader = new JsonTextReader(reader);
				while (jsonTextReader.Read())
				{
					if (jsonTextReader.Value == null)
					{
						continue;
					}
					switch (jsonTextReader.Value.ToString().ToLowerInvariant())
					{
					case "appid":
						jsonTextReader.Read();
						if (jsonTextReader.Value != null)
						{
							AppID = jsonTextReader.Value.ToString();
						}
						break;
					case "projectversionid":
						jsonTextReader.Read();
						if (jsonTextReader.Value != null)
						{
							ProjectVersionID = jsonTextReader.Value.ToString();
						}
						break;
					case "sharecode":
						jsonTextReader.Read();
						if (jsonTextReader.Value != null)
						{
							ShareCode = jsonTextReader.Value.ToString();
						}
						break;
					}
				}
				return;
			}
			catch
			{
				_LogError("Error: interactiveconfig.json file could not be read. Make sure it is valid JSON and has the correct format.");
				return;
			}
		}
		throw new Exception("Error: You need to specify an AppID and ProjectVersionID in the Interactive Editor. You can get to the Interactivity Editor from the Mixer menu (Mixer > Open editor).");
	}

	private void OnInternalCheckAuthStatusTimerCallback(object sender, MixerInteractiveHelper.InternalTimerCallbackEventArgs e)
	{
		TryGetTokenAsync();
	}

	private void TryGetTokenAsync()
	{
		_Log("Trying to obtain a new OAuth token. This is an expected and repeated call.");
		mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestOAuthExchangeTokenCompleted;
		mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestOAuthExchangeTokenCompleted;
		mixerInteractiveHelper._MakeWebRequest("OnRequestOAuthExchangeTokenCompleted", "https://mixer.com/api/v1/oauth/shortcode/check/" + _authShortCodeRequestHandle, new Dictionary<string, string> { { "Content-Type", "application/json" } });
	}

	private void OnRequestOAuthExchangeTokenCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
	{
		if (!(e.RequestID != "OnRequestOAuthExchangeTokenCompleted"))
		{
			mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestOAuthExchangeTokenCompleted;
			if (e.Succeeded)
			{
				CompleteRequestOAuthExchangeToken(e.ResponseCode, e.ResponseText);
			}
			else
			{
				_LogError("Error: Failed to request an OAuth exchange token. Error message: " + e.ErrorMessage);
			}
		}
	}

	private void CompleteRequestOAuthExchangeToken(long statusCode, string getShortCodeStatusServerResponse)
	{
		switch (statusCode)
		{
		default:
			_ = 404;
			break;
		case 200L:
		{
			string exchangeCode = ParseOAuthExchangeCodeFromStringResponse(getShortCodeStatusServerResponse);
			mixerInteractiveHelper.OnInternalCheckAuthStatusTimerCallback -= OnInternalCheckAuthStatusTimerCallback;
			mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.CheckAuthStatus);
			GetOauthToken(exchangeCode);
			break;
		}
		case 204L:
			break;
		}
	}

	private string ParseOAuthExchangeCodeFromStringResponse(string responseText)
	{
		string text = string.Empty;
		using (StringReader reader = new StringReader(responseText))
		{
			using JsonTextReader jsonTextReader = new JsonTextReader(reader);
			while (jsonTextReader.Read() && text == string.Empty)
			{
				if (jsonTextReader.Value != null && jsonTextReader.Value.ToString() == "code")
				{
					jsonTextReader.Read();
					text = jsonTextReader.Value.ToString();
				}
			}
		}
		return text;
	}

	private void GetOauthToken(string exchangeCode)
	{
		_Log("Retrieved an OAuth exchange token. Exchange token: " + exchangeCode + " Using AppID: " + AppID + " with exchange code: " + exchangeCode);
		string postData = "{ \"client_id\": \"" + AppID + "\", \"code\": \"" + exchangeCode + "\", \"grant_type\": \"authorization_code\" }";
		mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestOAuthTokenCompleted;
		mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestOAuthTokenCompleted;
		mixerInteractiveHelper._MakeWebRequest("OnRequestOAuthTokenCompleted", "https://mixer.com/api/v1/oauth/token", new Dictionary<string, string> { { "Content-Type", "application/json" } }, "POST", postData);
	}

	private void OnRequestOAuthTokenCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
	{
		if (!(e.RequestID != "OnRequestOAuthTokenCompleted"))
		{
			mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestOAuthTokenCompleted;
			if (e.Succeeded)
			{
				CompleteGetOAuthToken(e.ResponseCode, e.ResponseText);
			}
			else
			{
				_LogError("Error: Failed to request an OAuth token. Error message: " + e.ErrorMessage);
			}
		}
	}

	private void CompleteGetOAuthToken(long statusCode, string getCodeServerResponse)
	{
		if (statusCode == 400)
		{
			_LogError("Error: " + getCodeServerResponse + " while requesting an OAuth token.");
			return;
		}
		string oauthRefreshToken = string.Empty;
		string text = string.Empty;
		using (StringReader reader = new StringReader(getCodeServerResponse))
		{
			using JsonTextReader jsonTextReader = new JsonTextReader(reader);
			while (jsonTextReader.Read())
			{
				if (jsonTextReader.Value != null)
				{
					if (jsonTextReader.Value.ToString() == "access_token")
					{
						jsonTextReader.Read();
						text = jsonTextReader.Value.ToString();
					}
					else if (jsonTextReader.Value.ToString() == "refresh_token")
					{
						jsonTextReader.Read();
						oauthRefreshToken = jsonTextReader.Value.ToString();
					}
				}
			}
		}
		_authToken = "Bearer " + text;
		_oauthRefreshToken = oauthRefreshToken;
		mixerInteractiveHelper.WriteAuthTokensToCache(_authToken, _oauthRefreshToken);
		_Log("Retrieved a new OAuth token. Token: " + _authToken);
		mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.RefreshShortCode);
		mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.CheckAuthStatus);
		ConnectToWebsocket();
	}

	private void RefreshShortCode()
	{
		string postData = "{ \"client_id\": \"" + AppID + "\", \"scope\": \"interactive:robot:self\" }";
		mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestRefresheShortCodeCompleted;
		mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestRefresheShortCodeCompleted;
		mixerInteractiveHelper._MakeWebRequest("OnRequestRefresheShortCodeCompleted", "https://mixer.com/api/v1/oauth/shortcode", new Dictionary<string, string> { { "Content-Type", "application/json" } }, "POST", postData);
	}

	private void OnRequestRefresheShortCodeCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
	{
		if (e.RequestID != "OnRequestRefresheShortCodeCompleted")
		{
			return;
		}
		mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestRefresheShortCodeCompleted;
		if (e.Succeeded)
		{
			if (e.ResponseCode == 404)
			{
				_LogError("Error: OAuth Client ID not found. Make sure the OAuth Client ID you specified in the Unity editor matches the one in Interactive Studio.");
			}
			else
			{
				CompleteRefreshShortCode(e.ResponseText);
			}
		}
		else
		{
			_LogError("Error: Failed to retrieve a short code for short code authentication. Error message: " + e.ErrorMessage);
		}
	}

	private void CompleteRefreshShortCode(string getShortCodeServerResponse)
	{
		int num = -1;
		using (StringReader reader = new StringReader(getShortCodeServerResponse))
		{
			using JsonTextReader jsonTextReader = new JsonTextReader(reader);
			while (jsonTextReader.Read())
			{
				if (jsonTextReader.Value == null)
				{
					continue;
				}
				switch (jsonTextReader.Value.ToString().ToLowerInvariant())
				{
				case "code":
					jsonTextReader.Read();
					if (jsonTextReader.Value != null)
					{
						ShortCode = jsonTextReader.Value.ToString();
					}
					break;
				case "expires_in":
					jsonTextReader.Read();
					if (jsonTextReader.Value != null)
					{
						num = Convert.ToInt32(jsonTextReader.Value.ToString());
					}
					break;
				case "handle":
					jsonTextReader.Read();
					if (jsonTextReader.Value != null)
					{
						_authShortCodeRequestHandle = jsonTextReader.Value.ToString();
					}
					break;
				}
			}
		}
		mixerInteractiveHelper.OnInternalRefreshShortCodeTimerCallback -= OnInternalRefreshShortCodeTimerCallback;
		mixerInteractiveHelper.OnInternalRefreshShortCodeTimerCallback += OnInternalRefreshShortCodeTimerCallback;
		mixerInteractiveHelper.StartTimer(MixerInteractiveHelper.InteractiveTimerType.RefreshShortCode, num);
		mixerInteractiveHelper.OnInternalCheckAuthStatusTimerCallback -= OnInternalCheckAuthStatusTimerCallback;
		mixerInteractiveHelper.OnInternalCheckAuthStatusTimerCallback += OnInternalCheckAuthStatusTimerCallback;
		mixerInteractiveHelper.StartTimer(MixerInteractiveHelper.InteractiveTimerType.CheckAuthStatus, 0.5f);
		UpdateInteractivityState(InteractivityState.ShortCodeRequired);
	}

	private void VerifyAuthToken()
	{
		mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnVerifyAuthTokenRequestCompleted;
		mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnVerifyAuthTokenRequestCompleted;
		mixerInteractiveHelper._MakeWebRequest("OnVerifyAuthTokenRequestCompleted", _interactiveWebSocketUrl.Replace("wss", "https"), new Dictionary<string, string>
		{
			{ "Authorization", _authToken },
			{ "X-Interactive-Version", ProjectVersionID },
			{ "X-Protocol-Version", "2.0" }
		});
	}

	private void OnVerifyAuthTokenRequestCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
	{
		if (e.RequestID != "OnVerifyAuthTokenRequestCompleted")
		{
			return;
		}
		mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnVerifyAuthTokenRequestCompleted;
		if (e.Succeeded)
		{
			bool isTokenValid = false;
			if (e.ResponseCode == 401)
			{
				isTokenValid = false;
			}
			else if (e.ResponseCode == 200 || e.ResponseCode == 400)
			{
				isTokenValid = true;
			}
			else
			{
				_LogError("Error: Failed to while trying to validate a cached auth token. Error code: " + e.ResponseCode);
			}
			CompleteVerifyAuthTokenRequestStart(isTokenValid);
		}
		else
		{
			_LogError("Error: Failed to verify the auth token. Error message: " + e.ErrorMessage);
		}
	}

	private void CompleteVerifyAuthTokenRequestStart(bool isTokenValid)
	{
		if (!isTokenValid)
		{
			RefreshAuthToken();
		}
		else
		{
			ConnectToWebsocket();
		}
	}

	private void ConnectToWebsocket()
	{
		if (!_pendingConnectToWebSocket && !_websocketConnected)
		{
			_pendingConnectToWebSocket = false;
			_websocketConnected = true;
			string text = string.Empty;
			if (ShareCode != string.Empty)
			{
				text = ", Share Code: " + ShareCode;
			}
			_Log("Connecting to websocket with Project Version ID: " + ProjectVersionID + text + ", OAuth Client ID: " + AppID + " and Auth Token: " + _authToken + ".");
			_websocket = new WebSocket(_interactiveWebSocketUrl);
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection.Add("Authorization", _authToken);
			nameValueCollection.Add("X-Interactive-Version", ProjectVersionID);
			nameValueCollection.Add("X-Protocol-Version", "2.0");
			if (!string.IsNullOrEmpty(ShareCode))
			{
				nameValueCollection.Add("X-Interactive-Sharecode", ShareCode);
			}
			_websocket.SetHeaders(nameValueCollection);
			_websocket.OnOpen += OnWebsocketOpen;
			_websocket.OnMessage += OnWebSocketMessage;
			_websocket.OnError += OnWebSocketError;
			_websocket.OnClose += OnWebSocketClose;
			_websocket.Connect();
		}
	}

	private void OnWebsocketOpen(object sender, EventArgs args)
	{
		mixerInteractiveHelper.StopTimer(MixerInteractiveHelper.InteractiveTimerType.Reconnect);
	}

	private void OnWebSocketMessage(object sender, WebSocketSharp.MessageEventArgs args)
	{
		string empty = string.Empty;
		if (args.IsText)
		{
			empty = args.Data;
			ProcessWebSocketMessage(empty);
		}
	}

	private void OnWebSocketError(object sender, WebSocketSharp.ErrorEventArgs args)
	{
		UpdateInteractivityState(InteractivityState.InteractivityDisabled);
		_LogError("Error: Websocket OnError: " + args.Message);
	}

	private void OnWebSocketClose(object sender, WebSocketSharp.CloseEventArgs args)
	{
		UpdateInteractivityState(InteractivityState.InteractivityDisabled);
		if (args.Code == 4019)
		{
			_LogError("Connection failed (error code 4019): You don't have access to this project. Make sure that the account you are signed in with has access to this Version ID. If you are using a share code, make sure that the share code value matches the one in Interactive Studio for this project.");
			return;
		}
		if (args.Code == 4020)
		{
			_LogError("Connection failed (error code 4020): The interactive version was not found or you do not have access to it. Make sure that the account you are signed in with has access to this Version ID. If you are using a share code, make sure that the share code value matches the one in Interactive Studio for this project.");
			return;
		}
		if (args.Code == 4021)
		{
			_LogError("Connection failed (error code 4021): You are connected to this session somewhere else. Please disconnect from that session and try again.");
			return;
		}
		_pendingConnectToWebSocket = false;
		_websocketConnected = false;
		_activeWebsocketHostIndex++;
		_interactiveWebSocketUrl = _websocketHosts[_activeWebsocketHostIndex];
		mixerInteractiveHelper.OnInternalReconnectTimerCallback -= OnInternalReconnectTimerCallback;
		mixerInteractiveHelper.OnInternalReconnectTimerCallback += OnInternalReconnectTimerCallback;
		mixerInteractiveHelper.StartTimer(MixerInteractiveHelper.InteractiveTimerType.Reconnect, 0.5f);
	}

	private void RefreshAuthToken()
	{
		string postData = "{ \"client_id\": \"" + AppID + "\", \"refresh_token\": \"" + _oauthRefreshToken + "\", \"grant_type\": \"refresh_token\" }";
		mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestRefreshedAuthTokenCompleted;
		mixerInteractiveHelper.OnInternalWebRequestStateChanged += OnRequestRefreshedAuthTokenCompleted;
		mixerInteractiveHelper._MakeWebRequest("OnRequestRefreshedAuthTokenCompleted", "https://mixer.com/api/v1/oauth/token", new Dictionary<string, string> { { "Content-Type", "application/json" } }, "POST", postData);
	}

	private void OnRequestRefreshedAuthTokenCompleted(object sender, MixerInteractiveHelper._InternalWebRequestStateChangedEventArgs e)
	{
		if (!(e.RequestID != "OnRequestRefreshedAuthTokenCompleted"))
		{
			mixerInteractiveHelper.OnInternalWebRequestStateChanged -= OnRequestRefreshedAuthTokenCompleted;
			if (e.Succeeded)
			{
				CompleteRefreshAuthToken(e.ResponseCode, e.ResponseText);
			}
			else
			{
				_LogError("Error: Web request to refresh the Auth token failed. Error message: " + e.ErrorMessage);
			}
		}
	}

	private void CompleteRefreshAuthToken(long statusCode, string getCodeServerResponse)
	{
		if (statusCode == 400)
		{
			_LogError("Error: " + getCodeServerResponse + " trying to refresh the auth token.");
		}
		string text = string.Empty;
		string oauthRefreshToken = string.Empty;
		using (StringReader reader = new StringReader(getCodeServerResponse))
		{
			using JsonTextReader jsonTextReader = new JsonTextReader(reader);
			while (jsonTextReader.Read())
			{
				if (jsonTextReader.Value != null)
				{
					if (jsonTextReader.Value.ToString() == "access_token")
					{
						jsonTextReader.Read();
						text = jsonTextReader.Value.ToString();
					}
					else if (jsonTextReader.Value.ToString() == "refresh_token")
					{
						jsonTextReader.Read();
						oauthRefreshToken = jsonTextReader.Value.ToString();
					}
				}
			}
		}
		_authToken = "Bearer " + text;
		_oauthRefreshToken = oauthRefreshToken;
		mixerInteractiveHelper.WriteAuthTokensToCache(_authToken, _oauthRefreshToken);
		VerifyAuthToken();
	}

	private void UpdateInteractivityState(InteractivityState state)
	{
		InteractivityState = state;
		InteractivityStateChangedEventArgs item = new InteractivityStateChangedEventArgs(InteractiveEventType.InteractivityStateChanged, state);
		_queuedEvents.Add(item);
	}

	private InteractiveControl ControlFromControlID(string controlID)
	{
		foreach (InteractiveControl control in _Controls)
		{
			if (control.ControlID == controlID)
			{
				return control;
			}
		}
		return null;
	}

	internal void CaptureTransaction(string transactionID)
	{
		if (!string.IsNullOrEmpty(transactionID))
		{
			_SendCaptureTransactionMessage(transactionID);
		}
	}

	public void TriggerCooldown(string controlID, int cooldown)
	{
		if (InteractivityState != InteractivityState.InteractivityEnabled)
		{
			throw new Exception("Error: The InteractivityManager's InteractivityState must be InteractivityEnabled before calling this method.");
		}
		if (cooldown < 1000)
		{
			_Log("Info: Did you mean to use a cooldown of " + (float)cooldown / 1000f + " seconds? Remember, cooldowns are in milliseconds.");
		}
		string empty = string.Empty;
		string value = string.Empty;
		InteractiveControl interactiveControl = ControlFromControlID(controlID);
		if (interactiveControl != null)
		{
			if (!(interactiveControl is InteractiveButtonControl))
			{
				_LogError("Error: The control is not a button. You can only trigger a cooldown on a button.");
				return;
			}
			value = interactiveControl._sceneID;
		}
		long num = 0L;
		num = (long)Math.Truncate(DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds + (double)cooldown);
		if (interactiveControl is InteractiveButtonControl interactiveButtonControl)
		{
			interactiveButtonControl._cooldownExpirationTime = num;
		}
		uint num2 = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num2);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("updateControls");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("sceneID");
			jsonWriter.WriteValue(value);
			jsonWriter.WritePropertyName("controls");
			jsonWriter.WriteStartArray();
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("controlID");
			jsonWriter.WriteValue(controlID);
			jsonWriter.WritePropertyName("etag");
			jsonWriter.WriteValue(empty);
			jsonWriter.WritePropertyName("cooldown");
			jsonWriter.WriteValue(num);
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num2, "updateControls");
	}

	public void SendMessage(string message)
	{
		SendJsonString(message);
	}

	public void SendMessage(string messageType, Dictionary<string, object> parameters)
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue(messageType);
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			foreach (string key in parameters.Keys)
			{
				jsonWriter.WritePropertyName(key);
				jsonWriter.WriteValue(parameters[key].ToString());
			}
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, messageType);
	}

	public void StartInteractive()
	{
		if (InteractivityState == InteractivityState.NotInitialized)
		{
			MixerInteractive.GoInteractive();
		}
		if (InteractivityState != InteractivityState.Initializing && InteractivityState != InteractivityState.ShortCodeRequired && InteractivityState != InteractivityState.InteractivityPending && InteractivityState != InteractivityState.InteractivityEnabled)
		{
			SendReady(isReady: true);
			_shouldStartInteractive = false;
			UpdateInteractivityState(InteractivityState.InteractivityPending);
		}
	}

	public void StopInteractive()
	{
		if (InteractivityState != 0 && InteractivityState != InteractivityState.InteractivityDisabled)
		{
			UpdateInteractivityState(InteractivityState.InteractivityDisabled);
			SendReady(isReady: false);
			InteractiveEventArgs item = new InteractiveEventArgs(InteractiveEventType.InteractivityStateChanged);
			_queuedEvents.Add(item);
		}
	}

	public void DoWork()
	{
		ClearPreviousControlState();
		RaiseQueuedInteractiveEvents();
		SendQueuedSetControlPropertyUpdates();
	}

	private void RaiseQueuedInteractiveEvents()
	{
		InteractiveEventArgs[] array = _queuedEvents.ToArray();
		foreach (InteractiveEventArgs interactiveEventArgs in array)
		{
			switch (interactiveEventArgs.EventType)
			{
			case InteractiveEventType.InteractivityStateChanged:
				if (this.OnInteractivityStateChanged != null)
				{
					this.OnInteractivityStateChanged(this, interactiveEventArgs as InteractivityStateChangedEventArgs);
				}
				break;
			case InteractiveEventType.ParticipantStateChanged:
				if (this.OnParticipantStateChanged != null)
				{
					this.OnParticipantStateChanged(this, interactiveEventArgs as InteractiveParticipantStateChangedEventArgs);
				}
				break;
			case InteractiveEventType.Button:
				if (this.OnInteractiveButtonEvent != null)
				{
					this.OnInteractiveButtonEvent(this, interactiveEventArgs as InteractiveButtonEventArgs);
				}
				break;
			case InteractiveEventType.Joystick:
				if (this.OnInteractiveJoystickControlEvent != null)
				{
					this.OnInteractiveJoystickControlEvent(this, interactiveEventArgs as InteractiveJoystickEventArgs);
				}
				break;
			case InteractiveEventType.MouseButton:
				if (this.OnInteractiveMouseButtonEvent != null)
				{
					this.OnInteractiveMouseButtonEvent(this, interactiveEventArgs as InteractiveMouseButtonEventArgs);
				}
				break;
			case InteractiveEventType.Coordinates:
				if (this.OnInteractiveCoordinatesChangedEvent != null)
				{
					this.OnInteractiveCoordinatesChangedEvent(this, interactiveEventArgs as InteractiveCoordinatesChangedEventArgs);
				}
				break;
			case InteractiveEventType.TextInput:
				if (this.OnInteractiveTextControlEvent != null)
				{
					this.OnInteractiveTextControlEvent(this, interactiveEventArgs as InteractiveTextEventArgs);
				}
				break;
			case InteractiveEventType.Error:
				if (this.OnError != null)
				{
					this.OnError(this, interactiveEventArgs);
				}
				break;
			default:
				if (this.OnInteractiveMessageEvent != null)
				{
					this.OnInteractiveMessageEvent(this, interactiveEventArgs as InteractiveMessageEventArgs);
				}
				break;
			}
		}
		_queuedEvents.Clear();
		if (this.OnInteractiveDoWorkEvent != null)
		{
			this.OnInteractiveDoWorkEvent(this, new InteractiveEventArgs());
		}
	}

	private void SendQueuedSetControlPropertyUpdates()
	{
		foreach (string key in _queuedControlPropertyUpdates.Keys)
		{
			uint num = _currentmessageID++;
			StringWriter stringWriter = new StringWriter(new StringBuilder());
			using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
			{
				jsonWriter.WriteStartObject();
				jsonWriter.WritePropertyName("type");
				jsonWriter.WriteValue("method");
				jsonWriter.WritePropertyName("id");
				jsonWriter.WriteValue(num);
				jsonWriter.WritePropertyName("method");
				jsonWriter.WriteValue("updateControls");
				jsonWriter.WritePropertyName("params");
				jsonWriter.WriteStartObject();
				jsonWriter.WritePropertyName("sceneID");
				jsonWriter.WriteValue(key);
				jsonWriter.WritePropertyName("controls");
				jsonWriter.WriteStartArray();
				foreach (string key2 in _queuedControlPropertyUpdates[key].Keys)
				{
					jsonWriter.WriteStartObject();
					jsonWriter.WritePropertyName("controlID");
					jsonWriter.WriteValue(key2);
					Dictionary<string, _InternalControlPropertyMetaData> properties = _queuedControlPropertyUpdates[key][key2].properties;
					foreach (string key3 in properties.Keys)
					{
						jsonWriter.WritePropertyName(key3);
						_InternalControlPropertyMetaData internalControlPropertyMetaData = properties[key3];
						if (internalControlPropertyMetaData.type == _KnownControlPropertyPrimitiveTypes.Boolean)
						{
							jsonWriter.WriteValue(internalControlPropertyMetaData.boolValue);
						}
						else if (internalControlPropertyMetaData.type == _KnownControlPropertyPrimitiveTypes.Number)
						{
							jsonWriter.WriteValue(internalControlPropertyMetaData.numberValue);
						}
						else
						{
							jsonWriter.WriteValue(internalControlPropertyMetaData.stringValue);
						}
					}
					jsonWriter.WriteEndObject();
				}
				jsonWriter.WriteEndArray();
				jsonWriter.WriteEndObject();
				jsonWriter.WriteEnd();
				SendJsonString(stringWriter.ToString());
			}
			StoreIfExpectingReply(num, "updateControls");
		}
		_queuedControlPropertyUpdates.Clear();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			ResetInternalState();
			mixerInteractiveHelper.Dispose();
			if (_websocket != null)
			{
				_websocket.OnOpen -= OnWebsocketOpen;
				_websocket.OnMessage -= OnWebSocketMessage;
				_websocket.OnError -= OnWebSocketError;
				_websocket.OnClose -= OnWebSocketClose;
				_websocket.Close();
			}
			_disposed = true;
		}
	}

	public void SendMockWebSocketMessage(string rawText)
	{
		ProcessWebSocketMessage(rawText);
	}

	private void ProcessWebSocketMessage(string messageText)
	{
		try
		{
			using StringReader reader = new StringReader(messageText);
			using JsonTextReader jsonTextReader = new JsonTextReader(reader);
			int messageIDAsInt = -1;
			string empty = string.Empty;
			while (jsonTextReader.Read())
			{
				if (jsonTextReader.Value == null)
				{
					continue;
				}
				if (jsonTextReader.Value.ToString() == "id")
				{
					jsonTextReader.ReadAsInt32();
					messageIDAsInt = Convert.ToInt32(jsonTextReader.Value);
				}
				if (!(jsonTextReader.Value.ToString() == "type"))
				{
					continue;
				}
				jsonTextReader.Read();
				if (jsonTextReader.Value != null)
				{
					empty = jsonTextReader.Value.ToString();
					if (empty == "method")
					{
						ProcessMethod(jsonTextReader);
					}
					else if (empty == "reply")
					{
						ProcessReply(jsonTextReader, messageIDAsInt);
					}
				}
			}
		}
		catch
		{
			_LogError("Error: Failed to process message: " + messageText);
		}
		_Log(messageText);
		_queuedEvents.Add(new InteractiveMessageEventArgs(messageText));
	}

	private void ProcessMethod(JsonReader jsonReader)
	{
		try
		{
			while (jsonReader.Read())
			{
				if (jsonReader.Value == null)
				{
					continue;
				}
				string text = jsonReader.Value.ToString();
				try
				{
					if (text != null)
					{
						switch (text)
						{
						case "hello":
							HandleHelloMessage();
							break;
						case "onParticipantJoin":
							HandleParticipantJoin(jsonReader);
							break;
						case "onParticipantLeave":
							HandleParticipantLeave(jsonReader);
							break;
						case "onParticipantUpdate":
							HandleParticipantUpdate(jsonReader);
							break;
						case "giveInput":
							HandleGiveInput(jsonReader);
							break;
						case "onReady":
							HandleInteractivityStarted(jsonReader);
							break;
						case "onControlUpdate":
						case "onControlCreate":
							HandleControlUpdate(jsonReader);
							break;
						case "onGroupCreate":
							HandleGroupCreate(jsonReader);
							break;
						case "onGroupUpdate":
							HandleGroupUpdate(jsonReader);
							break;
						case "onSceneCreate":
							HandleSceneCreate(jsonReader);
							break;
						}
					}
				}
				catch (Exception ex)
				{
					_LogError("Error: Error while processing method: " + text + ". Error message: " + ex.Message);
				}
			}
		}
		catch (Exception ex2)
		{
			_LogError("Error: Error processing websocket message. Error message: " + ex2.Message);
		}
	}

	private void ProcessReply(JsonReader jsonReader, int messageIDAsInt)
	{
		uint key = 0u;
		if (messageIDAsInt != -1)
		{
			key = Convert.ToUInt32(messageIDAsInt);
		}
		else
		{
			try
			{
				while (jsonReader.Read())
				{
					if (jsonReader.Value != null && jsonReader.Value.ToString() == "id")
					{
						key = (uint)jsonReader.ReadAsInt32().Value;
					}
				}
			}
			catch
			{
				_LogError("Error: Failed to get the message ID from the reply message.");
			}
		}
		string value = string.Empty;
		_outstandingMessages.TryGetValue(key, out value);
		try
		{
			switch (value)
			{
			case "getAllParticipants":
				HandleGetAllParticipants(jsonReader);
				break;
			case "getGroups":
				HandleGetGroups(jsonReader);
				break;
			case "getScenes":
				HandleGetScenes(jsonReader);
				break;
			case "setCurrentScene":
				HandlePossibleError(jsonReader);
				break;
			}
		}
		catch
		{
			_LogError("Error: An error occured while processing the reply: " + value);
		}
	}

	private void HandlePossibleError(JsonReader jsonReader)
	{
		int num = 0;
		string text = string.Empty;
		while (jsonReader.Read())
		{
			if (jsonReader.Value == null)
			{
				continue;
			}
			switch (jsonReader.Value.ToString())
			{
			case "code":
				jsonReader.ReadAsInt32();
				num = Convert.ToInt32(jsonReader.Value);
				break;
			case "message":
				jsonReader.Read();
				if (jsonReader.Value != null)
				{
					text = text + " Message: " + jsonReader.Value.ToString();
				}
				break;
			case "path":
				jsonReader.Read();
				if (jsonReader.Value != null)
				{
					text = text + " Path: " + jsonReader.Value.ToString();
				}
				break;
			}
		}
		if (num != 0 && text != string.Empty)
		{
			_LogError(text, num);
		}
	}

	private void ResetInternalState()
	{
		_disposed = false;
		_initializedGroups = false;
		_initializedScenes = false;
		_shouldStartInteractive = false;
		_pendingConnectToWebSocket = false;
		_websocketConnected = false;
		UpdateInteractivityState(InteractivityState.NotInitialized);
	}

	private void HandleHelloMessage()
	{
		SendGetAllGroupsMessage();
		SendGetAllScenesMessage();
	}

	private void HandleInteractivityStarted(JsonReader jsonReader)
	{
		bool flag = false;
		while (jsonReader.Read())
		{
			if (jsonReader.Value != null && jsonReader.Value.ToString() == "isReady")
			{
				jsonReader.ReadAsBoolean();
				if (jsonReader.Value != null)
				{
					flag = (bool)jsonReader.Value;
					break;
				}
			}
		}
		if (flag)
		{
			UpdateInteractivityState(InteractivityState.InteractivityEnabled);
		}
	}

	private void HandleControlUpdate(JsonReader jsonReader)
	{
		string sceneID = string.Empty;
		while (jsonReader.Read())
		{
			if (jsonReader.Value != null)
			{
				string text = jsonReader.Value.ToString();
				if (text == "sceneID")
				{
					jsonReader.Read();
					sceneID = jsonReader.Value.ToString();
				}
				else if (text == "controls")
				{
					UpdateControls(jsonReader, sceneID);
				}
			}
		}
	}

	private void UpdateControls(JsonReader jsonReader, string sceneID)
	{
		try
		{
			while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndArray)
			{
				if (jsonReader.TokenType != JsonToken.StartObject)
				{
					continue;
				}
				InteractiveControl interactiveControl = ReadControl(jsonReader, sceneID);
				InteractiveControl interactiveControl2 = null;
				foreach (InteractiveControl control in _Controls)
				{
					if (control.ControlID == interactiveControl.ControlID)
					{
						interactiveControl2 = control;
						break;
					}
				}
				if (interactiveControl is InteractiveButtonControl item)
				{
					if (interactiveControl2 is InteractiveButtonControl item2)
					{
						_buttons.Remove(item2);
					}
					_buttons.Add(item);
				}
				if (interactiveControl is InteractiveJoystickControl item3)
				{
					if (interactiveControl2 is InteractiveJoystickControl item4)
					{
						_joysticks.Remove(item4);
					}
					_joysticks.Add(item3);
				}
				if (interactiveControl2 != null)
				{
					_controls.Remove(interactiveControl2);
				}
				_controls.Add(interactiveControl);
			}
		}
		catch
		{
			_LogError("Error: Failed reading controls for scene: " + sceneID + ".");
		}
	}

	private void HandleSceneCreate(JsonReader jsonReader)
	{
		while (jsonReader.Read())
		{
			if (jsonReader.Value != null && jsonReader.Value.ToString() == "scenes")
			{
				_scenes.Add(ReadScene(jsonReader));
			}
		}
	}

	private void HandleGroupCreate(JsonReader jsonReader)
	{
		ProcessGroups(jsonReader);
	}

	private void HandleGroupUpdate(JsonReader jsonReader)
	{
		ProcessGroups(jsonReader);
	}

	private void ProcessGroups(JsonReader jsonReader)
	{
		while (jsonReader.Read())
		{
			if (jsonReader.Value == null || !(jsonReader.Value.ToString() == "groups"))
			{
				continue;
			}
			InteractiveGroup interactiveGroup = ReadGroup(jsonReader);
			IList<InteractiveGroup> groups = Groups;
			int num = -1;
			for (int i = 0; i < groups.Count; i++)
			{
				if (groups[i].GroupID == interactiveGroup.GroupID)
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				CloneGroupValues(interactiveGroup, groups[num]);
			}
			else
			{
				_groups.Add(interactiveGroup);
			}
		}
	}

	private void CloneGroupValues(InteractiveGroup source, InteractiveGroup destination)
	{
		destination._etag = source._etag;
		destination.SceneID = source.SceneID;
		destination.GroupID = source.GroupID;
	}

	private void HandleGetAllParticipants(JsonReader jsonReader)
	{
		while (jsonReader.Read())
		{
			if (jsonReader.TokenType == JsonToken.StartObject)
			{
				_participants.Add(ReadParticipant(jsonReader));
			}
		}
	}

	private List<InteractiveParticipant> ReadParticipants(JsonReader jsonReader)
	{
		List<InteractiveParticipant> list = new List<InteractiveParticipant>();
		while (jsonReader.Read())
		{
			if (jsonReader.TokenType != JsonToken.StartObject)
			{
				continue;
			}
			InteractiveParticipant interactiveParticipant = ReadParticipant(jsonReader);
			IList<InteractiveParticipant> participants = Participants;
			int num = -1;
			for (int i = 0; i < participants.Count; i++)
			{
				if (participants[i].UserID == interactiveParticipant.UserID)
				{
					num = i;
				}
			}
			if (num != -1)
			{
				CloneParticipantValues(interactiveParticipant, participants[num]);
			}
			else
			{
				_participants.Add(interactiveParticipant);
			}
			list.Add(interactiveParticipant);
		}
		return list;
	}

	private void CloneParticipantValues(InteractiveParticipant source, InteractiveParticipant destination)
	{
		destination._sessionID = source._sessionID;
		destination.UserID = source.UserID;
		destination.UserName = source.UserName;
		destination.Level = source.Level;
		destination.LastInputAt = source.LastInputAt;
		destination.ConnectedAt = source.ConnectedAt;
		destination.InputDisabled = source.InputDisabled;
		destination.State = source.State;
		destination._groupID = source._groupID;
		destination._etag = source._etag;
	}

	private InteractiveParticipant ReadParticipant(JsonReader jsonReader)
	{
		uint userID = 0u;
		string newSessionID = string.Empty;
		string newEtag = string.Empty;
		string userName = string.Empty;
		string newGroupID = string.Empty;
		uint level = 0u;
		bool flag = false;
		List<string> list = new List<string>();
		double num = 0.0;
		double num2 = 0.0;
		DateTime lastInputAt = default(DateTime);
		DateTime connectedAt = default(DateTime);
		int depth = jsonReader.Depth;
		while (jsonReader.Read() && jsonReader.Depth > depth)
		{
			if (jsonReader.Value == null || jsonReader.Value == null)
			{
				continue;
			}
			switch (jsonReader.Value.ToString())
			{
			case "sessionID":
				jsonReader.Read();
				if (jsonReader.Value != null)
				{
					newSessionID = jsonReader.Value.ToString();
				}
				continue;
			case "etag":
				jsonReader.Read();
				if (jsonReader.Value != null)
				{
					newEtag = jsonReader.Value.ToString();
				}
				continue;
			case "userID":
				jsonReader.ReadAsInt32();
				userID = Convert.ToUInt32(jsonReader.Value);
				continue;
			case "username":
				jsonReader.Read();
				userName = jsonReader.Value.ToString();
				continue;
			case "level":
				jsonReader.Read();
				level = Convert.ToUInt32(jsonReader.Value);
				continue;
			case "lastInputAt":
				jsonReader.Read();
				num2 = Convert.ToDouble(jsonReader.Value);
				lastInputAt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(num2).ToLocalTime();
				continue;
			case "connectedAt":
				jsonReader.Read();
				num = Convert.ToDouble(jsonReader.Value);
				connectedAt = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(num).ToLocalTime();
				continue;
			case "groupID":
				jsonReader.Read();
				newGroupID = jsonReader.Value.ToString();
				continue;
			case "disabled":
				jsonReader.ReadAsBoolean();
				flag = (bool)jsonReader.Value;
				continue;
			case "channelGroups":
				break;
			default:
				continue;
			}
			while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndArray)
			{
				if (jsonReader.Value != null)
				{
					list.Add(jsonReader.Value.ToString());
				}
			}
		}
		InteractiveParticipantState state = (flag ? InteractiveParticipantState.InputDisabled : InteractiveParticipantState.Joined);
		return new InteractiveParticipant(newSessionID, newEtag, userID, newGroupID, userName, list, level, lastInputAt, connectedAt, flag, state);
	}

	private void HandleGetGroups(JsonReader jsonReader)
	{
		while (jsonReader.Read())
		{
			if (jsonReader.Value == null || !(jsonReader.Value.ToString() == "groups"))
			{
				continue;
			}
			InteractiveGroup interactiveGroup = ReadGroup(jsonReader);
			IList<InteractiveGroup> groups = Groups;
			int num = -1;
			for (int i = 0; i < groups.Count; i++)
			{
				if (groups[i].GroupID == interactiveGroup.GroupID)
				{
					num = i;
					break;
				}
			}
			if (num != -1)
			{
				CloneGroupValues(interactiveGroup, groups[num]);
			}
			else
			{
				_groups.Add(interactiveGroup);
			}
		}
		_initializedGroups = true;
		if (_initializedGroups && _initializedScenes)
		{
			UpdateInteractivityState(InteractivityState.Initialized);
			if (_shouldStartInteractive)
			{
				StartInteractive();
			}
		}
	}

	private InteractiveGroup ReadGroup(JsonReader jsonReader)
	{
		int depth = jsonReader.Depth;
		string newEtag = string.Empty;
		string sceneID = string.Empty;
		string groupID = string.Empty;
		jsonReader.Read();
		while (jsonReader.Read() && jsonReader.Depth > depth)
		{
			if (jsonReader.Value == null)
			{
				continue;
			}
			switch (jsonReader.Value.ToString())
			{
			case "etag":
				jsonReader.ReadAsString();
				if (jsonReader.Value != null)
				{
					newEtag = jsonReader.Value.ToString();
				}
				break;
			case "sceneID":
				jsonReader.ReadAsString();
				if (jsonReader.Value != null)
				{
					sceneID = jsonReader.Value.ToString();
				}
				break;
			case "groupID":
				jsonReader.ReadAsString();
				if (jsonReader.Value != null)
				{
					groupID = jsonReader.Value.ToString();
				}
				break;
			}
		}
		return new InteractiveGroup(newEtag, sceneID, groupID);
	}

	private Dictionary<string, object> ReadMetaProperties(JsonReader jsonReader)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndObject)
		{
			if (jsonReader.Value != null)
			{
				string metaPropertyKey = jsonReader.Value.ToString();
				ReadMetaProperty(jsonReader, metaPropertyKey, dictionary);
			}
		}
		return dictionary;
	}

	private void ReadMetaProperty(JsonReader jsonReader, string metaPropertyKey, Dictionary<string, object> metaProperties)
	{
		string text = string.Empty;
		while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndObject && !(text != string.Empty))
		{
			if (jsonReader.Value != null && jsonReader.Value.ToString() == "value")
			{
				jsonReader.Read();
				if (jsonReader.Value != null)
				{
					text = jsonReader.Value.ToString();
				}
			}
		}
		metaProperties.Add(metaPropertyKey, text);
	}

	private void HandleGetScenes(JsonReader jsonReader)
	{
		while (jsonReader.Read())
		{
			if (jsonReader.Value != null && jsonReader.Value.ToString() == "scenes")
			{
				_scenes = ReadScenes(jsonReader);
			}
		}
		_initializedScenes = true;
		if (_initializedGroups && _initializedScenes)
		{
			UpdateInteractivityState(InteractivityState.Initialized);
			if (_shouldStartInteractive)
			{
				StartInteractive();
			}
		}
	}

	private List<InteractiveScene> ReadScenes(JsonReader jsonReader)
	{
		List<InteractiveScene> list = new List<InteractiveScene>();
		while (jsonReader.Read())
		{
			if (jsonReader.TokenType == JsonToken.StartObject)
			{
				list.Add(ReadScene(jsonReader));
			}
		}
		return list;
	}

	private InteractiveScene ReadScene(JsonReader jsonReader)
	{
		InteractiveScene interactiveScene = new InteractiveScene();
		try
		{
			int depth = jsonReader.Depth;
			while (jsonReader.Read() && jsonReader.Depth > depth)
			{
				if (jsonReader.Value == null)
				{
					continue;
				}
				switch (jsonReader.Value.ToString())
				{
				case "sceneID":
					jsonReader.ReadAsString();
					if (jsonReader.Value != null)
					{
						interactiveScene.SceneID = jsonReader.Value.ToString();
					}
					break;
				case "etag":
					jsonReader.ReadAsString();
					if (jsonReader.Value != null)
					{
						interactiveScene._etag = jsonReader.Value.ToString();
					}
					break;
				case "controls":
					ReadControls(jsonReader, interactiveScene);
					break;
				}
			}
		}
		catch
		{
			_LogError("Error: Error reading scene " + interactiveScene.SceneID + ".");
		}
		return interactiveScene;
	}

	private void ReadControls(JsonReader jsonReader, InteractiveScene scene)
	{
		try
		{
			while (jsonReader.Read() && jsonReader.TokenType != JsonToken.EndArray)
			{
				if (jsonReader.TokenType == JsonToken.StartObject)
				{
					InteractiveControl interactiveControl = ReadControl(jsonReader, scene.SceneID);
					if (interactiveControl is InteractiveButtonControl item)
					{
						_buttons.Add(item);
					}
					if (interactiveControl is InteractiveJoystickControl item2)
					{
						_joysticks.Add(item2);
					}
					_controls.Add(interactiveControl);
				}
			}
		}
		catch
		{
			_LogError("Error: Failed reading controls for scene: " + scene.SceneID + ".");
		}
	}

	private InteractiveControl ReadControl(JsonReader jsonReader, string sceneID = "")
	{
		int depth = jsonReader.Depth;
		string text = string.Empty;
		uint cost = 0u;
		bool flag = false;
		string text2 = string.Empty;
		string eTag = string.Empty;
		string text3 = string.Empty;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		try
		{
			while (jsonReader.Read() && jsonReader.Depth > depth)
			{
				if (jsonReader.Value == null)
				{
					continue;
				}
				switch (jsonReader.Value.ToString())
				{
				case "controlID":
					jsonReader.ReadAsString();
					text = jsonReader.Value.ToString();
					continue;
				case "disabled":
					jsonReader.ReadAsBoolean();
					flag = (bool)jsonReader.Value;
					continue;
				case "text":
					jsonReader.Read();
					text2 = jsonReader.Value.ToString();
					continue;
				case "etag":
					jsonReader.Read();
					eTag = jsonReader.Value.ToString();
					continue;
				case "kind":
					jsonReader.Read();
					text3 = jsonReader.Value.ToString();
					continue;
				case "cost":
					jsonReader.ReadAsInt32();
					cost = Convert.ToUInt32(jsonReader.Value);
					continue;
				case "meta":
					break;
				default:
					continue;
				}
				while (jsonReader.Read())
				{
					if (jsonReader.TokenType == JsonToken.StartObject)
					{
						dictionary = ReadMetaProperties(jsonReader);
						break;
					}
				}
			}
		}
		catch
		{
			_LogError("Error: Error reading control " + text + ".");
		}
		return text3 switch
		{
			"button" => new InteractiveButtonControl(text, InteractiveEventType.Button, flag, text2, cost, eTag, sceneID, dictionary), 
			"joystick" => new InteractiveJoystickControl(text, InteractiveEventType.Joystick, flag, text2, eTag, sceneID, dictionary), 
			"textbox" => new InteractiveTextControl(text, InteractiveEventType.TextInput, flag, text2, eTag, sceneID, dictionary), 
			"label" => new InteractiveLabelControl(text, text2, sceneID), 
			_ => new InteractiveControl(text, text3, InteractiveEventType.Unknown, flag, text2, eTag, sceneID, dictionary), 
		};
	}

	private _InputEvent ReadInputObject(JsonReader jsonReader)
	{
		_InputEvent result = default(_InputEvent);
		while (jsonReader.Read())
		{
			if (jsonReader.TokenType == JsonToken.StartObject)
			{
				result = ReadInputInnerObject(jsonReader);
			}
		}
		return result;
	}

	private _InputEvent ReadInputInnerObject(JsonReader jsonReader)
	{
		int depth = jsonReader.Depth;
		string text = string.Empty;
		string text2 = string.Empty;
		object obj = null;
		bool isPressed = false;
		float x = 0f;
		float y = 0f;
		string textValue = string.Empty;
		try
		{
			while (jsonReader.Read() && jsonReader.Depth > depth)
			{
				if (jsonReader.Value == null)
				{
					continue;
				}
				string text3 = jsonReader.Value.ToString();
				switch (text3)
				{
				case "controlID":
					jsonReader.ReadAsString();
					if (jsonReader.Value != null)
					{
						text = jsonReader.Value.ToString();
					}
					break;
				case "event":
					text2 = jsonReader.ReadAsString();
					switch (text2)
					{
					case "mousedown":
					case "mouseup":
					case "keydown":
					case "keyup":
						switch (text2)
						{
						case "mousedown":
						case "keydown":
							isPressed = true;
							break;
						case "mouseup":
						case "keyup":
							isPressed = false;
							break;
						}
						break;
					}
					break;
				case "x":
					x = (float)jsonReader.ReadAsDouble().Value;
					break;
				case "y":
					y = (float)jsonReader.ReadAsDouble().Value;
					break;
				case "value":
					jsonReader.Read();
					obj = jsonReader.Value;
					break;
				}
				foreach (string key in _giveInputKeyValues.Keys)
				{
					if (key == text3)
					{
						_giveInputKeyValues[key] = obj;
					}
				}
			}
		}
		catch
		{
			_LogError("Error: Error reading input from control " + text + ".");
		}
		uint cost = 0u;
		InteractiveControl interactiveControl = ControlFromControlID(text);
		if (interactiveControl is InteractiveButtonControl interactiveButtonControl)
		{
			cost = interactiveButtonControl.Cost;
		}
		InteractiveEventType interactiveEventType = InteractiveEventTypeFromID(text);
		if (interactiveEventType == InteractiveEventType.TextInput)
		{
			textValue = obj.ToString();
		}
		return new _InputEvent(text, interactiveControl._kind, text2, interactiveEventType, isPressed, x, y, cost, string.Empty, textValue);
	}

	private void HandleParticipantJoin(JsonReader jsonReader)
	{
		_ = jsonReader.Depth;
		while (jsonReader.Read())
		{
			if (jsonReader.Value == null)
			{
				continue;
			}
			string text = jsonReader.Value.ToString();
			if (text != null && text == "participants")
			{
				List<InteractiveParticipant> list = ReadParticipants(jsonReader);
				for (int i = 0; i < list.Count; i++)
				{
					InteractiveParticipant interactiveParticipant = list[i];
					interactiveParticipant.State = InteractiveParticipantState.Joined;
					_queuedEvents.Add(new InteractiveParticipantStateChangedEventArgs(InteractiveEventType.ParticipantStateChanged, interactiveParticipant, interactiveParticipant.State));
				}
			}
		}
	}

	private void HandleParticipantLeave(JsonReader jsonReader)
	{
		try
		{
			_ = jsonReader.Depth;
			while (jsonReader.Read())
			{
				if (jsonReader.Value == null)
				{
					continue;
				}
				string text = jsonReader.Value.ToString();
				if (text == null || !(text == "participants"))
				{
					continue;
				}
				List<InteractiveParticipant> list = ReadParticipants(jsonReader);
				for (int i = 0; i < list.Count; i++)
				{
					for (int num = _participants.Count - 1; num >= 0; num--)
					{
						if (_participants[num].UserID == list[i].UserID)
						{
							InteractiveParticipant interactiveParticipant = _participants[num];
							interactiveParticipant.State = InteractiveParticipantState.Left;
							_queuedEvents.Add(new InteractiveParticipantStateChangedEventArgs(InteractiveEventType.ParticipantStateChanged, interactiveParticipant, interactiveParticipant.State));
						}
					}
				}
			}
		}
		catch
		{
			_LogError("Error: Error while processing participant leave message.");
		}
	}

	private void HandleParticipantUpdate(JsonReader jsonReader)
	{
		_ = jsonReader.Depth;
		while (jsonReader.Read())
		{
			if (jsonReader.Value != null)
			{
				string text = jsonReader.Value.ToString();
				if (text != null && text == "participants")
				{
					ReadParticipants(jsonReader);
				}
			}
		}
	}

	private void HandleGiveInput(JsonReader jsonReader)
	{
		string sessionID = string.Empty;
		string text = string.Empty;
		_InputEvent inputEvent = default(_InputEvent);
		while (jsonReader.Read())
		{
			if (jsonReader.Value != null)
			{
				switch (jsonReader.Value.ToString())
				{
				case "participantID":
					jsonReader.Read();
					sessionID = jsonReader.Value.ToString();
					break;
				case "input":
					inputEvent = ReadInputObject(jsonReader);
					break;
				case "transactionID":
					jsonReader.Read();
					text = jsonReader.Value.ToString();
					break;
				}
			}
		}
		inputEvent.TransactionID = text;
		InternalTransactionIDState value = default(InternalTransactionIDState);
		if (_transactionIDsState.ContainsKey(inputEvent.ControlID))
		{
			value = _transactionIDsState[inputEvent.ControlID];
		}
		value.nextTransactionID = text;
		_transactionIDsState[inputEvent.ControlID] = value;
		InteractiveParticipant interactiveParticipant = ParticipantBySessionId(sessionID);
		if (!_participantsWhoTriggeredGiveInput.ContainsKey(inputEvent.ControlID))
		{
			_participantsWhoTriggeredGiveInput.Add(inputEvent.ControlID, new _InternalParticipantTrackingState(interactiveParticipant));
		}
		interactiveParticipant.LastInputAt = DateTime.UtcNow;
		if (inputEvent.Type == InteractiveEventType.Button)
		{
			InteractiveButtonEventArgs interactiveButtonEventArgs = new InteractiveButtonEventArgs(inputEvent.Type, inputEvent.ControlID, interactiveParticipant, inputEvent.IsPressed, inputEvent.Cost, inputEvent.TransactionID);
			_queuedEvents.Add(interactiveButtonEventArgs);
			UpdateInternalButtonState(interactiveButtonEventArgs);
		}
		else if (inputEvent.Type == InteractiveEventType.Joystick)
		{
			InteractiveJoystickEventArgs interactiveJoystickEventArgs = new InteractiveJoystickEventArgs(inputEvent.Type, inputEvent.ControlID, interactiveParticipant, inputEvent.X, inputEvent.Y);
			_queuedEvents.Add(interactiveJoystickEventArgs);
			UpdateInternalJoystickState(interactiveJoystickEventArgs);
		}
		else if (inputEvent.Type == InteractiveEventType.TextInput)
		{
			InteractiveTextEventArgs interactiveTextEventArgs = new InteractiveTextEventArgs(inputEvent.Type, inputEvent.ControlID, interactiveParticipant, inputEvent.TextValue, inputEvent.TransactionID);
			_queuedEvents.Add(interactiveTextEventArgs);
			UpdateInternalTextBoxState(interactiveTextEventArgs);
		}
		uint userID = interactiveParticipant.UserID;
		if (inputEvent.Kind == "screen")
		{
			if (inputEvent.Event == "move")
			{
				Vector2 vector = new Vector2(inputEvent.X, inputEvent.Y);
				vector.x *= Screen.width;
				vector.y *= Screen.height;
				if (_mousePositionsByParticipant.ContainsKey(userID))
				{
					_mousePositionsByParticipant[userID] = vector;
				}
				else
				{
					_mousePositionsByParticipant.Add(userID, vector);
				}
				InteractiveCoordinatesChangedEventArgs item = new InteractiveCoordinatesChangedEventArgs(inputEvent.ControlID, interactiveParticipant, vector);
				_queuedEvents.Add(item);
			}
			else if (inputEvent.Event == "mousedown" || inputEvent.Event == "mouseup")
			{
				Vector2 vector2 = new Vector2(inputEvent.X, inputEvent.Y);
				vector2.x *= Screen.width;
				vector2.y *= Screen.height;
				InteractiveMouseButtonEventArgs interactiveMouseButtonEventArgs = new InteractiveMouseButtonEventArgs(inputEvent.ControlID, interactiveParticipant, inputEvent.IsPressed, vector2);
				_queuedEvents.Add(interactiveMouseButtonEventArgs);
				UpdateInternalMouseButtonState(interactiveMouseButtonEventArgs);
			}
		}
		string controlID = inputEvent.ControlID;
		string kind = inputEvent.Kind;
		Dictionary<string, object> value2 = new Dictionary<string, object>();
		if (_giveInputControlData.TryGetValue(controlID, out value2))
		{
			foreach (string key in value2.Keys)
			{
				object value3 = null;
				if (_giveInputKeyValues.TryGetValue(key, out value3))
				{
					value2[key] = value3;
				}
			}
			_giveInputControlData[controlID] = value2;
		}
		else
		{
			_giveInputControlData[controlID] = new Dictionary<string, object>();
		}
		Dictionary<uint, Dictionary<string, object>> value4 = new Dictionary<uint, Dictionary<string, object>>();
		if (_giveInputControlDataByParticipant.TryGetValue(kind, out value4))
		{
			Dictionary<string, object> value5 = new Dictionary<string, object>();
			if (value4.TryGetValue(userID, out value5))
			{
				foreach (string key2 in value5.Keys)
				{
					object value6 = null;
					if (_giveInputKeyValues.TryGetValue(key2, out value6))
					{
						value5[key2] = value6;
					}
				}
				value4[userID] = value5;
			}
			else
			{
				value5 = new Dictionary<string, object>();
			}
			_giveInputControlDataByParticipant[kind] = value4;
		}
		else
		{
			_giveInputControlDataByParticipant[kind] = new Dictionary<uint, Dictionary<string, object>>();
		}
	}

	private InteractiveParticipant ParticipantBySessionId(string sessionID)
	{
		InteractiveParticipant result = null;
		foreach (InteractiveParticipant participant in Participants)
		{
			if (participant._sessionID == sessionID)
			{
				result = participant;
				break;
			}
		}
		return result;
	}

	internal InteractiveParticipant _ParticipantByUserId(uint userID)
	{
		InteractiveParticipant result = null;
		foreach (InteractiveParticipant participant in Participants)
		{
			if (participant.UserID == userID)
			{
				result = participant;
				break;
			}
		}
		return result;
	}

	internal bool _GetButtonDown(string controlID, uint userID)
	{
		bool result = false;
		if (_buttonStatesByParticipant.TryGetValue(userID, out var value))
		{
			if (value.TryGetValue(controlID, out var value2))
			{
				result = value2.ButtonCountState.CountOfButtonDownEvents != 0;
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	internal bool _GetButtonPressed(string controlID, uint userID)
	{
		bool result = false;
		if (_buttonStatesByParticipant.TryGetValue(userID, out var value))
		{
			if (value.TryGetValue(controlID, out var value2))
			{
				result = value2.ButtonCountState.CountOfButtonPressEvents != 0;
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	internal bool _GetButtonUp(string controlID, uint userID)
	{
		bool result = false;
		if (_buttonStatesByParticipant.TryGetValue(userID, out var value))
		{
			if (value.TryGetValue(controlID, out var value2))
			{
				result = value2.ButtonCountState.CountOfButtonUpEvents != 0;
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	internal uint _GetCountOfButtonDowns(string controlID, uint userID)
	{
		uint result = 0u;
		if (_buttonStatesByParticipant.TryGetValue(userID, out var value) && value.TryGetValue(controlID, out var value2))
		{
			result = value2.ButtonCountState.CountOfButtonDownEvents;
		}
		return result;
	}

	internal uint _GetCountOfButtonPresses(string controlID, uint userID)
	{
		uint result = 0u;
		if (_buttonStatesByParticipant.TryGetValue(userID, out var value) && value.TryGetValue(controlID, out var value2))
		{
			result = value2.ButtonCountState.CountOfButtonPressEvents;
		}
		return result;
	}

	internal uint _GetCountOfButtonUps(string controlID, uint userID)
	{
		uint result = 0u;
		if (_buttonStatesByParticipant.TryGetValue(userID, out var value) && value.TryGetValue(controlID, out var value2))
		{
			result = value2.ButtonCountState.CountOfButtonUpEvents;
		}
		return result;
	}

	internal bool _TryGetButtonStateByParticipant(uint userID, string controlID, out _InternalButtonState buttonState)
	{
		buttonState = default(_InternalButtonState);
		bool result = false;
		if (_buttonStatesByParticipant.TryGetValue(userID, out var value) && value.TryGetValue(controlID, out buttonState))
		{
			result = true;
		}
		return result;
	}

	internal InteractiveJoystickControl _GetJoystick(string controlID, uint userID)
	{
		InteractiveJoystickControl interactiveJoystickControl = new InteractiveJoystickControl(controlID, InteractiveEventType.Joystick, enabled: true, string.Empty, string.Empty, string.Empty, new Dictionary<string, object>());
		foreach (InteractiveJoystickControl joystick in Joysticks)
		{
			if (joystick.ControlID == controlID)
			{
				interactiveJoystickControl = joystick;
			}
		}
		interactiveJoystickControl._userID = userID;
		return interactiveJoystickControl;
	}

	internal double _GetJoystickX(string controlID, uint userID)
	{
		double result = 0.0;
		if (TryGetJoystickStateByParticipant(userID, controlID, out var joystickState))
		{
			result = joystickState.X;
		}
		return result;
	}

	internal double _GetJoystickY(string controlID, uint userID)
	{
		double result = 0.0;
		if (TryGetJoystickStateByParticipant(userID, controlID, out var joystickState))
		{
			result = joystickState.Y;
		}
		return result;
	}

	private bool TryGetJoystickStateByParticipant(uint userID, string controlID, out _InternalJoystickState joystickState)
	{
		joystickState = default(_InternalJoystickState);
		bool result = false;
		if (_joystickStatesByParticipant.TryGetValue(userID, out var value) && value.TryGetValue(controlID, out joystickState))
		{
			result = true;
		}
		return result;
	}

	internal _InternalMouseButtonState TryGetMouseButtonState(uint userID)
	{
		_InternalMouseButtonState value = default(_InternalMouseButtonState);
		_mouseButtonStateByParticipant.TryGetValue(userID, out value);
		return value;
	}

	internal string GetText(string controlID, uint userID)
	{
		string value = string.Empty;
		if (_textboxValuesByParticipant.TryGetValue(userID, out var value2))
		{
			value2.TryGetValue(controlID, out value);
		}
		return value;
	}

	internal InteractiveControl _GetControl(string controlID)
	{
		InteractiveControl result = new InteractiveControl(controlID, "", InteractiveEventType.Unknown, disabled: true, "", "", "", new Dictionary<string, object>());
		foreach (InteractiveControl control in _Controls)
		{
			if (control.ControlID == controlID)
			{
				result = control;
				break;
			}
		}
		return result;
	}

	public InteractiveButtonControl GetButton(string controlID)
	{
		InteractiveButtonControl result = new InteractiveButtonControl(controlID, InteractiveEventType.Button, disabled: false, string.Empty, 0u, string.Empty, string.Empty, new Dictionary<string, object>());
		foreach (InteractiveButtonControl button in Buttons)
		{
			if (button.ControlID == controlID)
			{
				result = button;
				break;
			}
		}
		return result;
	}

	public InteractiveJoystickControl GetJoystick(string controlID)
	{
		InteractiveJoystickControl result = new InteractiveJoystickControl(controlID, InteractiveEventType.Joystick, enabled: true, "", "", "", new Dictionary<string, object>());
		foreach (InteractiveJoystickControl joystick in Joysticks)
		{
			if (joystick.ControlID == controlID)
			{
				result = joystick;
				break;
			}
		}
		return result;
	}

	public string GetCurrentScene()
	{
		return GroupFromID("default").SceneID;
	}

	public void SetCurrentScene(string sceneID)
	{
		GroupFromID("default")?.SetScene(sceneID);
	}

	internal IList<InteractiveTextResult> _GetText(string controlID)
	{
		List<InteractiveTextResult> list = new List<InteractiveTextResult>();
		InteractivityManager singletonInstance = SingletonInstance;
		Dictionary<uint, Dictionary<string, string>> textboxValuesByParticipant = _textboxValuesByParticipant;
		foreach (uint key in textboxValuesByParticipant.Keys)
		{
			Dictionary<string, string> dictionary = textboxValuesByParticipant[key];
			string value = string.Empty;
			dictionary.TryGetValue(controlID, out value);
			InteractiveTextResult item = default(InteractiveTextResult);
			item.Participant = singletonInstance._ParticipantByUserId(key);
			item.Text = value;
			list.Add(item);
		}
		return list;
	}

	internal void _SetCurrentSceneInternal(InteractiveGroup group, string sceneID)
	{
		_SendSetUpdateGroupsMessage(group.GroupID, sceneID, group._etag);
	}

	private InteractiveGroup GroupFromID(string groupID)
	{
		InteractiveGroup result = new InteractiveGroup("", groupID, "default");
		foreach (InteractiveGroup group in Groups)
		{
			if (group.GroupID == groupID)
			{
				result = group;
				break;
			}
		}
		return result;
	}

	private InteractiveScene SceneFromID(string sceneID)
	{
		InteractiveScene result = new InteractiveScene(sceneID);
		foreach (InteractiveScene scene in Scenes)
		{
			if (scene.SceneID == sceneID)
			{
				result = scene;
				break;
			}
		}
		return result;
	}

	private InteractiveEventType InteractiveEventTypeFromID(string controlID)
	{
		InteractiveEventType result = InteractiveEventType.Unknown;
		foreach (InteractiveControl control in _controls)
		{
			if (controlID == control.ControlID)
			{
				result = control._type;
				break;
			}
		}
		return result;
	}

	private void SendReady(bool isReady)
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("ready");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("isReady");
			jsonWriter.WriteValue(isReady);
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, "ready");
	}

	internal void _SendCaptureTransactionMessage(string transactionID)
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("capture");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("transactionID");
			jsonWriter.WriteValue(transactionID);
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, "capture");
	}

	internal void _SendCreateGroupsMessage(string groupID, string sceneID)
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("createGroups");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("groups");
			jsonWriter.WriteStartArray();
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("groupID");
			jsonWriter.WriteValue(groupID);
			jsonWriter.WritePropertyName("sceneID");
			jsonWriter.WriteValue(sceneID);
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, "setCurrentScene");
	}

	internal void _SendSetUpdateGroupsMessage(string groupID, string sceneID, string groupEtag)
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("updateGroups");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("groups");
			jsonWriter.WriteStartArray();
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("groupID");
			jsonWriter.WriteValue(groupID);
			jsonWriter.WritePropertyName("sceneID");
			jsonWriter.WriteValue(sceneID);
			jsonWriter.WritePropertyName("etag");
			jsonWriter.WriteValue(groupEtag);
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, "setCurrentScene");
	}

	internal void _SendSetUpdateScenesMessage(InteractiveScene scene)
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("updateScenes");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("scenes");
			jsonWriter.WriteStartArray();
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("sceneID");
			jsonWriter.WriteValue(scene.SceneID);
			jsonWriter.WritePropertyName("etag");
			jsonWriter.WriteValue(scene._etag);
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, "setCurrentScene");
	}

	internal void _SendUpdateParticipantsMessage(InteractiveParticipant participant)
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("updateParticipants");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("participants");
			jsonWriter.WriteStartArray();
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("sessionID");
			jsonWriter.WriteValue(participant._sessionID);
			jsonWriter.WritePropertyName("etag");
			jsonWriter.WriteValue(participant._etag);
			jsonWriter.WritePropertyName("groupID");
			jsonWriter.WriteValue(participant._groupID);
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, "updateParticipants");
	}

	private void SendSetCompressionMessage()
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("setCompression");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("scheme");
			jsonWriter.WriteStartArray();
			jsonWriter.WriteValue("gzip");
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, "setCompression");
	}

	internal void _SendSetJoystickSetCoordinates(string controlID, double x, double y)
	{
		InteractiveControl interactiveControl = ControlFromControlID(controlID);
		if (interactiveControl != null)
		{
			uint num = _currentmessageID++;
			StringWriter stringWriter = new StringWriter(new StringBuilder());
			using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
			{
				jsonWriter.WriteStartObject();
				jsonWriter.WritePropertyName("type");
				jsonWriter.WriteValue("method");
				jsonWriter.WritePropertyName("id");
				jsonWriter.WriteValue(num);
				jsonWriter.WritePropertyName("method");
				jsonWriter.WriteValue("updateControls");
				jsonWriter.WritePropertyName("params");
				jsonWriter.WriteStartObject();
				jsonWriter.WritePropertyName("sceneID");
				jsonWriter.WriteValue(interactiveControl._sceneID);
				jsonWriter.WritePropertyName("controls");
				jsonWriter.WriteStartArray();
				jsonWriter.WriteStartObject();
				jsonWriter.WritePropertyName("controlID");
				jsonWriter.WriteValue(controlID);
				jsonWriter.WritePropertyName("etag");
				jsonWriter.WriteValue(interactiveControl._eTag);
				jsonWriter.WritePropertyName("x");
				jsonWriter.WriteValue(x);
				jsonWriter.WritePropertyName("y");
				jsonWriter.WriteValue(y);
				jsonWriter.WriteEndObject();
				jsonWriter.WriteEndArray();
				jsonWriter.WriteEndObject();
				jsonWriter.WriteEnd();
				SendJsonString(stringWriter.ToString());
			}
			StoreIfExpectingReply(num, "setJoystickCoordinates");
		}
	}

	internal void _SendSetButtonControlProperties(string controlID, string propertyName, bool disabled, float progress, string text, uint cost)
	{
		InteractiveControl interactiveControl = ControlFromControlID(controlID);
		if (interactiveControl == null)
		{
			return;
		}
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue("updateControls");
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("sceneID");
			jsonWriter.WriteValue(interactiveControl._sceneID);
			jsonWriter.WritePropertyName("controls");
			jsonWriter.WriteStartArray();
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("controlID");
			jsonWriter.WriteValue(controlID);
			jsonWriter.WritePropertyName("etag");
			jsonWriter.WriteValue(interactiveControl._eTag);
			if (propertyName == "disabled")
			{
				jsonWriter.WritePropertyName("disabled");
				jsonWriter.WriteValue(disabled);
			}
			if (propertyName == "progress")
			{
				jsonWriter.WritePropertyName("progress");
				jsonWriter.WriteValue(progress);
			}
			if (propertyName == "text")
			{
				jsonWriter.WritePropertyName("text");
				jsonWriter.WriteValue(text);
			}
			if (propertyName == "cost")
			{
				jsonWriter.WritePropertyName("cost");
				jsonWriter.WriteValue(cost);
			}
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEndArray();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			SendJsonString(stringWriter.ToString());
		}
		StoreIfExpectingReply(num, "setButtonControlProperties");
	}

	private void SendGetAllGroupsMessage()
	{
		SendCallMethodMessage("getGroups");
	}

	private void SendGetAllScenesMessage()
	{
		SendCallMethodMessage("getScenes");
	}

	private void SendGetAllParticipants()
	{
		SendCallMethodMessage("getAllParticipants");
	}

	private void SendCallMethodMessage(string method)
	{
		uint num = _currentmessageID++;
		StringWriter stringWriter = new StringWriter(new StringBuilder());
		using (JsonWriter jsonWriter = new JsonTextWriter(stringWriter))
		{
			jsonWriter.WriteStartObject();
			jsonWriter.WritePropertyName("type");
			jsonWriter.WriteValue("method");
			jsonWriter.WritePropertyName("id");
			jsonWriter.WriteValue(num);
			jsonWriter.WritePropertyName("method");
			jsonWriter.WriteValue(method);
			jsonWriter.WritePropertyName("params");
			jsonWriter.WriteStartObject();
			jsonWriter.WriteEndObject();
			jsonWriter.WriteEnd();
			try
			{
				SendJsonString(stringWriter.ToString());
			}
			catch (Exception ex)
			{
				_ = ex.Message;
				_LogError("Error: Unable to send message: " + method);
			}
		}
		StoreIfExpectingReply(num, method);
	}

	private void SendJsonString(string jsonString)
	{
		if (_websocket != null)
		{
			_websocket.Send(jsonString);
			_Log(jsonString);
		}
	}

	private void StoreIfExpectingReply(uint messageID, string messageType)
	{
		if (messageType != "getAllParticipants" || messageType != "getGroups" || messageType != "getScenes" || messageType != "setCurrentScene")
		{
			_outstandingMessages.Add(messageID, messageType);
		}
	}

	private void InitializeInternal()
	{
		UpdateInteractivityState(InteractivityState.NotInitialized);
		_buttons = new List<InteractiveButtonControl>();
		_controls = new List<InteractiveControl>();
		_groups = new List<InteractiveGroup>();
		_joysticks = new List<InteractiveJoystickControl>();
		_participants = new List<InteractiveParticipant>();
		_scenes = new List<InteractiveScene>();
		_websocketHosts = new List<string>();
		_buttonStates = new Dictionary<string, _InternalButtonCountState>();
		_buttonStatesByParticipant = new Dictionary<uint, Dictionary<string, _InternalButtonState>>();
		if (!Application.isEditor)
		{
			LoggingLevel = LoggingLevel.None;
		}
		_joystickStates = new Dictionary<string, _InternalJoystickState>();
		_joystickStatesByParticipant = new Dictionary<uint, Dictionary<string, _InternalJoystickState>>();
		_mouseButtonStateByParticipant = new Dictionary<uint, _InternalMouseButtonState>();
		_mousePositionsByParticipant = new Dictionary<uint, Vector2>();
		_participantsWhoTriggeredGiveInput = new Dictionary<string, _InternalParticipantTrackingState>();
		_queuedControlPropertyUpdates = new Dictionary<string, Dictionary<string, _InternalControlPropertyUpdateData>>();
		_transactionIDsState = new Dictionary<string, InternalTransactionIDState>();
		_giveInputControlDataByParticipant = new Dictionary<string, Dictionary<uint, Dictionary<string, object>>>();
		_giveInputControlData = new Dictionary<string, Dictionary<string, object>>();
		_giveInputKeyValues = new Dictionary<string, object>();
		_textboxValuesByParticipant = new Dictionary<uint, Dictionary<string, string>>();
		_streamingAssetsPath = Application.streamingAssetsPath;
		CreateStorageDirectoryIfNotExists();
		mixerInteractiveHelper = MixerInteractiveHelper._SingletonInstance;
	}

	private void OnInternalRefreshShortCodeTimerCallback(object sender, MixerInteractiveHelper.InternalTimerCallbackEventArgs e)
	{
		mixerInteractiveHelper.OnInternalRefreshShortCodeTimerCallback -= OnInternalRefreshShortCodeTimerCallback;
		RefreshShortCode();
	}

	private void OnInternalReconnectTimerCallback(object sender, MixerInteractiveHelper.InternalTimerCallbackEventArgs e)
	{
		mixerInteractiveHelper.OnInternalReconnectTimerCallback -= OnInternalReconnectTimerCallback;
		VerifyAuthToken();
	}

	internal void _LogError(string message)
	{
		_LogError(message, ERROR_FAIL);
	}

	internal void _LogError(string message, int code)
	{
		_queuedEvents.Add(new InteractiveEventArgs(InteractiveEventType.Error, code, message));
		_Log(message, LoggingLevel.Minimal);
	}

	internal void _Log(string message, LoggingLevel level = LoggingLevel.Verbose)
	{
		if (LoggingLevel != 0 && (LoggingLevel != LoggingLevel.Minimal || level != LoggingLevel.Verbose))
		{
			Debug.Log(message);
		}
	}

	private void ClearPreviousControlState()
	{
		if (InteractivityState != InteractivityState.InteractivityEnabled)
		{
			return;
		}
		foreach (string item in new List<string>(_buttonStates.Keys))
		{
			_InternalButtonCountState internalButtonCountState = _buttonStates[item];
			_InternalButtonCountState value = default(_InternalButtonCountState);
			value.PreviousCountOfButtonDownEvents = internalButtonCountState.CountOfButtonDownEvents;
			value.CountOfButtonDownEvents = internalButtonCountState.NextCountOfButtonDownEvents;
			value.NextCountOfButtonDownEvents = 0u;
			value.PreviousCountOfButtonPressEvents = internalButtonCountState.CountOfButtonPressEvents;
			value.CountOfButtonPressEvents = internalButtonCountState.NextCountOfButtonPressEvents;
			value.NextCountOfButtonPressEvents = 0u;
			value.PreviousCountOfButtonUpEvents = internalButtonCountState.CountOfButtonUpEvents;
			value.CountOfButtonUpEvents = internalButtonCountState.NextCountOfButtonUpEvents;
			value.NextCountOfButtonUpEvents = 0u;
			value.PreviousTransactionID = internalButtonCountState.TransactionID;
			value.TransactionID = internalButtonCountState.NextTransactionID;
			value.NextTransactionID = string.Empty;
			_buttonStates[item] = value;
		}
		foreach (uint item2 in new List<uint>(_buttonStatesByParticipant.Keys))
		{
			foreach (string item3 in new List<string>(_buttonStatesByParticipant[item2].Keys))
			{
				_InternalButtonState internalButtonState = _buttonStatesByParticipant[item2][item3];
				_InternalButtonState value2 = default(_InternalButtonState);
				_InternalButtonCountState buttonCountState = default(_InternalButtonCountState);
				buttonCountState.PreviousCountOfButtonDownEvents = internalButtonState.ButtonCountState.CountOfButtonDownEvents;
				buttonCountState.CountOfButtonDownEvents = internalButtonState.ButtonCountState.NextCountOfButtonDownEvents;
				buttonCountState.NextCountOfButtonDownEvents = 0u;
				buttonCountState.PreviousCountOfButtonPressEvents = internalButtonState.ButtonCountState.CountOfButtonPressEvents;
				buttonCountState.CountOfButtonPressEvents = internalButtonState.ButtonCountState.NextCountOfButtonPressEvents;
				buttonCountState.NextCountOfButtonPressEvents = 0u;
				buttonCountState.PreviousCountOfButtonUpEvents = internalButtonState.ButtonCountState.CountOfButtonUpEvents;
				buttonCountState.CountOfButtonUpEvents = internalButtonState.ButtonCountState.NextCountOfButtonUpEvents;
				buttonCountState.NextCountOfButtonUpEvents = 0u;
				value2.ButtonCountState = buttonCountState;
				_buttonStatesByParticipant[item2][item3] = value2;
			}
		}
		foreach (uint item4 in new List<uint>(_mouseButtonStateByParticipant.Keys))
		{
			_InternalMouseButtonState internalMouseButtonState = _mouseButtonStateByParticipant[item4];
			_InternalMouseButtonState value3 = default(_InternalMouseButtonState);
			if (internalMouseButtonState.NextIsDown)
			{
				value3.IsDown = true;
				value3.IsPressed = true;
				value3.IsUp = false;
				value3.NextIsDown = false;
				value3.NextIsPressed = true;
				value3.NextIsUp = false;
			}
			else if (internalMouseButtonState.NextIsUp)
			{
				value3.IsDown = false;
				value3.IsPressed = false;
				value3.IsUp = true;
				value3.NextIsDown = false;
				value3.NextIsPressed = false;
				value3.NextIsUp = false;
			}
			else if (internalMouseButtonState.NextIsPressed)
			{
				value3.IsDown = false;
				value3.IsPressed = true;
				value3.IsUp = false;
				value3.NextIsDown = false;
				value3.NextIsPressed = true;
				value3.NextIsUp = false;
			}
			else
			{
				value3.IsDown = false;
				value3.IsPressed = false;
				value3.IsUp = false;
				value3.NextIsDown = false;
				value3.NextIsPressed = false;
				value3.NextIsUp = false;
			}
			_mouseButtonStateByParticipant[item4] = value3;
		}
		foreach (string item5 in new List<string>(_participantsWhoTriggeredGiveInput.Keys))
		{
			_InternalParticipantTrackingState internalParticipantTrackingState = _participantsWhoTriggeredGiveInput[item5];
			_InternalParticipantTrackingState value4 = default(_InternalParticipantTrackingState);
			value4.previousParticpant = internalParticipantTrackingState.particpant;
			value4.particpant = internalParticipantTrackingState.nextParticpant;
			value4.nextParticpant = null;
			_participantsWhoTriggeredGiveInput[item5] = value4;
		}
		foreach (string item6 in new List<string>(_transactionIDsState.Keys))
		{
			InternalTransactionIDState internalTransactionIDState = _transactionIDsState[item6];
			InternalTransactionIDState value5 = default(InternalTransactionIDState);
			value5.previousTransactionID = internalTransactionIDState.transactionID;
			value5.transactionID = internalTransactionIDState.nextTransactionID;
			value5.nextTransactionID = string.Empty;
			_transactionIDsState[item6] = value5;
		}
	}

	private void UpdateInternalButtonState(InteractiveButtonEventArgs e)
	{
		uint userID = e.Participant.UserID;
		string controlID = e.ControlID;
		_InternalButtonState value3;
		if (!_buttonStatesByParticipant.TryGetValue(userID, out var value))
		{
			value = new Dictionary<string, _InternalButtonState>();
			_InternalButtonState value2 = default(_InternalButtonState);
			value2.IsDown = e.IsPressed;
			value2.IsPressed = e.IsPressed;
			value2.IsUp = !e.IsPressed;
			value.Add(controlID, value2);
			_buttonStatesByParticipant.Add(userID, value);
		}
		else if (!value.TryGetValue(controlID, out value3))
		{
			value3 = default(_InternalButtonState);
			_InternalButtonState value4 = default(_InternalButtonState);
			value4.IsDown = e.IsPressed;
			value4.IsPressed = e.IsPressed;
			value4.IsUp = !e.IsPressed;
			value.Add(controlID, value4);
		}
		bool flag = _buttonStatesByParticipant[userID][controlID].ButtonCountState.NextCountOfButtonPressEvents != 0;
		bool isPressed = e.IsPressed;
		_InternalButtonState value5 = _buttonStatesByParticipant[userID][controlID];
		if (isPressed)
		{
			if (!flag)
			{
				value5.IsDown = true;
				value5.IsPressed = true;
				value5.IsUp = false;
			}
			else
			{
				value5.IsDown = false;
				value5.IsPressed = true;
				value5.IsUp = false;
			}
		}
		else
		{
			value5.IsDown = false;
			value5.IsPressed = false;
			value5.IsUp = true;
		}
		_InternalButtonCountState buttonCountState = value5.ButtonCountState;
		if (value5.IsDown)
		{
			buttonCountState.NextCountOfButtonDownEvents++;
		}
		if (value5.IsPressed)
		{
			buttonCountState.NextCountOfButtonPressEvents++;
		}
		if (value5.IsUp)
		{
			buttonCountState.NextCountOfButtonUpEvents++;
		}
		if (!string.IsNullOrEmpty(e.TransactionID))
		{
			buttonCountState.NextTransactionID = e.TransactionID;
		}
		value5.ButtonCountState = buttonCountState;
		_buttonStatesByParticipant[userID][controlID] = value5;
		if (_buttonStates.TryGetValue(controlID, out var _))
		{
			_buttonStates[controlID] = value5.ButtonCountState;
		}
		else
		{
			_buttonStates.Add(controlID, value5.ButtonCountState);
		}
	}

	private void UpdateInternalJoystickState(InteractiveJoystickEventArgs e)
	{
		uint userID = e.Participant.UserID;
		string controlID = e.ControlID;
		_InternalJoystickState value2;
		if (!_joystickStatesByParticipant.TryGetValue(userID, out var value))
		{
			value = new Dictionary<string, _InternalJoystickState>();
			value2 = default(_InternalJoystickState);
			value2.X = e.X;
			value2.Y = e.Y;
			value2.countOfUniqueJoystickInputs = 1;
			_joystickStatesByParticipant.Add(userID, value);
		}
		else
		{
			value2 = default(_InternalJoystickState);
			if (!value.TryGetValue(controlID, out value2))
			{
				value2.X = e.X;
				value2.Y = e.Y;
				value2.countOfUniqueJoystickInputs = 1;
				value.Add(controlID, value2);
			}
			int countOfUniqueJoystickInputs = value2.countOfUniqueJoystickInputs;
			value2.X = value2.X * (double)(countOfUniqueJoystickInputs - 1) / (double)countOfUniqueJoystickInputs + e.X * (double)(1 / countOfUniqueJoystickInputs);
			value2.Y = value2.Y * (double)(countOfUniqueJoystickInputs - 1) / (double)countOfUniqueJoystickInputs + e.Y * (double)(1 / countOfUniqueJoystickInputs);
		}
		_joystickStatesByParticipant[e.Participant.UserID][e.ControlID] = value2;
		if (!value.TryGetValue(controlID, out var value3))
		{
			value3.X = e.X;
			value3.Y = e.Y;
			value3.countOfUniqueJoystickInputs = 1;
			value.Add(controlID, value3);
		}
		value3.countOfUniqueJoystickInputs++;
		int countOfUniqueJoystickInputs2 = value3.countOfUniqueJoystickInputs;
		value3.X = value3.X * (double)(countOfUniqueJoystickInputs2 - 1) / (double)countOfUniqueJoystickInputs2 + e.X * (double)(1 / countOfUniqueJoystickInputs2);
		value3.Y = value3.Y * (double)(countOfUniqueJoystickInputs2 - 1) / (double)countOfUniqueJoystickInputs2 + e.Y * (double)(1 / countOfUniqueJoystickInputs2);
		_joystickStates[e.ControlID] = value3;
	}

	private void UpdateInternalTextBoxState(InteractiveTextEventArgs e)
	{
		uint userID = e.Participant.UserID;
		string controlID = e.ControlID;
		string text = e.Text;
		string value = string.Empty;
		if (!_textboxValuesByParticipant.TryGetValue(userID, out var value2))
		{
			value2 = new Dictionary<string, string>();
			value2.Add(controlID, text);
			_textboxValuesByParticipant.Add(userID, value2);
		}
		else if (!value2.TryGetValue(controlID, out value))
		{
			value2.Add(controlID, text);
		}
		_textboxValuesByParticipant[e.Participant.UserID][e.ControlID] = text;
	}

	private void UpdateInternalMouseButtonState(InteractiveMouseButtonEventArgs e)
	{
		uint userID = e.Participant.UserID;
		bool isPressed = e.IsPressed;
		if (!_mouseButtonStateByParticipant.TryGetValue(userID, out var value))
		{
			value = default(_InternalMouseButtonState);
			value.IsDown = false;
			value.IsPressed = false;
			value.IsUp = false;
			value.NextIsDown = e.IsPressed;
			value.NextIsPressed = e.IsPressed;
			value.NextIsUp = !e.IsPressed;
			_mouseButtonStateByParticipant.Add(userID, value);
		}
		_InternalMouseButtonState value2 = _mouseButtonStateByParticipant[userID];
		value2.NextIsDown = isPressed;
		value2.NextIsPressed = isPressed;
		value2.NextIsUp = !isPressed;
		_mouseButtonStateByParticipant[userID] = value2;
	}

	internal void _QueuePropertyUpdate(string sceneID, string controlID, string name, bool value)
	{
		_KnownControlPropertyPrimitiveTypes type = _KnownControlPropertyPrimitiveTypes.Boolean;
		_QueuePropertyUpdateImpl(sceneID, controlID, name, type, value);
	}

	internal void _QueuePropertyUpdate(string sceneID, string controlID, string name, double value)
	{
		_KnownControlPropertyPrimitiveTypes type = _KnownControlPropertyPrimitiveTypes.Number;
		_QueuePropertyUpdateImpl(sceneID, controlID, name, type, value);
	}

	internal void _QueuePropertyUpdate(string sceneID, string controlID, string name, string value)
	{
		_KnownControlPropertyPrimitiveTypes type = _KnownControlPropertyPrimitiveTypes.String;
		_QueuePropertyUpdateImpl(sceneID, controlID, name, type, value);
	}

	internal void _QueuePropertyUpdate(string sceneID, string controlID, string name, object value)
	{
		_KnownControlPropertyPrimitiveTypes type = _KnownControlPropertyPrimitiveTypes.Unknown;
		_QueuePropertyUpdateImpl(sceneID, controlID, name, type, value);
	}

	internal void _QueuePropertyUpdateImpl(string sceneID, string controlID, string name, _KnownControlPropertyPrimitiveTypes type, object value)
	{
		if (!_queuedControlPropertyUpdates.ContainsKey(sceneID))
		{
			_InternalControlPropertyUpdateData value2 = new _InternalControlPropertyUpdateData(name, type, value);
			Dictionary<string, _InternalControlPropertyUpdateData> dictionary = new Dictionary<string, _InternalControlPropertyUpdateData>();
			dictionary.Add(controlID, value2);
			_queuedControlPropertyUpdates.Add(sceneID, dictionary);
			return;
		}
		Dictionary<string, _InternalControlPropertyUpdateData> dictionary2 = _queuedControlPropertyUpdates[sceneID];
		if (!dictionary2.ContainsKey(controlID))
		{
			_InternalControlPropertyUpdateData value3 = new _InternalControlPropertyUpdateData(name, type, value);
			_queuedControlPropertyUpdates[sceneID].Add(controlID, value3);
			return;
		}
		_InternalControlPropertyUpdateData internalControlPropertyUpdateData = dictionary2[controlID];
		_InternalControlPropertyMetaData value4 = new _InternalControlPropertyMetaData
		{
			type = type
		};
		switch (type)
		{
		case _KnownControlPropertyPrimitiveTypes.Boolean:
			value4.boolValue = (bool)value;
			break;
		case _KnownControlPropertyPrimitiveTypes.Number:
			value4.numberValue = (double)value;
			break;
		default:
			value4.stringValue = value.ToString();
			break;
		}
		if (!internalControlPropertyUpdateData.properties.ContainsKey(name))
		{
			_queuedControlPropertyUpdates[sceneID][controlID].properties.Add(name, value4);
		}
		else
		{
			_queuedControlPropertyUpdates[sceneID][controlID].properties[name] = value4;
		}
	}

	internal void _RegisterControlForValueUpdates(string controlTypeName, List<string> valuesToTrack)
	{
		if (!_giveInputControlData.ContainsKey(controlTypeName))
		{
			Dictionary<string, object> value = new Dictionary<string, object>();
			_giveInputControlData[controlTypeName] = value;
		}
		foreach (string item in valuesToTrack)
		{
			if (!_giveInputKeyValues.ContainsKey(item))
			{
				_giveInputKeyValues.Add(item, null);
			}
		}
	}

	internal string _InteractiveControlPropertyToString(InteractiveControlProperty property)
	{
		string result = string.Empty;
		switch (property)
		{
		case InteractiveControlProperty.Text:
			result = "text";
			break;
		case InteractiveControlProperty.BackgroundColor:
			result = "backgroundColor";
			break;
		case InteractiveControlProperty.BackgroundImage:
			result = "backgroundImage";
			break;
		case InteractiveControlProperty.TextColor:
			result = "textColor";
			break;
		case InteractiveControlProperty.TextSize:
			result = "textSize";
			break;
		case InteractiveControlProperty.BorderColor:
			result = "borderColor";
			break;
		case InteractiveControlProperty.FocusColor:
			result = "focusColor";
			break;
		case InteractiveControlProperty.AccentColor:
			result = "accentColor";
			break;
		}
		return result;
	}
}
