using System;
using System.Collections.Generic;
using LazyBearTechnology;
using Pathfinding;
using UnityEngine;

[Serializable]
public class MovementComponent
{
	public enum Status
	{
		None,
		PathBuilding,
		Moving
	}

	public enum CompletionState
	{
		None,
		Success,
		Fail,
		Canceled
	}

	public enum StartPathResult
	{
		Started,
		AlreadyAtDestinationPoint,
		IncorrectMovementType
	}

	public enum DestinationType
	{
		Position,
		DockPoint,
		PointOfInterest
	}

	private const float TARGET_OFFSET = 0.1f;

	private const float TARGET_OFFSET_SQR = 0.010000001f;

	private const float EXACT_DEST_OFFSET_SQR = 1.0000001E-06f;

	private const int MAX_PATH_ADVANCE_ITERATIONS = 64;

	[NonSerialized]
	private IMovable assignedMovableObject;

	[SerializeField]
	private List<MovementData> movementData = new List<MovementData>();

	[SerializeField]
	private DestinationType destinationType;

	[SerializeField]
	private Status status;

	[SerializeField]
	private CompletionState completionState;

	[SerializeField]
	private int currentMovementDataIndex;

	[SerializeField]
	private int currentPathTargetIndex;

	[SerializeField]
	private Vector3 startPos;

	[SerializeField]
	private Vector3 endPos;

	[SerializeField]
	private DockPointData.Baked endDockPointData;

	[SerializeField]
	private NPCPointOfInterestData targetPointOfInterest;

	[SerializeField]
	private string startWorldId;

	[SerializeField]
	private string endWorldId;

	[SerializeField]
	private string currentWorldId;

	[SerializeField]
	private float speed;

	[SerializeField]
	private MovementType movementType;

	[SerializeField]
	private string onPathCompleteEvent;

	[SerializeField]
	private GraphMask graphMask;

	[SerializeField]
	private bool useVectorPathAsSource;

	[SerializeField]
	private bool isPaused;

	private Action onFinishPath;

	private ABPath wholePath;

	private Seeker seeker;

	private Vector3 currentPathTarget;

	private MovementData currentMovementData;

	private int movementDataCount;

	private int partialPathCount;

	private Action<float> moveMethod;

	private Vector2 prevDirection;

	private Action<float> onStartAction;

	private Action onFinishAction;

	private Action<Vector2> onDirectionChange;

	private Action<float> onPathLengthReady;

	private uint pathRequestId;

	private int CurrentMovementDataIndex
	{
		get
		{
			return currentMovementDataIndex;
		}
		set
		{
			if (value >= 0 && value < movementData.Count)
			{
				currentMovementDataIndex = value;
				currentMovementData = movementData[currentMovementDataIndex];
				partialPathCount = currentMovementData.worldPath.Count - 1;
			}
		}
	}

	private int CurrentPathTargetIndex
	{
		get
		{
			return currentPathTargetIndex;
		}
		set
		{
			if (IsValidPathTargetIndex(value))
			{
				currentPathTargetIndex = value;
				currentPathTarget = movementData[currentMovementDataIndex].worldPath[currentPathTargetIndex];
			}
		}
	}

	public string OnPathCompleteEvent => onPathCompleteEvent;

	public CompletionState Completion => completionState;

	public bool IsMoving => status == Status.Moving;

	public DestinationType TypeDestination => destinationType;

	public bool IsPaused
	{
		get
		{
			return isPaused;
		}
		set
		{
			isPaused = value;
		}
	}

	private bool UsesExactRequestedDestination
	{
		get
		{
			MovementType movementType = this.movementType;
			return movementType == MovementType.Direct || movementType == MovementType.GDGraph;
		}
	}

	public bool ShouldRegisterInMovementSystem()
	{
		return IsMoving;
	}

	public void Init(IMovable movableObject)
	{
		assignedMovableObject = movableObject;
		if (IsMoving)
		{
			UpdatePathData();
		}
		switch (status)
		{
		case Status.PathBuilding:
			DoPathCalculations();
			break;
		case Status.Moving:
			UpdatePathData();
			break;
		}
	}

	public void SetCallbacks(Action<float> onStartAction = null, Action onFinishAction = null, Action<Vector2> onDirectionChange = null)
	{
		this.onStartAction = onStartAction;
		this.onFinishAction = onFinishAction;
		this.onDirectionChange = onDirectionChange;
		if (IsMoving)
		{
			this.onStartAction?.Invoke(speed);
		}
	}

