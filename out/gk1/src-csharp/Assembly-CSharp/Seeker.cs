using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

[AddComponentMenu("Pathfinding/Seeker")]
[HelpURL("http://arongranberg.com/astar/docs/class_seeker.php")]
public class Seeker : MonoBehaviour, ISerializationCallbackReceiver
{
	public enum ModifierPass
	{
		PreProcess = 0,
		PostProcess = 2
	}

	public bool drawGizmos = true;

	public bool detailedGizmos;

	public StartEndModifier startEndModifier = new StartEndModifier();

	[HideInInspector]
	public int traversableTags = -1;

	[HideInInspector]
	[SerializeField]
	[FormerlySerializedAs("traversableTags")]
	protected TagMask traversableTagsCompatibility = new TagMask(-1, -1);

	[HideInInspector]
	public int[] tagPenalties = new int[32];

	public OnPathDelegate pathCallback;

	public OnPathDelegate preProcessPath;

	public OnPathDelegate postProcessPath;

	[NonSerialized]
	private List<Vector3> lastCompletedVectorPath;

	[NonSerialized]
	private List<GraphNode> lastCompletedNodePath;

	[NonSerialized]
	protected Path path;

	[NonSerialized]
	private Path prevPath;

	private readonly OnPathDelegate onPathDelegate;

	private readonly OnPathDelegate onPartialPathDelegate;

	private OnPathDelegate tmpPathCallback;

	protected uint lastPathID;

	private readonly List<IPathModifier> modifiers = new List<IPathModifier>();

	public Seeker()
	{
		onPathDelegate = OnPathComplete;
		onPartialPathDelegate = OnPartialPathComplete;
	}

	private void Awake()
	{
		startEndModifier.Awake(this);
	}

	public Path GetCurrentPath()
	{
		return path;
	}

	public void OnDestroy()
	{
		ReleaseClaimedPath();
		startEndModifier.OnDestroy(this);
	}

	public void ReleaseClaimedPath()
	{
		if (prevPath != null)
		{
			prevPath.Release(this, silent: true);
			prevPath = null;
		}
	}

	public void RegisterModifier(IPathModifier mod)
	{
		modifiers.Add(mod);
		modifiers.Sort((IPathModifier a, IPathModifier b) => a.Order.CompareTo(b.Order));
	}

	public void DeregisterModifier(IPathModifier mod)
	{
		modifiers.Remove(mod);
	}

	public void PostProcess(Path p)
	{
		RunModifiers(ModifierPass.PostProcess, p);
	}

	public void RunModifiers(ModifierPass pass, Path p)
	{
		if (pass == ModifierPass.PreProcess && preProcessPath != null)
		{
			preProcessPath(p);
		}
		else if (pass == ModifierPass.PostProcess && postProcessPath != null)
		{
			postProcessPath(p);
		}
		for (int i = 0; i < modifiers.Count; i++)
		{
			MonoModifier monoModifier = modifiers[i] as MonoModifier;
			if (!(monoModifier != null) || monoModifier.enabled)
			{
				switch (pass)
				{
				case ModifierPass.PreProcess:
					modifiers[i].PreProcess(p);
					break;
				case ModifierPass.PostProcess:
					modifiers[i].Apply(p);
					break;
				}
			}
		}
	}

	public bool IsDone()
	{
		if (path != null)
		{
			return path.GetState() >= PathState.Returned;
		}
		return true;
	}

	private void OnPathComplete(Path p)
	{
		OnPathComplete(p, runModifiers: true, sendCallbacks: true);
	}

	private void OnPathComplete(Path p, bool runModifiers, bool sendCallbacks)
	{
		if ((p != null && p != path && sendCallbacks) || this == null || p == null || p != path)
		{
			return;
		}
		if (!path.error && runModifiers)
		{
			RunModifiers(ModifierPass.PostProcess, path);
		}
		if (sendCallbacks)
		{
			p.Claim(this);
			lastCompletedNodePath = p.path;
			lastCompletedVectorPath = p.vectorPath;
			if (tmpPathCallback != null)
			{
				tmpPathCallback(p);
			}
			if (pathCallback != null)
			{
				pathCallback(p);
			}
			if (prevPath != null)
			{
				prevPath.Release(this, silent: true);
			}
			prevPath = p;
			if (!drawGizmos)
			{
				ReleaseClaimedPath();
			}
		}
	}

	private void OnPartialPathComplete(Path p)
	{
		OnPathComplete(p, runModifiers: true, sendCallbacks: false);
	}

