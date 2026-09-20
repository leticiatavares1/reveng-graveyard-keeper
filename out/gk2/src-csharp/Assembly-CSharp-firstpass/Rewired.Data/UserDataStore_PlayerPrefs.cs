using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Rewired.Utils.Attributes;
using Rewired.Utils.Libraries.TinyJson;
using UnityEngine;

namespace Rewired.Data;

public class UserDataStore_PlayerPrefs : UserDataStore
{
	private class ControllerAssignmentSaveInfo
	{
		public class PlayerInfo
		{
			public int id;

			public bool hasKeyboard;

			public bool hasMouse;

			public JoystickInfo[] joysticks;

			public int joystickCount
			{
				get
				{
					if (joysticks == null)
					{
						return 0;
					}
					return joysticks.Length;
				}
			}

			public int IndexOfJoystick(int joystickId)
			{
				for (int i = 0; i < joystickCount; i++)
				{
					if (joysticks[i] != null && joysticks[i].id == joystickId)
					{
						return i;
					}
				}
				return -1;
			}

			public bool ContainsJoystick(int joystickId)
			{
				return IndexOfJoystick(joystickId) >= 0;
			}
		}

		public class JoystickInfo
		{
			public Guid instanceGuid;

			public string hardwareIdentifier;

			public int id;
		}

		public PlayerInfo[] players;

		public int playerCount
		{
			get
			{
				if (players == null)
				{
					return 0;
				}
				return players.Length;
			}
		}

		public ControllerAssignmentSaveInfo()
		{
		}

		public ControllerAssignmentSaveInfo(int playerCount)
		{
			players = new PlayerInfo[playerCount];
			for (int i = 0; i < playerCount; i++)
			{
				players[i] = new PlayerInfo();
			}
		}

		public int IndexOfPlayer(int playerId)
		{
			for (int i = 0; i < playerCount; i++)
			{
				if (players[i] != null && players[i].id == playerId)
				{
					return i;
				}
			}
			return -1;
		}

		public bool ContainsPlayer(int playerId)
		{
			return IndexOfPlayer(playerId) >= 0;
		}
	}

	private class JoystickAssignmentHistoryInfo
	{
		public readonly Joystick joystick;

		public readonly int oldJoystickId;

		public JoystickAssignmentHistoryInfo(Joystick joystick, int oldJoystickId)
		{
			if (joystick == null)
			{
				throw new ArgumentNullException("joystick");
			}
			this.joystick = joystick;
			this.oldJoystickId = oldJoystickId;
		}
	}

	[Serializable]
	private class ControllerElementByRoleMap
	{
		[Serializable]
		public struct Entry
		{
			public int actionId;

			public ControllerElementType elementType;

			public AxisRange axisRange;

			public bool invert;

			public Pole axisContribution;

			public bool TryGetElementAssignment(ControllerType controllerType, Controller.Element targetElement, out ElementAssignment assignment)
			{
				if (targetElement.type == elementType)
				{
					assignment = ElementAssignment.CompleteAssignment(controllerType, targetElement.type, targetElement.elementIdentifier.id, axisRange, KeyCode.None, ModifierKeyFlags.None, actionId, axisContribution, invert);
					return true;
				}
				switch (elementType)
				{
				case ControllerElementType.Axis:
					if (targetElement.type == ControllerElementType.Button)
					{
						Pole pole = axisContribution;
						if (axisRange == AxisRange.Full && invert)
						{
							pole = Pole.Negative;
						}
						assignment = ElementAssignment.CompleteAssignment(controllerType, targetElement.type, targetElement.elementIdentifier.id, AxisRange.Full, KeyCode.None, ModifierKeyFlags.None, actionId, pole, invert: false);
						return true;
					}
					assignment = default(ElementAssignment);
					return false;
				case ControllerElementType.Button:
					if (targetElement.type == ControllerElementType.Axis)
					{
						assignment = ElementAssignment.CompleteAssignment(controllerType, targetElement.type, targetElement.elementIdentifier.id, AxisRange.Positive, KeyCode.None, ModifierKeyFlags.None, actionId, axisContribution, invert: false);
						return true;
					}
					assignment = default(ElementAssignment);
					return false;
				default:
					assignment = default(ElementAssignment);
					return false;
				}
			}

			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("actionId: ");
				stringBuilder.Append(actionId);
				stringBuilder.Append("\nelementType: ");
				stringBuilder.Append(elementType);
				stringBuilder.Append("\naxisRange: ");
				stringBuilder.Append(axisRange);
				stringBuilder.Append("\ninvert: ");
				stringBuilder.Append(invert);
				stringBuilder.Append("\naxisContribution: ");
				stringBuilder.Append(axisContribution);
				return stringBuilder.ToString();
			}
		}

		[DoNotSerialize]
		public string role;

		public List<Entry> data;

		[Preserve]
		public ControllerElementByRoleMap()
		{
			data = new List<Entry>();
		}

		public void Add(ActionElementMap elementMap)
		{
			data.Add(new Entry
			{
				actionId = elementMap.actionId,
				elementType = elementMap.elementType,
				axisRange = elementMap.axisRange,
				invert = elementMap.invert,
				axisContribution = elementMap.axisContribution
			});
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("role: ");
			stringBuilder.Append(role);
			stringBuilder.Append("\nentries:");
			stringBuilder.Append((data != null) ? data.Count : 0);
			stringBuilder.Append("\n");
			if (data != null)
			{
				for (int i = 0; i < data.Count; i++)
				{
					stringBuilder.Append("Entry[");
					stringBuilder.Append(i);
					stringBuilder.Append("]:\n");
					stringBuilder.Append(data[i]);
				}
			}
			return stringBuilder.ToString();
		}

		public string ToJson()
		{
			return JsonWriter.ToJson(this);
		}