	public void SetOnPathLengthReady(Action<float> callback)
	{
		onPathLengthReady = callback;
	}

	public void DeInit()
	{
		onStartAction = null;
		onFinishAction = null;
		onDirectionChange = null;
		onPathLengthReady = null;
	}

	public void Update(float deltaTime)
	{
		if (!isPaused && status == Status.Moving)
		{
			moveMethod?.Invoke(deltaTime);
		}
	}

	public StartPathResult StartPath(Vector3 endPos, DockPointData.Baked endDockPointData, string startWorldId, string destinationWorldId, MovementType movementType = MovementType.Recast, float speed = 1.5f, string onPathCompleteEvent = "", Action onFinish = null, Seeker customSeeker = null)
	{
		StartPathResult num = StartPath(endPos, startWorldId, destinationWorldId, movementType, speed, onPathCompleteEvent, onFinish, customSeeker, DestinationType.DockPoint);
		if (num == StartPathResult.Started)
		{
			this.endDockPointData = endDockPointData;
		}
		return num;
	}

	public StartPathResult StartPath(NPCPointOfInterestData targetPoint, float speed = 1.5f)
	{
		GDPointData gDPointData = targetPoint.GDPointData;
		if (gDPointData == null)
		{
			return StartPathResult.IncorrectMovementType;
		}
		StartPathResult num = StartPath(gDPointData.Position, gDPointData.GameSceneDataId, gDPointData.GameSceneDataId, MovementType.GDGraph, speed, string.Empty, null, null, DestinationType.PointOfInterest);
		if (num == StartPathResult.Started)
		{
			targetPointOfInterest = targetPoint;
		}
		return num;
	}

	public StartPathResult StartPath(Vector3 endPos, string startWorldId, string destinationWorldId, MovementType movementType = MovementType.Recast, float speed = 1.5f, string onPathCompleteEvent = "", Action onFinish = null, Seeker customSeeker = null, DestinationType destinationType = DestinationType.Position)
	{
		if (TryFinishAlreadyAtDestination(endPos, movementType, onFinish))
		{
			return StartPathResult.AlreadyAtDestinationPoint;
		}
		if (movementType == MovementType.None || movementType == MovementType.WorldZone)
		{
			Debug.LogError($"MC: [{assignedMovableObject.MovableObjectId}] cannot start path with movement type [{movementType}]");
			onFinish?.Invoke();
			return StartPathResult.IncorrectMovementType;
		}
		completionState = CompletionState.None;
		PrepareForNewPath();
		movementData.Clear();
		startPos = assignedMovableObject.MovablePosition;
		this.endPos = endPos;
		endWorldId = destinationWorldId;
		this.movementType = movementType;
		this.startWorldId = startWorldId;
		currentWorldId = this.startWorldId;
		this.speed = speed;
		this.onPathCompleteEvent = onPathCompleteEvent;
		onFinishPath = onFinish;
		seeker = customSeeker;
		this.destinationType = destinationType;
		DoPathCalculations();
		assignedMovableObject.OnPathStart();
		return StartPathResult.Started;
	}

	public StartPathResult StartPath(Vector3 endPos, DockPointData.Baked endDockPointData, LazyConsts.Navigation.Graph graph, string startWorldId, float speed = 1.5f, string onPathCompleteEvent = "", Action onFinish = null)
	{
		return StartPath(endPos, endDockPointData, NavigationGraphMaskUtils.ToGraphMask(graph), startWorldId, speed, onPathCompleteEvent, onFinish);
	}

	public StartPathResult StartPath(Vector3 endPos, DockPointData.Baked endDockPointData, GraphMask graphMask, string startWorldId, float speed = 1.5f, string onPathCompleteEvent = "", Action onFinish = null)
	{
		StartPathResult num = StartPath(endPos, graphMask, startWorldId, speed, onPathCompleteEvent, onFinish, DestinationType.DockPoint);
		if (num == StartPathResult.Started)
		{
			this.endDockPointData = endDockPointData;
		}
		return num;
	}

