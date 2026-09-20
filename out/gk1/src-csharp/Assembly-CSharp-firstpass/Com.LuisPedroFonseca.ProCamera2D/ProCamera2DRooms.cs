using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Com.LuisPedroFonseca.ProCamera2D;

[HelpURL("http://www.procamera2d.com/user-guide/extension-rooms/")]
public class ProCamera2DRooms : BasePC2D, IPositionOverrider, ISizeOverrider
{
	public const string ExtensionName = "Rooms";

	private int _currentRoomIndex = -1;

	private int _previousRoomIndex = -1;

	public List<Room> Rooms = new List<Room>();

	public float UpdateInterval = 0.1f;

	public bool UseTargetsMidPoint = true;

	public Transform TriggerTarget;

	public bool TransitionInstanlyOnStart = true;

	public bool RestoreOnRoomExit;

	public float RestoreDuration = 1f;

	public EaseType RestoreEaseType;

	public bool AutomaticRoomActivation = true;

	public RoomEvent OnStartedTransition;

	public RoomEvent OnFinishedTransition;

	public UnityEvent OnExitedAllRooms;

	private ProCamera2DNumericBoundaries _numericBoundaries;

	private NumericBoundariesSettings _defaultNumericBoundariesSettings;

	private bool _transitioning;

	private Vector3 _newPos;

	private float _newSize;

	private Coroutine _transitionRoutine;

	private float _originalSize;

	private int _poOrder = 1001;

	private int _soOrder = 3001;

	public int CurrentRoomIndex => _currentRoomIndex;

	public int PreviousRoomIndex => _previousRoomIndex;

	public int POOrder
	{
		get
		{
			return _poOrder;
		}
		set
		{
			_poOrder = value;
		}
	}