	private void OnMultiPathComplete(Path p)
	{
		OnPathComplete(p, runModifiers: false, sendCallbacks: true);
	}

	public ABPath GetNewPath(Vector3 start, Vector3 end)
	{
		return ABPath.Construct(start, end);
	}

	public Path StartPath(Vector3 start, Vector3 end)
	{
		return StartPath(start, end, null, -1);
	}

	public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback)
	{
		return StartPath(start, end, callback, -1);
	}

	public Path StartPath(Vector3 start, Vector3 end, OnPathDelegate callback, int graphMask)
	{
		return StartPath(GetNewPath(start, end), callback, graphMask);
	}

	public Path StartPath(Path p, OnPathDelegate callback = null, int graphMask = -1)
	{
		if (p is MultiTargetPath multiTargetPath)
		{
			OnPathDelegate[] array = new OnPathDelegate[multiTargetPath.targetPoints.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = onPartialPathDelegate;
			}
			multiTargetPath.callbacks = array;
			p.callback = (OnPathDelegate)Delegate.Combine(p.callback, new OnPathDelegate(OnMultiPathComplete));
		}
		else
		{
			p.callback = (OnPathDelegate)Delegate.Combine(p.callback, onPathDelegate);
		}
		p.enabledTags = traversableTags;
		p.tagPenalties = tagPenalties;
		p.nnConstraint.graphMask = graphMask;
		StartPathInternal(p, callback);
		return p;
	}

	private void StartPathInternal(Path p, OnPathDelegate callback)
	{
		if (path != null && path.GetState() <= PathState.Processing && lastPathID == path.pathID)
		{
			path.Error();
			path.LogError("Canceled path because a new one was requested.\nThis happens when a new path is requested from the seeker when one was already being calculated.\nFor example if a unit got a new order, you might request a new path directly instead of waiting for the now invalid path to be calculated. Which is probably what you want.\nIf you are getting this a lot, you might want to consider how you are scheduling path requests.");
		}
		path = p;
		tmpPathCallback = callback;
		lastPathID = path.pathID;
		RunModifiers(ModifierPass.PreProcess, path);
		AstarPath.StartPath(path);
	}

	public MultiTargetPath StartMultiTargetPath(Vector3 start, Vector3[] endPoints, bool pathsForAll, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = MultiTargetPath.Construct(start, endPoints, null);
		multiTargetPath.pathsForAll = pathsForAll;
		StartPath(multiTargetPath, callback, graphMask);
		return multiTargetPath;
	}

	public MultiTargetPath StartMultiTargetPath(Vector3[] startPoints, Vector3 end, bool pathsForAll, OnPathDelegate callback = null, int graphMask = -1)
	{
		MultiTargetPath multiTargetPath = MultiTargetPath.Construct(startPoints, end, null);
		multiTargetPath.pathsForAll = pathsForAll;
		StartPath(multiTargetPath, callback, graphMask);
		return multiTargetPath;
	}

	[Obsolete("You can use StartPath instead of this method now. It will behave identically.")]
	public MultiTargetPath StartMultiTargetPath(MultiTargetPath p, OnPathDelegate callback = null, int graphMask = -1)
	{
		StartPath(p, callback, graphMask);
		return p;
	}

	public void OnDrawGizmos()
	{
		if (lastCompletedNodePath == null || !drawGizmos)
		{
			return;
		}
		if (detailedGizmos)
		{
			Gizmos.color = new Color(0.7f, 0.5f, 0.1f, 0.5f);
			if (lastCompletedNodePath != null)
			{
				for (int i = 0; i < lastCompletedNodePath.Count - 1; i++)
				{
					Gizmos.DrawLine((Vector3)lastCompletedNodePath[i].position, (Vector3)lastCompletedNodePath[i + 1].position);
				}
			}
		}
		Gizmos.color = new Color(0f, 1f, 0f, 1f);
		if (lastCompletedVectorPath != null)
		{
			for (int j = 0; j < lastCompletedVectorPath.Count - 1; j++)
			{
				Gizmos.DrawLine(lastCompletedVectorPath[j], lastCompletedVectorPath[j + 1]);
			}
		}
	}

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		if (traversableTagsCompatibility != null && traversableTagsCompatibility.tagsChange != -1)
		{
			traversableTags = traversableTagsCompatibility.tagsChange;
			traversableTagsCompatibility = new TagMask(-1, -1);
		}
	}
}