	public StartPathResult StartPath(Vector3 endPos, LazyConsts.Navigation.Graph graph, string startWorldId, float speed = 1.5f, string onPathCompleteEvent = "", Action onFinish = null, DestinationType destinationType = DestinationType.Position)
	{
		return StartPath(endPos, NavigationGraphMaskUtils.ToGraphMask(graph), startWorldId, speed, onPathCompleteEvent, onFinish, destinationType);
	}

	public StartPathResult StartPath(Vector3 endPos, GraphMask graphMask, string startWorldId, float speed = 1.5f, string onPathCompleteEvent = "", Action onFinish = null, DestinationType destinationType = DestinationType.Position)
	{
		if (TryFinishAlreadyAtDestination(endPos, MovementType.WorldZone, onFinish))
		{
			return StartPathResult.AlreadyAtDestinationPoint;
		}
		completionState = CompletionState.None;
		this.destinationType = destinationType;
		this.graphMask = graphMask;
		movementType = MovementType.WorldZone;
		PrepareForNewPath();
		movementData.Clear();
		startPos = assignedMovableObject.MovablePosition;
		this.endPos = endPos;
		this.startWorldId = startWorldId;
		endWorldId = startWorldId;
		currentWorldId = this.startWorldId;
		this.speed = speed;
		this.onPathCompleteEvent = onPathCompleteEvent;
		onFinishPath = onFinish;
		DoPathCalculations();
		assignedMovableObject.OnPathStart();
		return StartPathResult.Started;
	}

	public void ForceStop()
	{
		onFinishAction?.Invoke();
		onFinishPath = null;
		onPathLengthReady = null;
		status = Status.None;
		completionState = CompletionState.Canceled;
		MainGame.Instance.movementSystem.RemoveMovingObject(this);
		assignedMovableObject.OnPathComplete(this);
	}

	private bool TryFinishAlreadyAtDestination(Vector3 destination, MovementType type, Action onFinish)
	{
		if (type == MovementType.Direct || type == MovementType.GDGraph)
		{
			if (!IsAtExactDestinationXZ(destination))
			{
				return false;
			}
		}
		else if ((assignedMovableObject.MovablePosition - destination).sqrMagnitude >= 0.010000001f)
		{
			return false;
		}
		Debug.Log($"MC: [{assignedMovableObject.MovableObjectId}] already at destination position [{destination}]");
		onFinish?.Invoke();
		return true;
	}

	private bool IsAtExactDestinationXZ(Vector3 destination)
	{
		return (destination - assignedMovableObject.MovablePosition).XZ().sqrMagnitude <= 1.0000001E-06f;
	}

	private void SnapToExactDestinationIfNeeded()
	{
		if (UsesExactRequestedDestination)
		{
			assignedMovableObject.MovablePositionWithoutDirectionChange = endPos;
		}
	}

	private bool IsOnFinalDestinationWaypoint()
	{
		if (currentPathTargetIndex == partialPathCount)
		{
			return currentMovementDataIndex == movementDataCount;
		}
		return false;
	}

	private void AppendExactDestinationWaypoint(List<Vector3> worldPath)
	{
		if (movementType != MovementType.GDGraph)
		{
			worldPath.Add(endPos);
		}
		else if (worldPath.Count == 0)
		{
			worldPath.Add(endPos);
		}
		else if ((worldPath[worldPath.Count - 1] - endPos).sqrMagnitude <= 0.010000001f)
		{
			worldPath[worldPath.Count - 1] = endPos;
		}
		else
		{
			worldPath.Add(endPos);
		}
	}

	private void PrepareForNewPath()
	{
		pathRequestId++;
		MainGame.Instance?.movementSystem?.RemoveMovingObject(this);
		currentMovementDataIndex = 0;
		currentPathTargetIndex = 0;
		partialPathCount = 0;
		moveMethod = null;
	}

	private bool IsValidPathTargetIndex(int index)
	{
		if (index >= 0 && currentMovementDataIndex >= 0 && currentMovementDataIndex < movementData.Count)
		{
			return index < movementData[currentMovementDataIndex].worldPath.Count;
		}
		return false;
	}

	private void OnPathCalculatedIfCurrent(uint requestId, Path p)
	{
		if (requestId == pathRequestId && status == Status.PathBuilding)
		{
			OnPathCalculated(p);
		}
	}

