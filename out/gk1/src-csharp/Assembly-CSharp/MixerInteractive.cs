using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Microsoft;
using Microsoft.Mixer;
using UnityEngine;
using UnityEngine.Networking;

public class MixerInteractive : MonoBehaviour
{
	public delegate void OnErrorEventHandler(object sender, InteractiveEventArgs e);

	public delegate void OnGoInteractiveHandler(object sender, InteractiveEventArgs e);

	public delegate void OnInteractivityStateChangedHandler(object sender, InteractivityStateChangedEventArgs e);

	public delegate void OnParticipantStateChangedHandler(object sender, InteractiveParticipantStateChangedEventArgs e);

	public delegate void OnInteractiveButtonEventHandler(object sender, InteractiveButtonEventArgs e);

	public delegate void OnInteractiveJoystickControlEventHandler(object sender, InteractiveJoystickEventArgs e);

	public delegate void OnInteractiveMouseButtonEventHandler(object sender, InteractiveMouseButtonEventArgs e);

	public delegate void OnInteractiveCoordinatesChangedHandler(object sender, InteractiveCoordinatesChangedEventArgs e);

	public delegate void OnInteractiveTextControlEventHandler(object sender, InteractiveTextEventArgs e);

	public delegate void OnInteractiveMessageEventHandler(object sender, InteractiveMessageEventArgs e);

	public class RpcCachedMethodInfo
	{
		public MonoBehaviour owningMonoBehavior;

		public MethodInfo methodInfo;
	}

	public class ObservedCachedFieldInfo
	{
		public FieldInfo fieldInfo;

		public object owningObject;

		public float updateInterval;

		public float lastSendTime;

		public string previousValueAsString;
	}

	public struct MixerHelperParameterInfo
	{
		public string typeName;

		public string typeValue;
	}

	public bool runInBackground = true;

	public string defaultSceneID;

	public List<string> groupIDs;

	public List<string> sceneIDs;

	private static InteractivityManager interactivityManager;

	private static List<InteractiveEventArgs> queuedEvents;

	private static bool previousRunInBackgroundValue;

	private static MixerInteractiveDialog mixerDialog;

	private static bool pendingGoInteractive;

	private static string outstandingSetDefaultSceneRequest;

	private static List<string> outstandingCreateGroupsRequests;

	private static bool outstandingRequestsCompleted;

	private static float lastCheckForOutstandingRequestsTime;

	private static bool processedSerializedProperties;

	private static bool hasFiredGoInteractiveEvent;

	private static bool shouldCheckForOutstandingRequests;

	public GameObject addNewRpcMethodSource;

	public List<string> rpcOwningMonoBehaviorNames;

	public List<string> rpcMethodNames;

	private static List<string> outboundMessages;

	internal static Websocket _websocket;

	private static BackgroundWorker backgroundWorker;

	private const string DEFAULT_GROUP_ID = "default";

	private const float CHECK_FOR_OUTSTANDING_REQUESTS_INTERVAL = 1f;

	internal const float _DEFAULT_MIXER_SYNCVAR_UPDATE_INTERVAL = 1f;

	public static string Token
	{
		get
		{
			return InteractivityManager.SingletonInstance._authToken;
		}
		set
		{
			InteractivityManager.SingletonInstance._authToken = value;
		}
	}

	public static InteractivityState InteractivityState => InteractivityManager.SingletonInstance.InteractivityState;

	public static IList<InteractiveGroup> Groups => InteractivityManager.SingletonInstance.Groups;

	public static IList<InteractiveScene> Scenes => InteractivityManager.SingletonInstance.Scenes;

	public static IList<InteractiveParticipant> Participants => InteractivityManager.SingletonInstance.Participants;

	public static IList<InteractiveButtonControl> Buttons => InteractivityManager.SingletonInstance.Buttons;

	public static IList<InteractiveJoystickControl> Joysticks => InteractivityManager.SingletonInstance.Joysticks;

	public static bool ManuallyHandleSparkTransactions { get; set; }