		public static ControllerElementByRoleMap FromJson(string role, string json)
		{
			ControllerElementByRoleMap controllerElementByRoleMap = JsonParser.FromJson<ControllerElementByRoleMap>(json);
			if (controllerElementByRoleMap != null)
			{
				controllerElementByRoleMap.role = role;
			}
			return controllerElementByRoleMap;
		}
	}

	public enum ActionMappingSaveMode
	{
		ByController,
		ByControllerElementRole
	}

	private const string thisScriptName = "UserDataStore_PlayerPrefs";

	private const string logPrefix = "Rewired: ";

	private const string playerPrefsKeySuffix_controllerAssignments = "ControllerAssignments";

	private const int controllerMapPPKeyVersion_original = 0;

	private const int controllerMapPPKeyVersion_includeDuplicateJoystickIndex = 1;

	private const int controllerMapPPKeyVersion_supportDisconnectedControllers = 2;

	private const int controllerMapPPKeyVersion_includeFormatVersion = 2;

	private const int controllerMapPPKeyVersion = 2;

	private const int controllerElementByRoleMapPPKeyVersion = 0;

	[Tooltip("Should this script be used? If disabled, nothing will be saved or loaded.")]
	[SerializeField]
	private bool isEnabled = true;

	[Tooltip("Should saved data be loaded on start?")]
	[SerializeField]
	private bool loadDataOnStart = true;

	[Tooltip("Should Player Joystick assignments be saved and loaded? This is not totally reliable for all Joysticks on all platforms. Some platforms/input sources do not provide enough information to reliably save assignments from session to session and reboot to reboot.")]
	[SerializeField]
	private bool loadJoystickAssignments = true;

	[Tooltip("Should Player Keyboard assignments be saved and loaded?")]
	[SerializeField]
	private bool loadKeyboardAssignments = true;

	[Tooltip("Should Player Mouse assignments be saved and loaded?")]
	[SerializeField]
	private bool loadMouseAssignments = true;

	[Tooltip("How should Action mapping data be saved?\n\nBy Controller: Data is stored per-controller. Action mappings apply only to the specific controller for which it was saved.\n\nBy Controller Element Role: Data is stored per-element on the controller if the controller element has a known role. Action mappings are mirrored on controller elements with the same role on all other controllers for the Player. Example: When saving Action mappings for a gamepad, element on all gamepads that have the same roles will inherit the mappings. This allows you to remap once for all compatible gamepads simultaneously, for example. This can extend beyond just gamepads, however. For example: On a console platform, a racing wheel with A, B, X, Y, D-Pad etc. elements will also reflect the same Action mappings if the gamepad is remapped. Action mappings for any controller elements that do not have known roles will be saved per-controller. Warning: Do not use this mode if you need to allow a Player to save different mappings for multiple controllers of the same type such as gamepads. (This option currently works best for gamepads and only miminally for other controller types.)")]
	[SerializeField]
	private ActionMappingSaveMode _actionMappingSaveMode;

	[Tooltip("The PlayerPrefs key prefix. Change this to change how keys are stored in PlayerPrefs. Changing this will make saved data already stored with the old key no longer accessible.")]
	[SerializeField]
	private string playerPrefsKeyPrefix = "RewiredSaveData";

	[NonSerialized]
	private bool allowImpreciseJoystickAssignmentMatching = true;

	[NonSerialized]
	private bool deferredJoystickAssignmentLoadPending;

	[NonSerialized]
	private bool wasJoystickEverDetected;

	[NonSerialized]
	private List<int> __allActionIds;

	[NonSerialized]
	private string __allActionIdsString;

	[NonSerialized]
	private readonly StringBuilder _sb = new StringBuilder();

	[NonSerialized]
	private Dictionary<string, ControllerElementByRoleMap> _tempElementByRoleMaps;

	[NonSerialized]
	private Dictionary<string, bool> _tempElementByRoleMapsEnabled;

	public bool IsEnabled
	{
		get
		{
			return isEnabled;
		}
		set
		{
			isEnabled = value;
		}
	}

	public bool LoadDataOnStart
	{
		get
		{
			return loadDataOnStart;
		}
		set
		{
			loadDataOnStart = value;
		}
	}

	public bool LoadJoystickAssignments
	{
		get
		{
			return loadJoystickAssignments;
		}
		set
		{
			loadJoystickAssignments = value;
		}
	}

	public bool LoadKeyboardAssignments
	{
		get
		{
			return loadKeyboardAssignments;
		}
		set
		{
			loadKeyboardAssignments = value;
		}
	}

	public bool LoadMouseAssignments
	{
		get
		{
			return loadMouseAssignments;
		}
		set
		{
			loadMouseAssignments = value;
		}
	}

	public ActionMappingSaveMode actionMappingSaveMode
	{
		get
		{
			return _actionMappingSaveMode;
		}
		set
		{
			_actionMappingSaveMode = value;
		}
	}

	public string PlayerPrefsKeyPrefix
	{
		get
		{
			return playerPrefsKeyPrefix;
		}
		set
		{
			playerPrefsKeyPrefix = value;
		}
	}

	private string playerPrefsKey_controllerAssignments => string.Format("{0}_{1}", playerPrefsKeyPrefix, "ControllerAssignments");

	private bool loadControllerAssignments
	{
		get
		{
			if (!loadKeyboardAssignments && !loadMouseAssignments)
			{
				return loadJoystickAssignments;
			}
			return true;
		}
	}

	private List<int> allActionIds
	{
		get
		{
			if (__allActionIds != null)
			{
				return __allActionIds;
			}
			List<int> list = new List<int>();
			IList<InputAction> actions = ReInput.mapping.Actions;
			for (int i = 0; i < actions.Count; i++)
			{
				list.Add(actions[i].id);
			}
			__allActionIds = list;
			return list;
		}
	}

	private string allActionIdsString
	{
		get
		{
			if (!string.IsNullOrEmpty(__allActionIdsString))
			{
				return __allActionIdsString;
			}
			StringBuilder stringBuilder = new StringBuilder();
			List<int> list = allActionIds;
			for (int i = 0; i < list.Count; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append(",");
				}
				stringBuilder.Append(list[i]);
			}
			__allActionIdsString = stringBuilder.ToString();
			return __allActionIdsString;
		}
	}

	public override void Save()
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SaveAll();
		}
	}

	public override void SaveControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SaveControllerDataNow(playerId, controllerType, controllerId);
		}
	}

	public override void SaveControllerData(ControllerType controllerType, int controllerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SaveControllerDataNow(controllerType, controllerId);
		}
	}

	public override void SavePlayerData(int playerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SavePlayerDataNow(playerId);
		}
	}

	public override void SaveInputBehavior(int playerId, int behaviorId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not save any data.", this);
		}
		else
		{
			SaveInputBehaviorNow(playerId, behaviorId);
		}
	}

	public override void Load()
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadAll();
		}
	}

	public override void LoadControllerData(int playerId, ControllerType controllerType, int controllerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadControllerDataNow(playerId, controllerType, controllerId);
		}
	}

	public override void LoadControllerData(ControllerType controllerType, int controllerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadControllerDataNow(controllerType, controllerId);
		}
	}

	public override void LoadPlayerData(int playerId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadPlayerDataNow(playerId);
		}
	}

	public override void LoadInputBehavior(int playerId, int behaviorId)
	{
		if (!isEnabled)
		{
			Debug.LogWarning("Rewired: UserDataStore_PlayerPrefs is disabled and will not load any data.", this);
		}
		else
		{
			LoadInputBehaviorNow(playerId, behaviorId);
		}
	}

	protected override void OnInitialize()
	{
		if (loadDataOnStart)
		{
			Load();
			if (loadControllerAssignments && ReInput.controllers.joystickCount > 0)
			{
				wasJoystickEverDetected = true;
				SaveControllerAssignments();
			}
		}
	}

	protected override void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		if (isEnabled && args.controllerType == ControllerType.Joystick)
		{
			LoadJoystickData(args.controllerId);
			if (loadDataOnStart && loadJoystickAssignments && !wasJoystickEverDetected)
			{
				StartCoroutine(LoadJoystickAssignmentsDeferred());
			}
			if (loadJoystickAssignments && !deferredJoystickAssignmentLoadPending)
			{
				SaveControllerAssignments();
			}
			wasJoystickEverDetected = true;
		}
	}

	protected override void OnControllerPreDisconnect(ControllerStatusChangedEventArgs args)
	{
		if (isEnabled && args.controllerType == ControllerType.Joystick)
		{
			SaveJoystickData(args.controllerId);
		}
	}

	protected override void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (isEnabled && loadControllerAssignments)
		{
			SaveControllerAssignments();
		}
	}

	public override void SaveControllerMap(int playerId, ControllerMap controllerMap)
	{
		if (controllerMap != null)
		{
			Player player = ReInput.players.GetPlayer(playerId);
			if (player != null)
			{
				SaveControllerMap(player, controllerMap);
			}
		}
	}

	public override ControllerMap LoadControllerMap(int playerId, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return null;
		}
		return LoadControllerMap(player, controllerIdentifier, categoryId, layoutId);
	}

	private int LoadAll()
	{
		int num = 0;
		if (loadControllerAssignments && LoadControllerAssignmentsNow())
		{
			num++;
		}
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			num += LoadPlayerDataNow(allPlayers[i]);
		}
		return num + LoadAllJoystickCalibrationData();
	}

	private int LoadPlayerDataNow(int playerId)
	{
		return LoadPlayerDataNow(ReInput.players.GetPlayer(playerId));
	}

	private int LoadPlayerDataNow(Player player)
	{
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		num += LoadInputBehaviors(player.id);
		num += LoadControllerMaps(player.id, ControllerType.Keyboard, 0);
		num += LoadControllerMaps(player.id, ControllerType.Mouse, 0);
		foreach (Joystick joystick in player.controllers.Joysticks)
		{
			num += LoadControllerMaps(player.id, ControllerType.Joystick, joystick.id);
		}
		RefreshLayoutManager(player.id);
		return num;
	}

	private int LoadAllJoystickCalibrationData()
	{
		int num = 0;
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			num += LoadJoystickCalibrationData(joysticks[i]);
		}
		return num;
	}

	private int LoadJoystickCalibrationData(Joystick joystick)
	{
		if (joystick == null)
		{
			return 0;
		}
		if (!joystick.ImportCalibrationMapFromXmlString(GetJoystickCalibrationMapXml(joystick)))
		{
			return 0;
		}
		return 1;
	}

	private int LoadJoystickCalibrationData(int joystickId)
	{
		return LoadJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private int LoadJoystickData(int joystickId)
	{
		int num = 0;
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				num += LoadControllerMaps(player.id, ControllerType.Joystick, joystickId);
				RefreshLayoutManager(player.id);
			}
		}
		return num + LoadJoystickCalibrationData(joystickId);
	}

	private int LoadControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		int num = 0 + LoadControllerMaps(playerId, controllerType, controllerId);
		RefreshLayoutManager(playerId);
		return num + LoadControllerDataNow(controllerType, controllerId);
	}

	private int LoadControllerDataNow(ControllerType controllerType, int controllerId)
	{
		int num = 0;
		if (controllerType == ControllerType.Joystick)
		{
			num += LoadJoystickCalibrationData(controllerId);
		}
		return num;
	}

	private int LoadControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		int num = 0;
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return num;
		}
		Controller controller = ReInput.controllers.GetController(controllerType, controllerId);
		if (controller == null)
		{
			return num;
		}
		IList<InputMapCategory> mapCategories = ReInput.mapping.MapCategories;
		for (int i = 0; i < mapCategories.Count; i++)
		{
			InputMapCategory inputMapCategory = mapCategories[i];
			if (!inputMapCategory.userAssignable)
			{
				continue;
			}
			IList<InputLayout> list = ReInput.mapping.MapLayouts(controller.type);
			for (int j = 0; j < list.Count; j++)
			{
				InputLayout inputLayout = list[j];
				switch (_actionMappingSaveMode)
				{
				case ActionMappingSaveMode.ByController:
				{
					ControllerMap controllerMap2 = LoadControllerMap(player, controller.identifier, inputMapCategory.id, inputLayout.id);
					if (controllerMap2 != null)
					{
						player.controllers.maps.AddMap(controller, controllerMap2);
						num++;
					}
					break;
				}
				case ActionMappingSaveMode.ByControllerElementRole:
				{
					Dictionary<string, ControllerElementByRoleMap> dictionary = ((_tempElementByRoleMaps != null) ? _tempElementByRoleMaps : (_tempElementByRoleMaps = new Dictionary<string, ControllerElementByRoleMap>()));
					dictionary.Clear();
					bool flag = false;
					bool flag2 = false;
					for (int k = 0; k < controller.elementCount; k++)
					{
						string role = controller.Elements[k].elementIdentifier.role;
						if (!string.IsNullOrEmpty(role))
						{
							LoadControllerElementMapByRole(player, controller, role, inputMapCategory.id, inputLayout.id, dictionary);
						}
					}
					ControllerMap controllerMap = LoadControllerMap(player, controller.identifier, inputMapCategory.id, inputLayout.id);
					if (controllerMap == null)
					{
						controllerMap = player.controllers.maps.GetMap(controller.type, controller.id, inputMapCategory.id, inputLayout.id);
						if (controllerMap == null)
						{
							if (dictionary.Count == 0)
							{
								break;
							}
							controllerMap = ControllerMap.Create(controller, inputMapCategory.id, inputLayout.id);
						}
					}
					else
					{
						flag = true;
					}
					if (dictionary.Count != 0)
					{
						if (_tempElementByRoleMapsEnabled == null)
						{
							_tempElementByRoleMapsEnabled = new Dictionary<string, bool>();
						}
						_tempElementByRoleMapsEnabled.Clear();
						for (int num2 = controllerMap.elementMapCount - 1; num2 >= 0; num2--)
						{
							ActionElementMap actionElementMap = controllerMap.ElementMaps[num2];
							ControllerElementIdentifier elementIdentifierById = controller.GetElementIdentifierById(actionElementMap.elementIdentifierId);
							if (elementIdentifierById != null && dictionary.ContainsKey(elementIdentifierById.role))
							{
								_tempElementByRoleMapsEnabled[elementIdentifierById.role] = actionElementMap.enabled;
								controllerMap.DeleteElementMap(actionElementMap.id);
								flag2 = true;
							}
						}
						foreach (KeyValuePair<string, ControllerElementByRoleMap> item in dictionary)
						{
							ControllerElementByRoleMap value = item.Value;
							for (int l = 0; l < controller.Elements.Count; l++)
							{
								Controller.Element element = controller.Elements[l];
								if (element.elementIdentifier.role != item.Value.role || value.data == null || value.data.Count == 0)
								{
									continue;
								}
								for (int m = 0; m < value.data.Count; m++)
								{
									if (value.data[m].TryGetElementAssignment(controllerType, element, out var assignment) && controllerMap.CreateElementMap(assignment, out var result))
									{
										if (_tempElementByRoleMapsEnabled.TryGetValue(item.Value.role, out var value2))
										{
											result.enabled = value2;
										}
										flag = true;
										flag2 = true;
									}
								}
							}
						}
					}
					if (flag2)
					{
						controllerMap.isModified = false;
					}
					if (flag)
					{
						player.controllers.maps.AddMap(controller, controllerMap);
						num++;
					}
					break;
				}
				default:
					throw new NotImplementedException();
				}
			}
		}
		return num;
	}

	private ControllerMap LoadControllerMap(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		if (player == null)
		{
			return null;
		}
		string controllerMapXml = GetControllerMapXml(player, controllerIdentifier, categoryId, layoutId);
		if (string.IsNullOrEmpty(controllerMapXml))
		{
			return null;
		}
		ControllerMap controllerMap = ControllerMap.CreateFromXml(controllerIdentifier.controllerType, controllerMapXml);
		if (controllerMap == null)
		{
			return null;
		}
		List<int> controllerMapKnownActionIds = GetControllerMapKnownActionIds(player, controllerIdentifier, categoryId, layoutId);
		AddDefaultMappingsForNewActions(controllerIdentifier, controllerMap, controllerMapKnownActionIds);
		return controllerMap;
	}

	private bool LoadControllerElementMapByRole(Player player, Controller controller, string role, int mapCategoryId, int layoutId, Dictionary<string, ControllerElementByRoleMap> elementByRoleMaps)
	{
		if (string.IsNullOrEmpty(role))
		{
			return false;
		}
		string controllerElementByRoleMapPlayerPrefsKey = GetControllerElementByRoleMapPlayerPrefsKey(player, role, mapCategoryId, layoutId, 0);
		try
		{
			string @string;
			if (!PlayerPrefs.HasKey(controllerElementByRoleMapPlayerPrefsKey) || string.IsNullOrEmpty(@string = PlayerPrefs.GetString(controllerElementByRoleMapPlayerPrefsKey)))
			{
				return false;
			}
			if (string.IsNullOrEmpty(@string))
			{
				return false;
			}
			ControllerElementByRoleMap controllerElementByRoleMap = ControllerElementByRoleMap.FromJson(role, @string);
			if (controllerElementByRoleMap == null)
			{
				return false;
			}
			elementByRoleMaps[role] = controllerElementByRoleMap;
			return true;
		}
		catch
		{
			return false;
		}
	}

	private int LoadInputBehaviors(int playerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return 0;
		}
		int num = 0;
		IList<InputBehavior> inputBehaviors = ReInput.mapping.GetInputBehaviors(player.id);
		for (int i = 0; i < inputBehaviors.Count; i++)
		{
			num += LoadInputBehaviorNow(player, inputBehaviors[i]);
		}
		return num;
	}

	private int LoadInputBehaviorNow(int playerId, int behaviorId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null)
		{
			return 0;
		}
		InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
		if (inputBehavior == null)
		{
			return 0;
		}
		return LoadInputBehaviorNow(player, inputBehavior);
	}

	private int LoadInputBehaviorNow(Player player, InputBehavior inputBehavior)
	{
		if (player == null || inputBehavior == null)
		{
			return 0;
		}
		string inputBehaviorXml = GetInputBehaviorXml(player, inputBehavior.id);
		if (inputBehaviorXml == null || inputBehaviorXml == string.Empty)
		{
			return 0;
		}
		if (!inputBehavior.ImportXmlString(inputBehaviorXml))
		{
			return 0;
		}
		return 1;
	}

	private bool LoadControllerAssignmentsNow()
	{
		try
		{
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = LoadControllerAssignmentData();
			if (controllerAssignmentSaveInfo == null)
			{
				return false;
			}
			if (loadKeyboardAssignments || loadMouseAssignments)
			{
				LoadKeyboardAndMouseAssignmentsNow(controllerAssignmentSaveInfo);
			}
			if (loadJoystickAssignments)
			{
				LoadJoystickAssignmentsNow(controllerAssignmentSaveInfo);
			}
		}
		catch
		{
		}
		return true;
	}

	private bool LoadKeyboardAndMouseAssignmentsNow(ControllerAssignmentSaveInfo data)
	{
		try
		{
			if (data == null && (data = LoadControllerAssignmentData()) == null)
			{
				return false;
			}
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				if (data.ContainsPlayer(allPlayer.id))
				{
					ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.players[data.IndexOfPlayer(allPlayer.id)];
					if (loadKeyboardAssignments)
					{
						allPlayer.controllers.hasKeyboard = playerInfo.hasKeyboard;
					}
					if (loadMouseAssignments)
					{
						allPlayer.controllers.hasMouse = playerInfo.hasMouse;
					}
				}
			}
		}
		catch
		{
		}
		return true;
	}

	private bool LoadJoystickAssignmentsNow(ControllerAssignmentSaveInfo data)
	{
		try
		{
			if (ReInput.controllers.joystickCount == 0)
			{
				return false;
			}
			if (data == null && (data = LoadControllerAssignmentData()) == null)
			{
				return false;
			}
			foreach (Player allPlayer in ReInput.players.AllPlayers)
			{
				allPlayer.controllers.ClearControllersOfType(ControllerType.Joystick);
			}
			List<JoystickAssignmentHistoryInfo> list = (loadJoystickAssignments ? new List<JoystickAssignmentHistoryInfo>() : null);
			foreach (Player allPlayer2 in ReInput.players.AllPlayers)
			{
				if (!data.ContainsPlayer(allPlayer2.id))
				{
					continue;
				}
				ControllerAssignmentSaveInfo.PlayerInfo playerInfo = data.players[data.IndexOfPlayer(allPlayer2.id)];
				for (int i = 0; i < playerInfo.joystickCount; i++)
				{
					ControllerAssignmentSaveInfo.JoystickInfo joystickInfo2 = playerInfo.joysticks[i];
					if (joystickInfo2 == null)
					{
						continue;
					}
					Joystick joystick = FindJoystickPrecise(joystickInfo2);
					if (joystick != null)
					{
						if (list.Find((JoystickAssignmentHistoryInfo x) => x.joystick == joystick) == null)
						{
							list.Add(new JoystickAssignmentHistoryInfo(joystick, joystickInfo2.id));
						}
						allPlayer2.controllers.AddController(joystick, removeFromOtherPlayers: false);
					}
				}
			}
			if (allowImpreciseJoystickAssignmentMatching)
			{
				foreach (Player allPlayer3 in ReInput.players.AllPlayers)
				{
					if (!data.ContainsPlayer(allPlayer3.id))
					{
						continue;
					}
					ControllerAssignmentSaveInfo.PlayerInfo playerInfo2 = data.players[data.IndexOfPlayer(allPlayer3.id)];
					for (int j = 0; j < playerInfo2.joystickCount; j++)
					{
						ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = playerInfo2.joysticks[j];
						if (joystickInfo == null)
						{
							continue;
						}
						Joystick joystick2 = null;
						int num = list.FindIndex((JoystickAssignmentHistoryInfo x) => x.oldJoystickId == joystickInfo.id);
						if (num >= 0)
						{
							joystick2 = list[num].joystick;
						}
						else
						{
							if (!TryFindJoysticksImprecise(joystickInfo, out var matches))
							{
								continue;
							}
							foreach (Joystick match in matches)
							{
								if (list.Find((JoystickAssignmentHistoryInfo x) => x.joystick == match) == null)
								{
									joystick2 = match;
									break;
								}
							}
							if (joystick2 == null)
							{
								continue;
							}
							list.Add(new JoystickAssignmentHistoryInfo(joystick2, joystickInfo.id));
						}
						allPlayer3.controllers.AddController(joystick2, removeFromOtherPlayers: false);
					}
				}
			}
		}
		catch
		{
		}
		if (ReInput.configuration.autoAssignJoysticks)
		{
			ReInput.controllers.AutoAssignJoysticks();
		}
		return true;
	}

	private ControllerAssignmentSaveInfo LoadControllerAssignmentData()
	{
		try
		{
			if (!PlayerPrefs.HasKey(playerPrefsKey_controllerAssignments))
			{
				return null;
			}
			string @string = PlayerPrefs.GetString(playerPrefsKey_controllerAssignments);
			if (string.IsNullOrEmpty(@string))
			{
				return null;
			}
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = JsonParser.FromJson<ControllerAssignmentSaveInfo>(@string);
			if (controllerAssignmentSaveInfo == null || controllerAssignmentSaveInfo.playerCount == 0)
			{
				return null;
			}
			return controllerAssignmentSaveInfo;
		}
		catch
		{
			return null;
		}
	}

	private IEnumerator LoadJoystickAssignmentsDeferred()
	{
		deferredJoystickAssignmentLoadPending = true;
		yield return new WaitForEndOfFrame();
		if (ReInput.isReady)
		{
			LoadJoystickAssignmentsNow(null);
			SaveControllerAssignments();
			deferredJoystickAssignmentLoadPending = false;
		}
	}

	private void SaveAll()
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			SavePlayerDataNow(allPlayers[i]);
		}
		SaveAllJoystickCalibrationData();
		if (loadControllerAssignments)
		{
			SaveControllerAssignments();
		}
		PlayerPrefs.Save();
		for (int j = 0; j < allPlayers.Count; j++)
		{
			OnControllerMapsSaved(allPlayers[j]);
		}
	}

	private void SavePlayerDataNow(int playerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		SavePlayerDataNow(player);
		PlayerPrefs.Save();
		OnControllerMapsSaved(player);
	}

	private void SavePlayerDataNow(Player player)
	{
		if (player != null)
		{
			PlayerSaveData saveData = player.GetSaveData(userAssignableMapsOnly: true);
			SaveInputBehaviors(player, saveData);
			SaveControllerMaps(player, saveData);
		}
	}

	private void SaveAllJoystickCalibrationData()
	{
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			SaveJoystickCalibrationData(joysticks[i]);
		}
	}

	private void SaveJoystickCalibrationData(int joystickId)
	{
		SaveJoystickCalibrationData(ReInput.controllers.GetJoystick(joystickId));
	}

	private void SaveJoystickCalibrationData(Joystick joystick)
	{
		if (joystick != null)
		{
			JoystickCalibrationMapSaveData calibrationMapSaveData = joystick.GetCalibrationMapSaveData();
			PlayerPrefs.SetString(GetJoystickCalibrationMapPlayerPrefsKey(joystick), calibrationMapSaveData.map.ToXmlString());
		}
	}

	private void SaveJoystickData(int joystickId)
	{
		IList<Player> allPlayers = ReInput.players.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			Player player = allPlayers[i];
			if (player.controllers.ContainsController(ControllerType.Joystick, joystickId))
			{
				SaveControllerMaps(player.id, ControllerType.Joystick, joystickId);
			}
		}
		SaveJoystickCalibrationData(joystickId);
	}

	private void SaveControllerDataNow(int playerId, ControllerType controllerType, int controllerId)
	{
		SaveControllerMaps(playerId, controllerType, controllerId);
		SaveControllerDataNow(controllerType, controllerId);
		PlayerPrefs.Save();
	}

	private void SaveControllerDataNow(ControllerType controllerType, int controllerId)
	{
		if (controllerType == ControllerType.Joystick)
		{
			SaveJoystickCalibrationData(controllerId);
		}
		PlayerPrefs.Save();
	}

	private void SaveControllerMaps(Player player, PlayerSaveData playerSaveData)
	{
		List<ControllerMapSaveData> list = new List<ControllerMapSaveData>(playerSaveData.AllControllerMapSaveData);
		if (_actionMappingSaveMode == ActionMappingSaveMode.ByControllerElementRole)
		{
			list.Sort(SortOldestToNewest);
		}
		for (int i = 0; i < list.Count; i++)
		{
			SaveControllerMap(player, list[i].map);
		}
	}

	private void SaveControllerMaps(int playerId, ControllerType controllerType, int controllerId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player == null || !player.controllers.ContainsController(controllerType, controllerId))
		{
			return;
		}
		ControllerMapSaveData[] mapSaveData = player.controllers.maps.GetMapSaveData(controllerType, controllerId, userAssignableMapsOnly: true);
		if (mapSaveData != null)
		{
			if (_actionMappingSaveMode == ActionMappingSaveMode.ByControllerElementRole)
			{
				List<ControllerMapSaveData> list = new List<ControllerMapSaveData>(mapSaveData);
				list.Sort(SortOldestToNewest);
				list.CopyTo(mapSaveData);
			}
			for (int i = 0; i < mapSaveData.Length; i++)
			{
				SaveControllerMap(player, mapSaveData[i].map);
			}
		}
	}

	private void SaveControllerMap(Player player, ControllerMap controllerMap)
	{
		switch (_actionMappingSaveMode)
		{
		case ActionMappingSaveMode.ByController:
			SaveControllerMapByController(player, controllerMap);
			break;
		case ActionMappingSaveMode.ByControllerElementRole:
			SaveControllerMapByControllerElementRole(player, controllerMap.controller, controllerMap);
			break;
		default:
			throw new NotImplementedException();
		}
	}

	private void SaveControllerMapByController(Player player, ControllerMap controllerMap)
	{
		PlayerPrefs.SetString(GetControllerMapPlayerPrefsKey(player, controllerMap.controller.identifier, controllerMap.categoryId, controllerMap.layoutId, 2), controllerMap.ToXmlString());
		PlayerPrefs.SetString(GetControllerMapKnownActionIdsPlayerPrefsKey(player, controllerMap.controller.identifier, controllerMap.categoryId, controllerMap.layoutId, 2), allActionIdsString);
	}

	private void SaveControllerMapByControllerElementRole(Player player, Controller controller, ControllerMap controllerMap)
	{
		if (controller == null)
		{
			return;
		}
		SaveControllerMapByController(player, controllerMap);
		IList<ActionElementMap> elementMaps = controllerMap.ElementMaps;
		Dictionary<string, ControllerElementByRoleMap> maps = null;
		for (int i = 0; i < controller.elementCount; i++)
		{
			string role = controller.Elements[i].elementIdentifier.role;
			if (string.IsNullOrEmpty(role))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < elementMaps.Count; j++)
			{
				Controller.Element elementById = controller.GetElementById(elementMaps[j].elementIdentifierId);
				if (elementById != null && !(elementById.elementIdentifier.role != role))
				{
					flag |= AddControllerElementByRoleMapEntry(player, controllerMap.controller, elementMaps[j], ref maps);
				}
			}
			if (!flag)
			{
				if (maps == null)
				{
					maps = new Dictionary<string, ControllerElementByRoleMap>();
				}
				maps.Add(role, new ControllerElementByRoleMap
				{
					role = role
				});
			}
		}
		if (maps == null)
		{
			return;
		}
		foreach (KeyValuePair<string, ControllerElementByRoleMap> item in maps)
		{
			PlayerPrefs.SetString(GetControllerElementByRoleMapPlayerPrefsKey(player, item.Value.role, controllerMap.categoryId, controllerMap.layoutId, 0), item.Value.ToJson());
		}
	}

	private bool AddControllerElementByRoleMapEntry(Player player, Controller controller, ActionElementMap elementMap, ref Dictionary<string, ControllerElementByRoleMap> maps)
	{
		ControllerElementIdentifier elementIdentifierById = controller.GetElementIdentifierById(elementMap.elementIdentifierId);
		if (elementIdentifierById == null || string.IsNullOrEmpty(elementIdentifierById.role))
		{
			return false;
		}
		if (maps == null)
		{
			maps = new Dictionary<string, ControllerElementByRoleMap>();
		}
		if (!maps.TryGetValue(elementIdentifierById.role, out var value))
		{
			value = new ControllerElementByRoleMap();
			value.role = elementIdentifierById.role;
			maps.Add(elementIdentifierById.role, value);
		}
		value.Add(elementMap);
		return true;
	}

	private void SaveInputBehaviors(Player player, PlayerSaveData playerSaveData)
	{
		if (player != null)
		{
			InputBehavior[] inputBehaviors = playerSaveData.inputBehaviors;
			for (int i = 0; i < inputBehaviors.Length; i++)
			{
				SaveInputBehaviorNow(player, inputBehaviors[i]);
			}
		}
	}

	private void SaveInputBehaviorNow(int playerId, int behaviorId)
	{
		Player player = ReInput.players.GetPlayer(playerId);
		if (player != null)
		{
			InputBehavior inputBehavior = ReInput.mapping.GetInputBehavior(playerId, behaviorId);
			if (inputBehavior != null)
			{
				SaveInputBehaviorNow(player, inputBehavior);
				PlayerPrefs.Save();
			}
		}
	}

	private void SaveInputBehaviorNow(Player player, InputBehavior inputBehavior)
	{
		if (player != null && inputBehavior != null)
		{
			PlayerPrefs.SetString(GetInputBehaviorPlayerPrefsKey(player, inputBehavior.id), inputBehavior.ToXmlString());
		}
	}

	private bool SaveControllerAssignments()
	{
		try
		{
			ControllerAssignmentSaveInfo controllerAssignmentSaveInfo = new ControllerAssignmentSaveInfo(ReInput.players.allPlayerCount);
			for (int i = 0; i < ReInput.players.allPlayerCount; i++)
			{
				Player player = ReInput.players.AllPlayers[i];
				ControllerAssignmentSaveInfo.PlayerInfo playerInfo = new ControllerAssignmentSaveInfo.PlayerInfo();
				controllerAssignmentSaveInfo.players[i] = playerInfo;
				playerInfo.id = player.id;
				playerInfo.hasKeyboard = player.controllers.hasKeyboard;
				playerInfo.hasMouse = player.controllers.hasMouse;
				ControllerAssignmentSaveInfo.JoystickInfo[] array = (playerInfo.joysticks = new ControllerAssignmentSaveInfo.JoystickInfo[player.controllers.joystickCount]);
				for (int j = 0; j < player.controllers.joystickCount; j++)
				{
					Joystick joystick = player.controllers.Joysticks[j];
					ControllerAssignmentSaveInfo.JoystickInfo joystickInfo = new ControllerAssignmentSaveInfo.JoystickInfo();
					joystickInfo.instanceGuid = joystick.deviceInstanceGuid;
					joystickInfo.id = joystick.id;
					joystickInfo.hardwareIdentifier = joystick.hardwareIdentifier;
					array[j] = joystickInfo;
				}
			}
			PlayerPrefs.SetString(playerPrefsKey_controllerAssignments, JsonWriter.ToJson(controllerAssignmentSaveInfo));
			PlayerPrefs.Save();
		}
		catch
		{
		}
		return true;
	}

	private bool ControllerAssignmentSaveDataExists()
	{
		if (!PlayerPrefs.HasKey(playerPrefsKey_controllerAssignments))
		{
			return false;
		}
		if (string.IsNullOrEmpty(PlayerPrefs.GetString(playerPrefsKey_controllerAssignments)))
		{
			return false;
		}
		return true;
	}

	private string GetControllerMapPlayerPrefsKey(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		_sb.Length = 0;
		AppendBaseKey(_sb, playerPrefsKeyPrefix);
		AppendPlayerKey(_sb, player);
		AppendControllerMapKey(_sb, player, controllerIdentifier, categoryId, layoutId, ppKeyVersion);
		return _sb.ToString();
	}

	private string GetControllerElementByRoleMapPlayerPrefsKey(Player player, string elementRole, int categoryId, int layoutId, int ppKeyVersion)
	{
		_sb.Length = 0;
		AppendBaseKey(_sb, playerPrefsKeyPrefix);
		AppendPlayerKey(_sb, player);
		AppendControllerElementByRoleMapKey(_sb, elementRole, categoryId, layoutId, ppKeyVersion);
		return _sb.ToString();
	}

	private string GetJoystickCalibrationMapPlayerPrefsKey(Joystick joystick)
	{
		_sb.Length = 0;
		AppendBaseKey(_sb, playerPrefsKeyPrefix);
		AppendJoystickCalibrationMapKey(_sb, joystick);
		return _sb.ToString();
	}

	private string GetControllerMapKnownActionIdsPlayerPrefsKey(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		_sb.Length = 0;
		AppendBaseKey(_sb, playerPrefsKeyPrefix);
		AppendPlayerKey(_sb, player);
		AppendControllerMapKnownActionIdsKey(_sb, player, controllerIdentifier, categoryId, layoutId, ppKeyVersion);
		return _sb.ToString();
	}

	private string GetInputBehaviorPlayerPrefsKey(Player player, int inputBehaviorId)
	{
		_sb.Length = 0;
		AppendBaseKey(_sb, playerPrefsKeyPrefix);
		AppendPlayerKey(_sb, player);
		AppendInputBehaviorKey(_sb, inputBehaviorId);
		return _sb.ToString();
	}

	private static void AppendBaseKey(StringBuilder sb, string playerPrefsKeyPrefix)
	{
		sb.Append(playerPrefsKeyPrefix);
	}

	private static void AppendPlayerKey(StringBuilder sb, Player player)
	{
		sb.Append("|playerName=");
		sb.Append(player.name);
	}

	private static void AppendControllerMapKey(StringBuilder sb, Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		sb.Append("|dataType=ControllerMap");
		AppendControllerMapKeyCommonSuffix(sb, player, controllerIdentifier, categoryId, layoutId, ppKeyVersion);
	}

	private static void AppendControllerMapKnownActionIdsKey(StringBuilder sb, Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		sb.Append("|dataType=ControllerMap_KnownActionIds");
		AppendControllerMapKeyCommonSuffix(sb, player, controllerIdentifier, categoryId, layoutId, ppKeyVersion);
	}

	private static void AppendControllerMapKeyCommonSuffix(StringBuilder sb, Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId, int ppKeyVersion)
	{
		if (ppKeyVersion >= 2)
		{
			sb.Append("|kv=");
			sb.Append(ppKeyVersion);
		}
		sb.Append("|controllerMapType=");
		sb.Append(GetControllerMapType(controllerIdentifier.controllerType).Name);
		sb.Append("|categoryId=");
		sb.Append(categoryId);
		sb.Append("|layoutId=");
		sb.Append(layoutId);
		if (ppKeyVersion >= 2)
		{
			sb.Append("|hardwareGuid=");
			sb.Append(controllerIdentifier.hardwareTypeGuid);
			if (controllerIdentifier.hardwareTypeGuid == Guid.Empty)
			{
				sb.Append("|hardwareIdentifier=");
				sb.Append(controllerIdentifier.hardwareIdentifier);
			}
			if (controllerIdentifier.controllerType == ControllerType.Joystick)
			{
				sb.Append("|duplicate=");
				sb.Append(GetDuplicateIndex(player, controllerIdentifier));
			}
			return;
		}
		sb.Append("|hardwareIdentifier=");
		sb.Append(controllerIdentifier.hardwareIdentifier);
		if (controllerIdentifier.controllerType == ControllerType.Joystick)
		{
			sb.Append("|hardwareGuid=");
			sb.Append(controllerIdentifier.hardwareTypeGuid);
			if (ppKeyVersion >= 1)
			{
				sb.Append("|duplicate=");
				sb.Append(GetDuplicateIndex(player, controllerIdentifier));
			}
		}
	}

	private static void AppendControllerElementByRoleMapKey(StringBuilder sb, string elementRole, int categoryId, int layoutId, int ppKeyVersion)
	{
		sb.Append("|dataType=ElementRoleMap");
		sb.Append("|kv=");
		sb.Append(ppKeyVersion);
		sb.Append("|categoryId=");
		sb.Append(categoryId);
		sb.Append("|layoutId=");
		sb.Append(layoutId);
		sb.Append("|role=");
		sb.Append(elementRole);
	}

	private static void AppendJoystickCalibrationMapKey(StringBuilder sb, Joystick joystick)
	{
		sb.Append("|dataType=CalibrationMap");
		sb.Append("|controllerType=");
		sb.Append(joystick.type.ToString());
		sb.Append("|hardwareIdentifier=");
		sb.Append(joystick.hardwareIdentifier);
		sb.Append("|hardwareGuid=");
		sb.Append(joystick.hardwareTypeGuid.ToString());
	}

	private static void AppendInputBehaviorKey(StringBuilder sb, int inputBehaviorId)
	{
		sb.Append("|dataType=InputBehavior");
		sb.Append("|id=");
		sb.Append(inputBehaviorId);
	}

	private string GetControllerMapXml(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		for (int num = 2; num >= 0; num--)
		{
			string controllerMapPlayerPrefsKey = GetControllerMapPlayerPrefsKey(player, controllerIdentifier, categoryId, layoutId, num);
			if (PlayerPrefs.HasKey(controllerMapPlayerPrefsKey))
			{
				return PlayerPrefs.GetString(controllerMapPlayerPrefsKey);
			}
		}
		return null;
	}

	private List<int> GetControllerMapKnownActionIds(Player player, ControllerIdentifier controllerIdentifier, int categoryId, int layoutId)
	{
		List<int> list = new List<int>();
		string key = null;
		bool flag = false;
		for (int num = 2; num >= 0; num--)
		{
			key = GetControllerMapKnownActionIdsPlayerPrefsKey(player, controllerIdentifier, categoryId, layoutId, num);
			if (PlayerPrefs.HasKey(key))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return list;
		}
		string @string = PlayerPrefs.GetString(key);
		if (string.IsNullOrEmpty(@string))
		{
			return list;
		}
		string[] array = @string.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (!string.IsNullOrEmpty(array[i]) && int.TryParse(array[i], out var result))
			{
				list.Add(result);
			}
		}
		return list;
	}

	private string GetJoystickCalibrationMapXml(Joystick joystick)
	{
		string joystickCalibrationMapPlayerPrefsKey = GetJoystickCalibrationMapPlayerPrefsKey(joystick);
		if (!PlayerPrefs.HasKey(joystickCalibrationMapPlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(joystickCalibrationMapPlayerPrefsKey);
	}

	private string GetInputBehaviorXml(Player player, int id)
	{
		string inputBehaviorPlayerPrefsKey = GetInputBehaviorPlayerPrefsKey(player, id);
		if (!PlayerPrefs.HasKey(inputBehaviorPlayerPrefsKey))
		{
			return string.Empty;
		}
		return PlayerPrefs.GetString(inputBehaviorPlayerPrefsKey);
	}

	private void AddDefaultMappingsForNewActions(ControllerIdentifier controllerIdentifier, ControllerMap controllerMap, List<int> knownActionIds)
	{
		if (controllerMap == null || knownActionIds == null || knownActionIds == null || knownActionIds.Count == 0)
		{
			return;
		}
		ControllerMap controllerMapInstance = ReInput.mapping.GetControllerMapInstance(controllerIdentifier, controllerMap.categoryId, controllerMap.layoutId);
		if (controllerMapInstance == null)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (int allActionId in allActionIds)
		{
			if (!knownActionIds.Contains(allActionId))
			{
				list.Add(allActionId);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		bool flag = false;
		foreach (ActionElementMap allMap in controllerMapInstance.AllMaps)
		{
			if (list.Contains(allMap.actionId) && !controllerMap.DoesElementAssignmentConflict(allMap))
			{
				ElementAssignment elementAssignment = new ElementAssignment(controllerMap.controllerType, allMap.elementType, allMap.elementIdentifierId, allMap.axisRange, allMap.keyCode, allMap.modifierKeyFlags, allMap.actionId, allMap.axisContribution, allMap.invert);
				controllerMap.CreateElementMap(elementAssignment);
				flag = true;
			}
		}
		if (flag)
		{
			controllerMap.isModified = false;
		}
	}

	private Joystick FindJoystickPrecise(ControllerAssignmentSaveInfo.JoystickInfo joystickInfo)
	{
		if (joystickInfo == null)
		{
			return null;
		}
		if (joystickInfo.instanceGuid == Guid.Empty)
		{
			return null;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (joysticks[i].deviceInstanceGuid == joystickInfo.instanceGuid)
			{
				return joysticks[i];
			}
		}
		return null;
	}

	private bool TryFindJoysticksImprecise(ControllerAssignmentSaveInfo.JoystickInfo joystickInfo, out List<Joystick> matches)
	{
		matches = null;
		if (joystickInfo == null)
		{
			return false;
		}
		if (string.IsNullOrEmpty(joystickInfo.hardwareIdentifier))
		{
			return false;
		}
		IList<Joystick> joysticks = ReInput.controllers.Joysticks;
		for (int i = 0; i < joysticks.Count; i++)
		{
			if (string.Equals(joysticks[i].hardwareIdentifier, joystickInfo.hardwareIdentifier, StringComparison.OrdinalIgnoreCase))
			{
				if (matches == null)
				{
					matches = new List<Joystick>();
				}
				matches.Add(joysticks[i]);
			}
		}
		return matches != null;
	}

	private static int GetDuplicateIndex(Player player, ControllerIdentifier controllerIdentifier)
	{
		Controller controller = ReInput.controllers.GetController(controllerIdentifier);
		if (controller == null)
		{
			return 0;
		}
		int num = 0;
		foreach (Controller controller2 in player.controllers.Controllers)
		{
			if (controller2.type != controller.type)
			{
				continue;
			}
			bool flag = false;
			if (controller.type == ControllerType.Joystick)
			{
				if ((controller2 as Joystick).hardwareTypeGuid != controller.hardwareTypeGuid)
				{
					continue;
				}
				if (controller.hardwareTypeGuid != Guid.Empty)
				{
					flag = true;
				}
			}
			if (flag || !(controller2.hardwareIdentifier != controller.hardwareIdentifier))
			{
				if (controller2 == controller)
				{
					return num;
				}
				num++;
			}
		}
		return num;
	}

	private void RefreshLayoutManager(int playerId)
	{
		ReInput.players.GetPlayer(playerId)?.controllers.maps.layoutManager.Apply();
	}

	private void OnControllerMapsSaved(Player player)
	{
		if (_actionMappingSaveMode != ActionMappingSaveMode.ByControllerElementRole)
		{
			return;
		}
		int joystickCount = player.controllers.joystickCount;
		if (joystickCount > 1)
		{
			for (int i = 0; i < joystickCount; i++)
			{
				LoadControllerMaps(player.id, ControllerType.Joystick, player.controllers.Joysticks[i].id);
			}
			RefreshLayoutManager(player.id);
		}
	}

	private static Type GetControllerMapType(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Custom:
			return typeof(CustomControllerMap);
		case ControllerType.Joystick:
			return typeof(JoystickMap);
		case ControllerType.Keyboard:
			return typeof(KeyboardMap);
		case ControllerType.Mouse:
			return typeof(MouseMap);
		default:
			Debug.LogWarning("Rewired: Unknown ControllerType " + controllerType);
			return null;
		}
	}

	private static int SortOldestToNewest(ControllerMapSaveData a, ControllerMapSaveData b)
	{
		if (a.map == null)
		{
			if (b.map == null)
			{
				return 0;
			}
			return -1;
		}
		if (b.map == null)
		{
			return 1;
		}
		return a.map.modifiedTime.CompareTo(b.map.modifiedTime);
	}
}