	private void FindPathRecastGraph(OnPathDelegate callback = null)
	{
		int num = GraphHelper.Instance.SceneGraphsData.GetRecastGraphIndexByWorldId(startWorldId)[0];
		LazyConsts.Navigation.Graph graph = (LazyConsts.Navigation.Graph)num;
		if (graph == LazyConsts.Navigation.Graph.None)
		{
			onPathLengthReady = null;
			return;
		}
		if (!IsReachable(startPos, endPos, num))
		{
			Debug.LogError(GetPathCalculationError());
			onPathLengthReady = null;
			return;
		}
		status = Status.PathBuilding;
		uint requestId = pathRequestId;
		wholePath = new ABPath();
		wholePath.path = new List<GraphNode>();
		wholePath.vectorPath = new List<Vector3>();
		useVectorPathAsSource = true;
		LazySingleton<GlobalNavigationManager>.Instance.CalculatePath(graph, startPos, endPos, delegate(Path p)
		{
			OnPathCalculatedIfCurrent(requestId, p);
		});
	}

	private void FindPathGDGraph(OnPathDelegate callback = null)
	{
		status = Status.PathBuilding;
		uint requestId = pathRequestId;
		wholePath = new ABPath();
		wholePath.path = new List<GraphNode>();
		wholePath.vectorPath = new List<Vector3>();
		ABPath aBPath = ABPath.Construct(startPos, endPos, delegate(Path p)
		{
			if (requestId == pathRequestId)
			{
				OnPathCalculated(p);
				Debug.Log($"MC: GDGraph path: startPos [{startPos}], endPos [{endPos}], path [{p.path.Count}], nodes");
			}
		});
		aBPath.traversalConstraint.graphMask = new GraphMask((uint)GraphHelper.Instance.SceneGraphsData.GDPointGraphMask);
		AstarPath.StartPath(aBPath);
	}

	private void FindPathWorldZone()
	{
		status = Status.PathBuilding;
		uint requestId = pathRequestId;
		wholePath = new ABPath();
		wholePath.path = new List<GraphNode>();
		wholePath.vectorPath = new List<Vector3>();
		useVectorPathAsSource = true;
		LazySingleton<GlobalNavigationManager>.Instance.CalculatePath(graphMask, startPos, endPos, delegate(Path p)
		{
			OnPathCalculatedIfCurrent(requestId, p);
		});
	}

	private bool IsReachedDestinationPoint()
	{
		if (!IsOnFinalDestinationWaypoint())
		{
			return false;
		}
		if (!(UsesExactRequestedDestination ? IsAtExactDestinationXZ(currentPathTarget) : (Vector3.Distance(currentPathTarget, assignedMovableObject.MovablePosition) < 0.1f)))
		{
			return false;
		}
		OnDestinationReachSucceed();
		return true;
	}

	private bool TransitToNextWorld()
	{
		if (Vector3.Distance(currentPathTarget, assignedMovableObject.MovablePosition) < 0.1f && currentPathTargetIndex == partialPathCount && currentMovementDataIndex != movementDataCount && !string.IsNullOrEmpty(currentMovementData.transitionWorldId))
		{
			Debug.Log("MC: [" + assignedMovableObject.MovableObjectId + " transition point: [" + currentWorldId + "]-->[" + currentMovementData.transitionWorldId + "]]");
			assignedMovableObject.OnTransitionReached(currentWorldId, movementData[currentMovementDataIndex].transitionWorldId);
			currentWorldId = currentMovementData.transitionWorldId;
			CurrentMovementDataIndex++;
			assignedMovableObject.MovablePositionWithoutDirectionChange = currentMovementData.worldPath[0];
			CurrentPathTargetIndex = ((currentMovementData.worldPath.Count > 1) ? 1 : 0);
			return true;
		}
		return false;
	}

	private bool IsReachedNextWaypoint()
	{
		if (Vector3.Distance(currentPathTarget, assignedMovableObject.MovablePosition) < 0.1f && currentPathTargetIndex < partialPathCount)
		{
			CurrentPathTargetIndex++;
			return true;
		}
		return false;
	}

	private bool IsAtCurrentPathTarget()
	{
		return Vector3.Distance(currentPathTarget, assignedMovableObject.MovablePosition) < 0.1f;
	}