	public static Vector3 MousePosition
	{
		get
		{
			Vector3 zero = Vector3.zero;
			if (InteractivityManager._mousePositionsByParticipant.Count > 0)
			{
				Dictionary<uint, Vector2> mousePositionsByParticipant = InteractivityManager._mousePositionsByParticipant;
				Dictionary<uint, Vector2>.KeyCollection keys = mousePositionsByParticipant.Keys;
				float num = 0f;
				float num2 = 0f;
				foreach (uint item in keys)
				{
					num += mousePositionsByParticipant[item].x;
					num2 += mousePositionsByParticipant[item].y;
				}
				zero.x = num / (float)keys.Count;
				zero.y = num2 / (float)keys.Count;
			}
			return zero;
		}
	}

	public static string ShortCode => InteractivityManager.SingletonInstance.ShortCode;

	public static event OnErrorEventHandler OnError;

	public static event OnGoInteractiveHandler OnGoInteractive;

	public static event OnInteractivityStateChangedHandler OnInteractivityStateChanged;

	public static event OnParticipantStateChangedHandler OnParticipantStateChanged;

	public static event OnInteractiveButtonEventHandler OnInteractiveButtonEvent;

	public static event OnInteractiveJoystickControlEventHandler OnInteractiveJoystickControlEvent;

	public static event OnInteractiveMouseButtonEventHandler OnInteractiveMouseButtonEvent;

	public static event OnInteractiveCoordinatesChangedHandler OnInteractiveCoordinatesChangedEvent;

	public static event OnInteractiveTextControlEventHandler OnInteractiveTextControlEvent;

