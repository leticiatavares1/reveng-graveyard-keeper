using System;
using NodeCanvas.Framework;
using UnityEngine;

namespace NodeCanvas.BehaviourTrees;

[GraphInfo(packageName = "NodeCanvas", docsURL = "http://nodecanvas.paradoxnotion.com/documentation/", resourcesURL = "http://nodecanvas.paradoxnotion.com/downloads/", forumsURL = "http://nodecanvas.paradoxnotion.com/forums-page/")]
public class BehaviourTree : Graph
{
	[Serializable]
	private struct DerivedSerializationData
	{
		public bool repeat;

		public float updateInterval;
	}

	[SerializeField]
	public bool repeat = true;

	[SerializeField]
	public float updateInterval;

	private float intervalCounter;

	private Status _rootStatus = Status.Resting;

	public Status rootStatus
	{
		get
		{
			return _rootStatus;
		}
		private set
		{
			if (_rootStatus != value)
			{
				_rootStatus = value;
				if (BehaviourTree.onRootStatusChanged != null)
				{
					BehaviourTree.onRootStatusChanged(this, value);
				}
			}
		}
	}

	public override Type baseNodeType => typeof(BTNode);

	public override bool requiresAgent => true;

	public override bool requiresPrimeNode => true;

	public override bool autoSort => true;

	public override bool useLocalBlackboard => false;

	public static event Action<BehaviourTree, Status> onRootStatusChanged;

	public override object OnDerivedDataSerialization()
	{
		DerivedSerializationData derivedSerializationData = default(DerivedSerializationData);
		derivedSerializationData.repeat = repeat;
		derivedSerializationData.updateInterval = updateInterval;
		return derivedSerializationData;
	}

	public override void OnDerivedDataDeserialization(object data)
	{
		if (data is DerivedSerializationData)
		{
			repeat = ((DerivedSerializationData)data).repeat;
			updateInterval = ((DerivedSerializationData)data).updateInterval;
		}
	}

	protected override void OnGraphStarted()
	{
		intervalCounter = updateInterval;
		rootStatus = base.primeNode.status;
	}

	protected override void OnGraphUpdate()
	{
		if (intervalCounter >= updateInterval)
		{
			intervalCounter = 0f;
			if (Tick(base.agent, base.blackboard) != Status.Running && !repeat)
			{
				Stop(rootStatus == Status.Success);
			}
		}
		if (updateInterval > 0f)
		{
			intervalCounter += Time.deltaTime;
		}
	}

	public Status Tick(Component agent, IBlackboard blackboard)
	{
		if (rootStatus != Status.Running)
		{
			base.primeNode.Reset();
		}
		rootStatus = base.primeNode.Execute(agent, blackboard);
		return rootStatus;
	}
}