	private bool ApplyMovementStepTowardTarget(ref float remainingStep, bool requireExactArrival = false)
	{
		bool flag = (requireExactArrival ? IsAtExactDestinationXZ(currentPathTarget) : IsAtCurrentPathTarget());
		if (remainingStep <= 0f || flag)
		{
			return flag;
		}
		Vector3 movablePosition = assignedMovableObject.MovablePosition;
		Vector3 vector = (requireExactArrival ? (currentPathTarget - movablePosition).XZ() : (currentPathTarget - movablePosition));
		float sqrMagnitude = vector.sqrMagnitude;
		float magnitude = vector.magnitude;
		Vector3 vector2 = ((magnitude > 0.0001f) ? (vector / magnitude) : Vector3.zero);
		Vector3 vector3;
		if ((remainingStep - magnitude).EqualsOrMore(0f))
		{
			vector3 = (requireExactArrival ? (movablePosition + vector) : currentPathTarget);
			remainingStep -= magnitude;
		}
		else
		{
			vector3 = movablePosition + remainingStep * vector2;
			remainingStep = 0f;
		}
		prevDirection = assignedMovableObject.MovableDirection;
		if (sqrMagnitude > 0.010000001f)
		{
			Vector2 vector4 = new Vector2(vector2.x, vector2.z);
			assignedMovableObject.MovableDirection = vector4;
			CheckDirectionChanged(prevDirection, vector4);
			assignedMovableObject.MovablePosition = vector3;
		}
		else
		{
			assignedMovableObject.MovablePositionWithoutDirectionChange = vector3;
		}
		if (!requireExactArrival)
		{
			return IsAtCurrentPathTarget();
		}
		return IsAtExactDestinationXZ(currentPathTarget);
	}

	private bool ProcessPathTransitionsAtCurrentTarget(out bool pathFinished)
	{
		pathFinished = false;
		if (!((UsesExactRequestedDestination && IsOnFinalDestinationWaypoint()) ? IsAtExactDestinationXZ(currentPathTarget) : IsAtCurrentPathTarget()))
		{
			return false;
		}
		if (TransitToNextPoint())
		{
			return true;
		}
		if (IsReachedNextWaypoint())
		{
			return true;
		}
		if (IsReachedDestinationPoint())
		{
			pathFinished = true;
			return false;
		}
		if (TransitToNextWorld())
		{
			return true;
		}
		return false;
	}

	private bool IsReachedDestinationDirect()
	{
		if (!IsAtExactDestinationXZ(endPos))
		{
			return false;
		}
		OnDestinationReachSucceed();
		return true;
	}

	private void MoveDirect(float deltaTime)
	{
		if (IsReachedDestinationDirect())
		{
			return;
		}
		Vector3 movablePosition = assignedMovableObject.MovablePosition;
		Vector3 vector = (endPos - movablePosition).XZ();
		float magnitude = vector.magnitude;
		float num = deltaTime * speed;
		if (magnitude <= 0.0001f)
		{
			OnDestinationReachSucceed();
			return;
		}
		Vector2 vector2 = vector.XZ2() / magnitude;
		prevDirection = assignedMovableObject.MovableDirection;
		assignedMovableObject.MovableDirection = vector2;
		CheckDirectionChanged(prevDirection, vector2);
		if ((num - magnitude).EqualsOrMore(0f))
		{
			assignedMovableObject.MovablePosition = movablePosition + vector;
			OnDestinationReachSucceed();
		}
		else
		{
			assignedMovableObject.MovablePosition = movablePosition + num * vector2.XZ();
		}
	}

	private void MoveAStar(float deltaTime)
	{
		if (status != Status.Moving || movementData.Count == 0)
		{
			return;
		}
		float remainingStep = deltaTime * speed;
		for (int i = 0; i < 64; i++)
		{
			if (!remainingStep.More(0f))
			{
				break;
			}
			bool flag = UsesExactRequestedDestination && IsOnFinalDestinationWaypoint();
			if (((!IsAtCurrentPathTarget() || (flag && !IsAtExactDestinationXZ(currentPathTarget))) && !ApplyMovementStepTowardTarget(ref remainingStep, flag)) || !ProcessPathTransitionsAtCurrentTarget(out var _))
			{
				break;
			}
		}
	}