	public static event OnInteractiveMessageEventHandler OnInteractiveMessageEvent;

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.transform.gameObject);
		base.gameObject.AddComponent<MixerInteractiveHelper>();
	}

	private void Start()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (mixerDialog == null)
		{
			mixerDialog = UnityEngine.Object.FindObjectOfType<MixerInteractiveDialog>();
		}
		if (queuedEvents == null)
		{
			queuedEvents = new List<InteractiveEventArgs>();
		}
		bool flag = false;
		if (interactivityManager == null)
		{
			interactivityManager = InteractivityManager.SingletonInstance;
			interactivityManager.OnError -= HandleError;
			interactivityManager.OnInteractivityStateChanged -= HandleInteractivityStateChanged;
			interactivityManager.OnParticipantStateChanged -= HandleParticipantStateChanged;
			interactivityManager.OnInteractiveButtonEvent -= HandleInteractiveButtonEvent;
			interactivityManager.OnInteractiveJoystickControlEvent -= HandleInteractiveJoystickControlEvent;
			interactivityManager.OnInteractiveMouseButtonEvent -= HandleInteractiveMouseButtonEvent;
			interactivityManager.OnInteractiveCoordinatesChangedEvent -= HandleInteractiveCoordinatesChangedHandler;
			interactivityManager.OnInteractiveTextControlEvent -= HandleInteractiveTextControlEvent;
			interactivityManager.OnInteractiveMessageEvent -= HandleInteractiveMessageEvent;
			interactivityManager.OnError += HandleError;
			interactivityManager.OnInteractivityStateChanged += HandleInteractivityStateChanged;
			interactivityManager.OnParticipantStateChanged += HandleParticipantStateChanged;
			interactivityManager.OnInteractiveButtonEvent += HandleInteractiveButtonEvent;
			interactivityManager.OnInteractiveMouseButtonEvent += HandleInteractiveMouseButtonEvent;
			interactivityManager.OnInteractiveCoordinatesChangedEvent += HandleInteractiveCoordinatesChangedHandler;
			interactivityManager.OnInteractiveJoystickControlEvent += HandleInteractiveJoystickControlEvent;
			interactivityManager.OnInteractiveTextControlEvent += HandleInteractiveTextControlEvent;
			interactivityManager.OnInteractiveMessageEvent += HandleInteractiveMessageEvent;
		}
		else
		{
			flag = true;
		}
		MixerInteractiveHelper singletonInstance = MixerInteractiveHelper._SingletonInstance;
		singletonInstance._runInBackgroundIfInteractive = runInBackground;
		singletonInstance._defaultSceneID = defaultSceneID;
		for (int i = 0; i < groupIDs.Count; i++)
		{
			string text = groupIDs[i];
			if (text != string.Empty && !singletonInstance._groupSceneMapping.ContainsKey(text))
			{
				singletonInstance._groupSceneMapping.Add(text, sceneIDs[i]);
			}
		}
		if (outstandingCreateGroupsRequests == null)
		{
			outstandingCreateGroupsRequests = new List<string>();
		}
		outstandingSetDefaultSceneRequest = string.Empty;
		processedSerializedProperties = false;
		outstandingRequestsCompleted = false;
		shouldCheckForOutstandingRequests = false;
		lastCheckForOutstandingRequestsTime = -1f;
		outboundMessages = new List<string>();
		backgroundWorker = new BackgroundWorker();
		if (flag && InteractivityManager.SingletonInstance.InteractivityState == InteractivityState.InteractivityEnabled)
		{
			ProcessSerializedProperties();
		}
		_websocket = base.gameObject.AddComponent<Websocket>();
		InteractivityManager.SingletonInstance.SetWebsocketInstance(_websocket);
	}

	private static void HandleInteractiveJoystickControlEvent(object sender, InteractiveJoystickEventArgs e)
	{
		queuedEvents.Add(e);
	}

	private static void HandleInteractiveMouseButtonEvent(object sender, InteractiveMouseButtonEventArgs e)
	{
		queuedEvents.Add(e);
	}

	private static void HandleInteractiveCoordinatesChangedHandler(object sender, InteractiveCoordinatesChangedEventArgs e)
	{
		queuedEvents.Add(e);
	}

	private static void HandleInteractiveButtonEvent(object sender, InteractiveButtonEventArgs e)
	{
		queuedEvents.Add(e);
	}

	private void HandleInteractiveTextControlEvent(object sender, InteractiveTextEventArgs e)
	{
		queuedEvents.Add(e);
	}

	private static void HandleParticipantStateChanged(object sender, InteractiveEventArgs e)
	{
		queuedEvents.Add(e);
	}

	private static void HandleInteractivityStateChanged(object sender, InteractivityStateChangedEventArgs e)
	{
		queuedEvents.Add(e);
	}

	private static void HandleError(object sender, InteractiveEventArgs e)
	{
		queuedEvents.Add(e);
	}

	private static void HandleInteractiveMessageEvent(object sender, InteractiveEventArgs e)
	{
		queuedEvents.Add(e);
	}

	internal static void InvokeRpcMethod(string methodName, List<MixerHelperParameterInfo> mixerParameterInfos)
	{
		if (!FindAndInvokeRpcMethod(methodName, mixerParameterInfos))
		{
			RefreshRPCMethods();
			FindAndInvokeRpcMethod(methodName, mixerParameterInfos);
		}
	}

	private static bool FindAndInvokeRpcMethod(string methodName, List<MixerHelperParameterInfo> mixerParameterInfos)
	{
		bool result = false;
		RpcCachedMethodInfo value = new RpcCachedMethodInfo();
		if (MixerInteractiveHelper._SingletonInstance.cachedRPCMethods.TryGetValue(methodName, out value))
		{
			MethodInfo methodInfo = value.methodInfo;
			ParameterInfo[] parameters = methodInfo.GetParameters();
			object[] array = new object[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
			{
				ParameterInfo parameterInfo = parameters[i];
				string typeValue = mixerParameterInfos[i].typeValue;
				array[i] = Convert.ChangeType(typeValue, parameterInfo.ParameterType);
			}
			result = true;
			try
			{
				methodInfo.Invoke(value.owningMonoBehavior, array);
			}
			catch (Exception ex)
			{
				Debug.Log("Error calling method " + value.owningMonoBehavior.name + "." + methodInfo.Name + ". Details: " + ex.Message);
			}
		}
		return result;
	}

	public static void AddRpcMethodsFromTheEditor(GameObject owningGameObject, List<string> rpcMethodNames)
	{
		if (rpcMethodNames.Count == 0)
		{
			return;
		}
		MixerInteractiveHelper singletonInstance = MixerInteractiveHelper._SingletonInstance;
		MonoBehaviour[] components = owningGameObject.GetComponents<MonoBehaviour>();
		foreach (string rpcMethodName in rpcMethodNames)
		{
			string text = TrimMethodName(rpcMethodName);
			MonoBehaviour[] array = components;
			foreach (MonoBehaviour monoBehaviour in array)
			{
				MethodInfo[] methods = monoBehaviour.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				foreach (MethodInfo methodInfo in methods)
				{
					if (methodInfo.Name == text && !singletonInstance.cachedRPCMethods.ContainsKey(methodInfo.Name))
					{
						singletonInstance.cachedRPCMethods.Add(methodInfo.Name, new RpcCachedMethodInfo
						{
							owningMonoBehavior = monoBehaviour,
							methodInfo = methodInfo
						});
					}
				}
			}
		}
	}

	internal static string TrimMethodName(string unTrimmedMethodName)
	{
		return unTrimmedMethodName.Split('(')[0].Trim();
	}

	private static void RefreshRPCMethods(bool includeMethodsFromTheInspector = false)
	{
		MixerInteractiveHelper singletonInstance = MixerInteractiveHelper._SingletonInstance;
		MonoBehaviour[] array = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in array)
		{
			MethodInfo[] methods = monoBehaviour.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.IsDefined(typeof(MixerRpcMethod), inherit: false) && !singletonInstance.cachedRPCMethods.ContainsKey(methodInfo.Name))
				{
					singletonInstance.cachedRPCMethods.Add(methodInfo.Name, new RpcCachedMethodInfo
					{
						owningMonoBehavior = monoBehaviour,
						methodInfo = methodInfo
					});
				}
				if (!includeMethodsFromTheInspector)
				{
					continue;
				}
				string text = monoBehaviour.name;
				for (int k = 0; k < singletonInstance.rpcOwningMonoBehaviorNames.Count; k++)
				{
					if (singletonInstance.rpcOwningMonoBehaviorNames[k] == text && singletonInstance.rpcMethodNames[k] == methodInfo.Name && !singletonInstance.cachedRPCMethods.ContainsKey(methodInfo.Name))
					{
						singletonInstance.cachedRPCMethods.Add(methodInfo.Name, new RpcCachedMethodInfo
						{
							owningMonoBehavior = monoBehaviour,
							methodInfo = methodInfo
						});
					}
				}
			}
		}
	}

	public static void FlushUpdates()
	{
		SendOutboundMessages();
	}

	internal static void SendOutboundMessages()
	{
		foreach (string outboundMessage in outboundMessages)
		{
			InteractivityManager.SingletonInstance.SendMessage(outboundMessage);
		}
	}

	private static void SerializeSyncVars()
	{
		MonoBehaviour[] array = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
		foreach (MonoBehaviour monoBehaviour in array)
		{
			FieldInfo[] fields = monoBehaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (Attribute.GetCustomAttribute(fieldInfo, typeof(MixerSyncVar)) is MixerSyncVar)
				{
					object value = fieldInfo.GetValue(monoBehaviour);
					string value2 = string.Empty;
					if (value != null)
					{
						value2 = value.ToString();
					}
					ParseAndSendCustomMessage(fieldInfo.Name, value2);
				}
			}
		}
	}

	private static void ParseAndSendCustomMessage(string name, string value)
	{
		InteractivityManager.SingletonInstance.SendMessage("{   name: " + name + "   value: " + value + "}");
	}

	internal static void QueueCustomMessage(string newMessage)
	{
		if (outboundMessages == null)
		{
			outboundMessages = new List<string>();
		}
		outboundMessages.Add(newMessage);
	}

	internal static int GetTimeSinceStartUpInMilliSeconds()
	{
		return (int)Time.realtimeSinceStartup * 1000;
	}

	public static InteractiveParticipant GetParticipantWhoGaveInputForControl(string controlID)
	{
		InteractiveParticipant result = null;
		_InternalParticipantTrackingState value = default(_InternalParticipantTrackingState);
		if (InteractivityManager._participantsWhoTriggeredGiveInput.TryGetValue(controlID, out value))
		{
			result = value.particpant;
		}
		return result;
	}

	public static bool HasSubmissions(string controlID)
	{
		bool flag = false;
		if (GetControl(controlID) is InteractiveTextControl && GetText(controlID).Count > 0)
		{
			flag = true;
		}
		if (flag)
		{
			CaptureTransactionForControlID(controlID);
		}
		return flag;
	}

	public static void Initialize(bool goInteractive = true)
	{
		InteractivityManager.SingletonInstance.Initialize(goInteractive);
	}

	public static void TriggerCooldown(string controlID, int cooldown)
	{
		InteractivityManager.SingletonInstance.TriggerCooldown(controlID, cooldown);
	}

	public static void StartInteractive()
	{
		InteractivityManager.SingletonInstance.StartInteractive();
	}

	public static void StopInteractive()
	{
		InteractivityManager.SingletonInstance.StopInteractive();
		pendingGoInteractive = false;
		if (MixerInteractiveHelper._SingletonInstance._runInBackgroundIfInteractive)
		{
			Application.runInBackground = previousRunInBackgroundValue;
		}
	}

	public static void DoWork()
	{
		InteractivityManager.SingletonInstance.DoWork();
		SendOutboundMessages();
	}

	public static void Dispose()
	{
		InteractivityManager singletonInstance = InteractivityManager.SingletonInstance;
		if (singletonInstance != null)
		{
			singletonInstance.OnInteractivityStateChanged -= HandleInteractivityStateChangedInternal;
			backgroundWorker.DoWork -= BackgroundWorkerDoWork;
		}
		if (queuedEvents != null)
		{
			queuedEvents.Clear();
		}
		previousRunInBackgroundValue = true;
		pendingGoInteractive = false;
		outstandingSetDefaultSceneRequest = string.Empty;
		if (outstandingCreateGroupsRequests != null)
		{
			outstandingCreateGroupsRequests.Clear();
		}
		outstandingRequestsCompleted = false;
		lastCheckForOutstandingRequestsTime = -1f;
		processedSerializedProperties = false;
		hasFiredGoInteractiveEvent = false;
		singletonInstance.Dispose();
	}

	private void ResetInternalState()
	{
		previousRunInBackgroundValue = true;
		outstandingSetDefaultSceneRequest = string.Empty;
		if (outstandingCreateGroupsRequests != null)
		{
			outstandingCreateGroupsRequests.Clear();
		}
		outstandingRequestsCompleted = false;
		lastCheckForOutstandingRequestsTime = -1f;
		processedSerializedProperties = false;
	}

	private void Update()
	{
		if (processedSerializedProperties && shouldCheckForOutstandingRequests && !outstandingRequestsCompleted && Time.time - lastCheckForOutstandingRequestsTime > 1f)
		{
			lastCheckForOutstandingRequestsTime = Time.time;
			outstandingRequestsCompleted = CheckForOutStandingRequestsCompleted();
		}
		DoWork();
		List<InteractiveEventArgs> list = new List<InteractiveEventArgs>();
		if (queuedEvents != null)
		{
			foreach (InteractiveEventArgs queuedEvent in queuedEvents)
			{
				if (queuedEvent == null)
				{
					continue;
				}
				switch (queuedEvent.EventType)
				{
				case InteractiveEventType.InteractivityStateChanged:
				{
					InteractivityStateChangedEventArgs interactivityStateChangedEventArgs = queuedEvent as InteractivityStateChangedEventArgs;
					if (interactivityStateChangedEventArgs.State == InteractivityState.InteractivityEnabled && (!shouldCheckForOutstandingRequests || outstandingRequestsCompleted) && !hasFiredGoInteractiveEvent && MixerInteractive.OnGoInteractive != null)
					{
						hasFiredGoInteractiveEvent = true;
						MixerInteractive.OnGoInteractive(this, interactivityStateChangedEventArgs);
					}
					if (MixerInteractive.OnInteractivityStateChanged != null)
					{
						MixerInteractive.OnInteractivityStateChanged(this, interactivityStateChangedEventArgs);
					}
					list.Add(queuedEvent);
					break;
				}
				case InteractiveEventType.ParticipantStateChanged:
					if (outstandingRequestsCompleted)
					{
						if (MixerInteractive.OnParticipantStateChanged != null)
						{
							MixerInteractive.OnParticipantStateChanged(this, queuedEvent as InteractiveParticipantStateChangedEventArgs);
						}
						list.Add(queuedEvent);
					}
					break;
				case InteractiveEventType.Button:
					if (MixerInteractive.OnInteractiveButtonEvent != null)
					{
						MixerInteractive.OnInteractiveButtonEvent(this, queuedEvent as InteractiveButtonEventArgs);
					}
					list.Add(queuedEvent);
					break;
				case InteractiveEventType.Joystick:
					if (MixerInteractive.OnInteractiveJoystickControlEvent != null)
					{
						MixerInteractive.OnInteractiveJoystickControlEvent(this, queuedEvent as InteractiveJoystickEventArgs);
					}
					list.Add(queuedEvent);
					break;
				case InteractiveEventType.MouseButton:
					if (MixerInteractive.OnInteractiveMouseButtonEvent != null)
					{
						MixerInteractive.OnInteractiveMouseButtonEvent(this, queuedEvent as InteractiveMouseButtonEventArgs);
					}
					list.Add(queuedEvent);
					break;
				case InteractiveEventType.Coordinates:
					if (MixerInteractive.OnInteractiveCoordinatesChangedEvent != null)
					{
						MixerInteractive.OnInteractiveCoordinatesChangedEvent(this, queuedEvent as InteractiveCoordinatesChangedEventArgs);
					}
					list.Add(queuedEvent);
					break;
				case InteractiveEventType.TextInput:
					if (MixerInteractive.OnInteractiveTextControlEvent != null)
					{
						MixerInteractive.OnInteractiveTextControlEvent(this, queuedEvent as InteractiveTextEventArgs);
					}
					list.Add(queuedEvent);
					break;
				case InteractiveEventType.Error:
					if (MixerInteractive.OnError != null)
					{
						MixerInteractive.OnError(this, queuedEvent);
					}
					list.Add(queuedEvent);
					break;
				default:
					if (MixerInteractive.OnInteractiveMessageEvent != null)
					{
						MixerInteractive.OnInteractiveMessageEvent(this, queuedEvent as InteractiveMessageEventArgs);
					}
					list.Add(queuedEvent);
					break;
				}
			}
			foreach (InteractiveEventArgs item in list)
			{
				queuedEvents.Remove(item);
			}
		}
		if (InteractivityManager.SingletonInstance.InteractivityState == InteractivityState.InteractivityEnabled && shouldCheckForOutstandingRequests && outstandingRequestsCompleted && !hasFiredGoInteractiveEvent && MixerInteractive.OnGoInteractive != null)
		{
			hasFiredGoInteractiveEvent = true;
			MixerInteractive.OnGoInteractive(this, new InteractiveEventArgs());
		}
	}

	public static bool GetButtonDown(string controlID)
	{
		bool buttonDown = InteractivityManager.SingletonInstance.GetButton(controlID).ButtonDown;
		if (buttonDown && !ManuallyHandleSparkTransactions)
		{
			CaptureTransactionForButtonControlID(controlID);
		}
		return buttonDown;
	}

	public static bool GetButton(string controlID)
	{
		bool buttonPressed = InteractivityManager.SingletonInstance.GetButton(controlID).ButtonPressed;
		if (buttonPressed && !ManuallyHandleSparkTransactions)
		{
			CaptureTransactionForButtonControlID(controlID);
		}
		return buttonPressed;
	}

	public static bool GetButtonUp(string controlID)
	{
		bool buttonUp = InteractivityManager.SingletonInstance.GetButton(controlID).ButtonUp;
		if (buttonUp && !ManuallyHandleSparkTransactions)
		{
			CaptureTransactionForButtonControlID(controlID);
		}
		return buttonUp;
	}

	public static uint GetCountOfButtonDowns(string controlID)
	{
		return InteractivityManager.SingletonInstance.GetButton(controlID).CountOfButtonDowns;
	}

	public static uint GetCountOfButtons(string controlID)
	{
		return InteractivityManager.SingletonInstance.GetButton(controlID).CountOfButtonPresses;
	}

	public static uint GetCountOfButtonUps(string controlID)
	{
		return InteractivityManager.SingletonInstance.GetButton(controlID).CountOfButtonUps;
	}

	public static InteractiveJoystickControl GetJoystick(string controlID)
	{
		return InteractivityManager.SingletonInstance.GetJoystick(controlID);
	}

	public static float GetJoystickX(string controlID)
	{
		return (float)InteractivityManager.SingletonInstance.GetJoystick(controlID).X;
	}

	public static float GetJoystickY(string controlID)
	{
		return (float)InteractivityManager.SingletonInstance.GetJoystick(controlID).Y;
	}

	public static bool GetMouseButtonDown(int buttonIndex = 0)
	{
		bool result = false;
		Dictionary<uint, _InternalMouseButtonState> mouseButtonStateByParticipant = InteractivityManager._mouseButtonStateByParticipant;
		foreach (uint key in mouseButtonStateByParticipant.Keys)
		{
			if (mouseButtonStateByParticipant[key].IsDown)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool GetMouseButton(int buttonIndex = 0)
	{
		bool result = false;
		Dictionary<uint, _InternalMouseButtonState> mouseButtonStateByParticipant = InteractivityManager._mouseButtonStateByParticipant;
		foreach (uint key in mouseButtonStateByParticipant.Keys)
		{
			if (mouseButtonStateByParticipant[key].IsPressed)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool GetMouseButtonUp(int buttonIndex = 0)
	{
		bool result = false;
		Dictionary<uint, _InternalMouseButtonState> mouseButtonStateByParticipant = InteractivityManager._mouseButtonStateByParticipant;
		foreach (uint key in mouseButtonStateByParticipant.Keys)
		{
			if (mouseButtonStateByParticipant[key].IsUp)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static InteractiveButtonControl Button(string controlID)
	{
		return InteractivityManager.SingletonInstance.GetButton(controlID);
	}

	public static string GetCurrentScene()
	{
		return InteractivityManager.SingletonInstance.GetCurrentScene();
	}

	public static void SetCurrentScene(string sceneID)
	{
		InteractivityManager.SingletonInstance.SetCurrentScene(sceneID);
	}

	public static InteractiveGroup GetGroup(string groupID)
	{
		return InteractivityManager.SingletonInstance.GetGroup(groupID);
	}

	public static InteractiveScene GetScene(string sceneID)
	{
		return InteractivityManager.SingletonInstance.GetScene(sceneID);
	}

	public static void SendInteractiveMessage(string message)
	{
		InteractivityManager.SingletonInstance.SendMessage(message);
	}

	public static void SendInteractiveMessage(string messageType, Dictionary<string, object> parameters)
	{
		InteractivityManager.SingletonInstance.SendMessage(messageType, parameters);
	}

	public static void ClearSavedLoginInformation()
	{
		PlayerPrefs.DeleteKey("MixerInteractive-AuthToken");
		PlayerPrefs.DeleteKey("MixerInteractive-RefreshToken");
		PlayerPrefs.Save();
	}

	private IEnumerator InitializeCoRoutine()
	{
		using UnityWebRequest request = UnityWebRequest.Get("https://mixer.com/api/v1/interactive/hosts");
		yield return request.SendWebRequest();
		if (request.isNetworkError)
		{
			Debug.Log("Error: Could not retrieve websocket URL. " + request.error);
			yield break;
		}
		_ = request.downloadHandler.text;
		InteractivityManager.SingletonInstance.Initialize(goInteractive: true, null);
	}

	public static InteractiveControl GetControl(string controlID)
	{
		return InteractivityManager.SingletonInstance._GetControl(controlID);
	}

	public static IList<InteractiveTextResult> GetText(string controlID)
	{
		return InteractivityManager.SingletonInstance._GetText(controlID);
	}

	public static void GoInteractive()
	{
		if (!pendingGoInteractive)
		{
			pendingGoInteractive = true;
			hasFiredGoInteractiveEvent = false;
			InteractivityManager singletonInstance = InteractivityManager.SingletonInstance;
			singletonInstance.OnInteractivityStateChanged -= HandleInteractivityStateChangedInternal;
			singletonInstance.OnInteractivityStateChanged += HandleInteractivityStateChangedInternal;
			if (backgroundWorker == null)
			{
				backgroundWorker = new BackgroundWorker();
			}
			backgroundWorker.DoWork -= BackgroundWorkerDoWork;
			backgroundWorker.DoWork += BackgroundWorkerDoWork;
			backgroundWorker.RunWorkerAsync();
			if (MixerInteractiveHelper._SingletonInstance._runInBackgroundIfInteractive)
			{
				previousRunInBackgroundValue = Application.runInBackground;
				Application.runInBackground = true;
			}
		}
	}

	private static void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
	{
		InteractivityManager.SingletonInstance.Initialize();
	}

	private static void HandleInteractivityStateChangedInternal(object sender, InteractivityStateChangedEventArgs e)
	{
		if (e == null)
		{
			return;
		}
		switch (e.State)
		{
		case InteractivityState.ShortCodeRequired:
			if (!mixerDialog.gameObject.activeInHierarchy)
			{
				mixerDialog.gameObject.SetActive(value: true);
			}
			mixerDialog.Show(InteractivityManager.SingletonInstance.ShortCode);
			break;
		case InteractivityState.InteractivityEnabled:
			mixerDialog.Hide();
			ProcessSerializedProperties();
			pendingGoInteractive = false;
			break;
		}
	}

	private static void ProcessSerializedProperties()
	{
		MixerInteractiveHelper singletonInstance = MixerInteractiveHelper._SingletonInstance;
		InteractivityManager singletonInstance2 = InteractivityManager.SingletonInstance;
		string text = singletonInstance._defaultSceneID;
		if (singletonInstance._groupSceneMapping.Count > 0 || text != string.Empty)
		{
			shouldCheckForOutstandingRequests = true;
		}
		if (singletonInstance._groupSceneMapping.Count > 0)
		{
			foreach (string key in singletonInstance._groupSceneMapping.Keys)
			{
				if (!(key == string.Empty))
				{
					string text2 = singletonInstance._groupSceneMapping[key];
					if (text2 != string.Empty)
					{
						new InteractiveGroup(key, text2);
					}
					else
					{
						new InteractiveGroup(key);
					}
					outstandingCreateGroupsRequests.Add(key);
				}
			}
			if (text != string.Empty)
			{
				singletonInstance2.SetCurrentScene(text);
				outstandingSetDefaultSceneRequest = text;
			}
		}
		processedSerializedProperties = true;
	}

	private static bool CheckForOutStandingRequestsCompleted()
	{
		bool result = false;
		List<string> list = new List<string>();
		if (outstandingSetDefaultSceneRequest == string.Empty)
		{
			foreach (string outstandingCreateGroupsRequest in outstandingCreateGroupsRequests)
			{
				foreach (InteractiveGroup group in InteractivityManager.SingletonInstance.Groups)
				{
					if (group.GroupID == outstandingCreateGroupsRequest)
					{
						list.Add(outstandingCreateGroupsRequest);
					}
				}
			}
			foreach (string item in list)
			{
				outstandingCreateGroupsRequests.Remove(item);
			}
		}
		else
		{
			foreach (InteractiveGroup group2 in InteractivityManager.SingletonInstance.Groups)
			{
				if (group2.GroupID == "default" && group2.SceneID == outstandingSetDefaultSceneRequest)
				{
					outstandingSetDefaultSceneRequest = string.Empty;
					break;
				}
			}
		}
		if (outstandingCreateGroupsRequests.Count == 0 && outstandingSetDefaultSceneRequest == string.Empty)
		{
			result = true;
		}
		return result;
	}

	private static void CaptureTransactionForButtonControlID(string controlID)
	{
		_ = Buttons;
		foreach (string key in InteractivityManager._buttonStates.Keys)
		{
			if (key == controlID)
			{
				InteractivityManager.SingletonInstance.CaptureTransaction(InteractivityManager._buttonStates[key].TransactionID);
				break;
			}
		}
	}

	private static void CaptureTransactionForControlID(string controlID)
	{
		foreach (string key in InteractivityManager._transactionIDsState.Keys)
		{
			if (key == controlID)
			{
				InteractivityManager.SingletonInstance.CaptureTransaction(InteractivityManager._transactionIDsState[key].transactionID);
				break;
			}
		}
	}

	private void OnDestroy()
	{
		ResetInternalState();
	}

	private void OnApplicationQuit()
	{
		StopInteractive();
	}
}