	public int SOOrder
	{
		get
		{
			return _soOrder;
		}
		set
		{
			_soOrder = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		_numericBoundaries = ProCamera2D.GetComponent<ProCamera2DNumericBoundaries>();
		_defaultNumericBoundariesSettings = _numericBoundaries.Settings;
		_originalSize = ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f;
		ProCamera2D.Instance.AddPositionOverrider(this);
		ProCamera2D.Instance.AddSizeOverrider(this);
	}

	private void Start()
	{
		StartCoroutine(TestRoomRoutine());
		if (TransitionInstanlyOnStart)
		{
			Vector3 targetPos = ProCamera2D.TargetsMidPoint;
			if (!UseTargetsMidPoint && TriggerTarget != null)
			{
				targetPos = TriggerTarget.position;
			}
			int num = ComputeCurrentRoom(targetPos);
			if (num != -1)
			{
				EnterRoom(num, useTransition: false);
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ProCamera2D.RemovePositionOverrider(this);
		ProCamera2D.RemoveSizeOverrider(this);
	}

	public Vector3 OverridePosition(float deltaTime, Vector3 originalPosition)
	{
		if (!base.enabled)
		{
			return originalPosition;
		}
		if (_transitioning)
		{
			return _newPos;
		}
		return originalPosition;
	}

	public float OverrideSize(float deltaTime, float originalSize)
	{
		if (!base.enabled)
		{
			return originalSize;
		}
		if (_transitioning)
		{
			return _newSize;
		}
		return originalSize;
	}

	public void TestRoom()
	{
		Vector3 targetPos = ProCamera2D.TargetsMidPoint;
		if (!UseTargetsMidPoint && TriggerTarget != null)
		{
			targetPos = TriggerTarget.position;
		}
		int num = ComputeCurrentRoom(targetPos);
		if (num != -1 && _currentRoomIndex != num)
		{
			EnterRoom(num);
		}
		if (num == -1 && _currentRoomIndex != -1)
		{
			ExitRoom();
		}
	}

	public int ComputeCurrentRoom(Vector3 targetPos)
	{
		int result = -1;
		for (int i = 0; i < Rooms.Count; i++)
		{
			if (Utils.IsInsideRectangle(Rooms[i].Dimensions.x, Rooms[i].Dimensions.y, Rooms[i].Dimensions.width, Rooms[i].Dimensions.height, Vector3H(targetPos), Vector3V(targetPos)))
			{
				result = i;
			}
		}
		return result;
	}

	public void EnterRoom(int roomIndex, bool useTransition = true)
	{
		if (roomIndex < 0 || roomIndex > Rooms.Count - 1)
		{
			Debug.LogError("Can't find room with index: " + roomIndex);
		}
		else if (roomIndex != _currentRoomIndex)
		{
			_previousRoomIndex = _currentRoomIndex;
			_currentRoomIndex = roomIndex;
			TransitionToRoom(Rooms[_currentRoomIndex], useTransition);
			if (OnStartedTransition != null)
			{
				OnStartedTransition.Invoke(roomIndex, _previousRoomIndex);
			}
		}
	}

	public void EnterRoom(string roomID, bool useTransition = true)
	{
		EnterRoom(Rooms.FindIndex((Room room) => room.ID == roomID), useTransition);
	}

	public void ExitRoom()
	{
		if (RestoreOnRoomExit)
		{
			_currentRoomIndex = -1;
			if (OnStartedTransition != null)
			{
				OnStartedTransition.Invoke(_currentRoomIndex, _previousRoomIndex);
			}
			if (_transitionRoutine != null)
			{
				StopCoroutine(_transitionRoutine);
			}
			_transitionRoutine = StartCoroutine(TransitionRoutine(_defaultNumericBoundariesSettings, _originalSize, RestoreDuration, RestoreEaseType));
		}
		if (OnExitedAllRooms != null)
		{
			OnExitedAllRooms.Invoke();
		}
	}

	public void AddRoom(float roomX, float roomY, float roomWidth, float roomHeight, float transitionDuration = 1f, EaseType transitionEaseType = EaseType.EaseInOut, bool scaleToFit = false, bool zoom = false, float zoomScale = 1.5f, string id = "")
	{
		Room item = new Room
		{
			ID = id,
			Dimensions = new Rect(roomX, roomY, roomWidth, roomHeight),
			TransitionDuration = transitionDuration,
			TransitionEaseType = transitionEaseType,
			ScaleCameraToFit = scaleToFit,
			Zoom = zoom,
			ZoomScale = zoomScale
		};
		Rooms.Add(item);
	}

	public void RemoveRoom(string roomName)
	{
		Room room = Rooms.Find((Room obj) => obj.ID == roomName);
		if (room != null)
		{
			Rooms.Remove(room);
		}
		else
		{
			Debug.LogWarning(roomName + " not found in the Rooms list.");
		}
	}

	public void SetDefaultNumericBoundariesSettings(NumericBoundariesSettings settings)
	{
		_defaultNumericBoundariesSettings = settings;
	}

	public Room GetRoom(string roomID)
	{
		return Rooms.Find((Room obj) => obj.ID == roomID);
	}

	private IEnumerator TestRoomRoutine()
	{
		yield return new WaitForEndOfFrame();
		WaitForSeconds waitForSeconds = new WaitForSeconds(UpdateInterval);
		while (true)
		{
			if (AutomaticRoomActivation)
			{
				TestRoom();
			}
			yield return waitForSeconds;
		}
	}

	private void TransitionToRoom(Room room, bool useTransition = true)
	{
		if (_transitionRoutine != null)
		{
			StopCoroutine(_transitionRoutine);
		}
		NumericBoundariesSettings numericBoundariesSettings = default(NumericBoundariesSettings);
		numericBoundariesSettings.UseNumericBoundaries = true;
		numericBoundariesSettings.UseTopBoundary = true;
		numericBoundariesSettings.TopBoundary = room.Dimensions.y + room.Dimensions.height / 2f;
		numericBoundariesSettings.UseBottomBoundary = true;
		numericBoundariesSettings.BottomBoundary = room.Dimensions.y - room.Dimensions.height / 2f;
		numericBoundariesSettings.UseLeftBoundary = true;
		numericBoundariesSettings.LeftBoundary = room.Dimensions.x - room.Dimensions.width / 2f;
		numericBoundariesSettings.UseRightBoundary = true;
		numericBoundariesSettings.RightBoundary = room.Dimensions.x + room.Dimensions.width / 2f;
		NumericBoundariesSettings numericBoundariesSettings2 = numericBoundariesSettings;
		float num = ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f;
		float cameraSizeForRoom = GetCameraSizeForRoom(room.Dimensions);
		if (room.ScaleCameraToFit)
		{
			num = cameraSizeForRoom;
		}
		else if (room.Zoom && _originalSize * room.ZoomScale < cameraSizeForRoom)
		{
			num = _originalSize * room.ZoomScale;
		}
		else if (cameraSizeForRoom < num)
		{
			num = cameraSizeForRoom;
		}
		_transitionRoutine = StartCoroutine(TransitionRoutine(numericBoundariesSettings2, num, useTransition ? room.TransitionDuration : 0f, room.TransitionEaseType));
	}

	private IEnumerator TransitionRoutine(NumericBoundariesSettings numericBoundariesSettings, float targetSize, float transitionDuration = 1f, EaseType transitionEaseType = EaseType.EaseOut)
	{
		_transitioning = true;
		_numericBoundaries.UseNumericBoundaries = false;
		float initialSize = ProCamera2D.ScreenSizeInWorldCoordinates.y / 2f;
		float initialCamPosH = Vector3H(ProCamera2D.LocalPosition);
		float initialCamPosV = Vector3V(ProCamera2D.LocalPosition);
		float t = 0f;
		while (t <= 1f)
		{
			if (transitionDuration < float.Epsilon)
			{
				t = 1.1f;
			}
			else if (ProCamera2D.DeltaTime > float.Epsilon)
			{
				t += ProCamera2D.DeltaTime / transitionDuration;
			}
			_newSize = Utils.EaseFromTo(initialSize, targetSize, t, transitionEaseType);
			float horizontalPos = ProCamera2D.CameraTargetPositionSmoothed.x;
			float verticalPos = ProCamera2D.CameraTargetPositionSmoothed.y;
			LimitToNumericBoundaries(ref horizontalPos, ref verticalPos, targetSize * ProCamera2D.GameCamera.aspect, targetSize, numericBoundariesSettings);
			float arg = Utils.EaseFromTo(initialCamPosH, horizontalPos, t, transitionEaseType);
			float arg2 = Utils.EaseFromTo(initialCamPosV, verticalPos, t, transitionEaseType);
			_newPos = VectorHVD(arg, arg2, 0f);
			yield return ProCamera2D.GetYield();
		}
		_transitioning = false;
		_numericBoundaries.Settings = numericBoundariesSettings;
		_transitionRoutine = null;
		if (OnFinishedTransition != null)
		{
			OnFinishedTransition.Invoke(_currentRoomIndex, _previousRoomIndex);
		}
		_previousRoomIndex = _currentRoomIndex;
	}

	private void LimitToNumericBoundaries(ref float horizontalPos, ref float verticalPos, float halfCameraWidth, float halfCameraHeight, NumericBoundariesSettings numericBoundaries)
	{
		if (numericBoundaries.UseLeftBoundary && horizontalPos - halfCameraWidth < numericBoundaries.LeftBoundary)
		{
			horizontalPos = numericBoundaries.LeftBoundary + halfCameraWidth;
		}
		else if (numericBoundaries.UseRightBoundary && horizontalPos + halfCameraWidth > numericBoundaries.RightBoundary)
		{
			horizontalPos = numericBoundaries.RightBoundary - halfCameraWidth;
		}
		if (numericBoundaries.UseBottomBoundary && verticalPos - halfCameraHeight < numericBoundaries.BottomBoundary)
		{
			verticalPos = numericBoundaries.BottomBoundary + halfCameraHeight;
		}
		else if (numericBoundaries.UseTopBoundary && verticalPos + halfCameraHeight > numericBoundaries.TopBoundary)
		{
			verticalPos = numericBoundaries.TopBoundary - halfCameraHeight;
		}
	}

	private float GetCameraSizeForRoom(Rect roomRect)
	{
		float num = roomRect.width / ProCamera2D.ScreenSizeInWorldCoordinates.x;
		float num2 = roomRect.height / ProCamera2D.ScreenSizeInWorldCoordinates.y;
		if (num < num2)
		{
			return roomRect.width / ProCamera2D.GameCamera.aspect / 2f;
		}
		return roomRect.height / 2f;
	}
}