	private void OnPathCalculated(Path p)
	{
		if (status != Status.PathBuilding)
		{
			return;
		}
		_ = endPos;
		if (p.CompleteState != PathCompleteState.Complete)
		{
			Debug.LogError(GetPathCalculationError());
			OnDestinationReachFailed();
			return;
		}
		MovementType movementType = this.movementType;
		if (movementType == MovementType.WorldZone || movementType == MovementType.Recast)
		{
			List<Vector3> vectorPath = p.vectorPath;
			endPos = vectorPath[vectorPath.Count - 1];
		}
		wholePath.path.AddRange(p.path);
		wholePath.vectorPath = new List<Vector3>(p.vectorPath);
		CreateMovementData();
		NotifyPathLengthReady(p.GetTotalLength());
		onStartAction?.Invoke(speed);
	}

	private bool TransitToNextPoint()
	{
		if (Vector3.Distance(currentPathTarget, assignedMovableObject.MovablePosition) < 0.1f && currentPathTargetIndex == partialPathCount && currentMovementDataIndex != movementDataCount && currentMovementData.lastPointTransitToNextMovementData)
		{
			CurrentMovementDataIndex++;
			CurrentPathTargetIndex = ((currentMovementData.worldPath.Count > 1) ? 1 : 0);
			Debug.Log($"MC: [{assignedMovableObject.MovableObjectId} teleport to transition point, because last in movement data cur pos is:[{assignedMovableObject.MovablePosition}] newPos is:[{movementData[currentMovementDataIndex].worldPath[0]}]");
			UpdatePathData();
			Vector3 movablePosition = assignedMovableObject.MovablePosition;
			assignedMovableObject.MovablePositionWithoutDirectionChange = currentMovementData.worldPath[0];
			assignedMovableObject.OnTeleportToTransitPoint(movablePosition, assignedMovableObject.MovablePosition);
			return true;
		}
		return false;
	}

	private void CreateMovementData()
	{
		movementData.Clear();
		int num = 0;
		movementData.Add(new MovementData
		{
			worldPath = new List<Vector3>()
		});
		movementData[num].worldPath.Add(startPos);
		if (!useVectorPathAsSource)
		{
			movementData[num].worldPath.Add((Vector3)wholePath.path[0].position);
			for (int i = 1; i < wholePath.path.Count; i++)
			{
				if (wholePath.path[i] is GDPointNode gDPointNode)
				{
					if (!string.IsNullOrEmpty(gDPointNode.GdPointData.GameSceneDataIdToTransit))
					{
						movementData[num].transitionWorldId = gDPointNode.GdPointData.GameSceneDataIdToTransit;
						movementData[num].worldPath.Add((Vector3)gDPointNode.position);
						movementData.Add(new MovementData
						{
							worldPath = new List<Vector3>()
						});
						num++;
						continue;
					}
					if (string.IsNullOrEmpty(gDPointNode.GdPointData.GameSceneDataIdToTransit) && gDPointNode.GdPointData.IsTransitPoint && !string.IsNullOrEmpty(gDPointNode.GdPointData.TransitToGdPointId))
					{
						movementData[num].lastPointTransitToNextMovementData = true;
						movementData[num].worldPath.Add((Vector3)gDPointNode.position);
						movementData.Add(new MovementData
						{
							worldPath = new List<Vector3>()
						});
						num++;
						continue;
					}
				}
				movementData[num].worldPath.Add((Vector3)wholePath.path[i].position);
			}
		}
		else
		{
			movementData[num].worldPath.AddRange(wholePath.vectorPath);
		}
		AppendExactDestinationWaypoint(movementData[num].worldPath);
		status = Status.Moving;
		currentMovementDataIndex = 0;
		currentPathTargetIndex = 0;
		UpdatePathData();
		MainGame.Instance.movementSystem.AddMovingObject(this);
		Debug.Log("MC: Starting path for wgo [" + assignedMovableObject.MovableObjectId + "]");
	}

	private void DoPathCalculations()
	{
		switch (movementType)
		{
		case MovementType.GDGraph:
			FindPathGDGraph();
			break;
		case MovementType.Recast:
			FindPathRecastGraph();
			break;
		case MovementType.Direct:
			status = Status.Moving;
			moveMethod = MoveDirect;
			NotifyPathLengthReady(Vector3.Distance(startPos, endPos));
			onStartAction?.Invoke(speed);
			MainGame.Instance.movementSystem.AddMovingObject(this);
			break;
		case MovementType.WorldZone:
			FindPathWorldZone();
			break;
		}
	}

	private void UpdatePathData()
	{
		switch (movementType)
		{
		case MovementType.Recast:
		case MovementType.GDGraph:
		case MovementType.WorldZone:
			if (movementData.Count == 0 || currentMovementDataIndex < 0 || currentMovementDataIndex >= movementData.Count || !IsValidPathTargetIndex(currentPathTargetIndex))
			{
				moveMethod = null;
				break;
			}
			currentMovementData = movementData[currentMovementDataIndex];
			currentPathTarget = currentMovementData.worldPath[currentPathTargetIndex];
			movementDataCount = movementData.Count - 1;
			partialPathCount = currentMovementData.worldPath.Count - 1;
			moveMethod = MoveAStar;
			break;
		case MovementType.Direct:
			moveMethod = MoveDirect;
			break;
		}
	}

	private bool CheckPathCalculationState(Path p)
	{
		return true;
	}

	private string GetPathCalculationError()
	{
		return movementType switch
		{
			MovementType.GDGraph => "MC: Cannot calculate gd point graph path for [" + assignedMovableObject.MovableObjectId + "]", 
			MovementType.Recast => "MC: Cannot calculate recast graph path for [" + assignedMovableObject.MovableObjectId + "]", 
			MovementType.WorldZone => $"MC: Cannot calculate world zone path for [{assignedMovableObject.MovableObjectId}] in graph mask: [{graphMask}]", 
			_ => $"MC: Wrong path calculation for type [{movementType}]", 
		};
	}

	private void OnDestinationReachSucceed()
	{
		SnapToExactDestinationIfNeeded();
		useVectorPathAsSource = false;
		status = Status.None;
		completionState = CompletionState.Success;
		onFinishAction?.Invoke();
		onFinishPath?.Invoke();
		if (status == Status.None)
		{
			onFinishPath = null;
		}
		MainGame.Instance.movementSystem.RemoveMovingObject(this);
		if (movementType == MovementType.Recast || movementType == MovementType.GDGraph)
		{
			Debug.Log($"MC: [{assignedMovableObject.MovableObjectId}] reached destination: [{currentPathTarget}], gameScene: [{endWorldId}]]");
		}
		else
		{
			Debug.Log($"MC: [{assignedMovableObject.MovableObjectId}] reached destination: {endPos}, gameScene: [{currentWorldId}]");
		}
		if (destinationType == DestinationType.DockPoint)
		{
			Vector2 vector = endDockPointData.Direction.ConvertToVector2XZ();
			assignedMovableObject.MovableDirection = vector;
			onDirectionChange?.Invoke(vector);
		}
		assignedMovableObject.OnPathComplete(this);
		GlobalEventsSystem.FireTrigger(GlobalEventsSystem.Event.Type.WgoGoToFinished, assignedMovableObject.MovableObjectId);
	}

	private void NotifyPathLengthReady(float length)
	{
		Action<float> action = onPathLengthReady;
		onPathLengthReady = null;
		action?.Invoke(length);
	}

	private void OnDestinationReachFailed()
	{
		useVectorPathAsSource = false;
		status = Status.None;
		completionState = CompletionState.Fail;
		onPathLengthReady = null;
		onFinishAction?.Invoke();
		onFinishPath?.Invoke();
		if (status == Status.None)
		{
			onFinishPath = null;
		}
		MainGame.Instance.movementSystem.RemoveMovingObject(this);
		Debug.LogError($"MC: [{assignedMovableObject.MovableObjectId}] cannot reach destination: [{currentPathTarget}] in graph mask: [{graphMask}]");
		assignedMovableObject.OnPathComplete(this);
	}

	private void CheckDirectionChanged(Vector2 prevDir, Vector2 curDir)
	{
		if (!Vector2.Dot(prevDir.normalized, curDir.normalized).EqualsTo(1f, 0.1f))
		{
			onDirectionChange?.Invoke(curDir);
		}
	}

	private bool IsReachable(Vector3 startPos, Vector3 endPos, int graphIndex)
	{
		NavGraph obj = AstarPath.active.data.graphs[graphIndex];
		NNInfo nearest = obj.GetNearest(startPos);
		NNInfo nearest2 = obj.GetNearest(endPos);
		return PathUtilities.IsPathPossible(nearest.node, nearest2.node);
	}
}
