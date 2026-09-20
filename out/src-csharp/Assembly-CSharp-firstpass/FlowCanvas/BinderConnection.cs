using System;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Serialization.FullSerializer;
using ParadoxNotion.Services;
using UnityEngine;

namespace FlowCanvas;

public class BinderConnection<T> : BinderConnection
{
	public override void Bind()
	{
		if (base.isActive)
		{
			DoNormalBinding(base.sourcePort, base.targetPort);
		}
	}

	public override void UnBind()
	{
		if (base.targetPort is ValueInput)
		{
			(base.targetPort as ValueInput).UnBind();
		}
	}

	private void DoNormalBinding(Port source, Port target)
	{
		(target as ValueInput<T>).BindTo((ValueOutput)source);
	}
}
public class BinderConnection : Connection
{
	[SerializeField]
	[fsProperty("_sourcePortName")]
	private string _sourcePortID;

	[fsProperty("_targetPortName")]
	[SerializeField]
	private string _targetPortID;

	[NonSerialized]
	private Port _sourcePort;

	[NonSerialized]
	private Port _targetPort;

	public string sourcePortID
	{
		get
		{
			if (sourcePort == null)
			{
				return _sourcePortID;
			}
			return sourcePort.ID;
		}
		private set
		{
			_sourcePortID = value;
		}
	}

	public string targetPortID
	{
		get
		{
			if (targetPort == null)
			{
				return _targetPortID;
			}
			return targetPort.ID;
		}
		private set
		{
			_targetPortID = value;
		}
	}

	public Port sourcePort
	{
		get
		{
			if (_sourcePort == null && base.sourceNode is FlowNode)
			{
				_sourcePort = (base.sourceNode as FlowNode).GetOutputPort(_sourcePortID);
			}
			return _sourcePort;
		}
	}

	public Port targetPort
	{
		get
		{
			if (_targetPort == null && base.targetNode is FlowNode)
			{
				_targetPort = (base.targetNode as FlowNode).GetInputPort(_targetPortID);
			}
			return _targetPort;
		}
	}

	public Type bindingType
	{
		get
		{
			if (!GetType().RTIsGenericType())
			{
				return typeof(Flow);
			}
			return GetType().RTGetGenericArguments()[0];
		}
	}

	public void GatherAndValidateSourcePort()
	{
		_sourcePort = null;
		if (sourcePort != null && TypeConverter.HasConvertion(sourcePort.type, bindingType))
		{
			sourcePortID = sourcePort.ID;
			sourcePort.connections++;
		}
		else
		{
			base.graph.RemoveConnection(this, recordUndo: false);
		}
	}

	public void GatherAndValidateTargetPort()
	{
		_targetPort = null;
		if (targetPort != null)
		{
			if (targetPort.type == bindingType)
			{
				targetPortID = targetPort.ID;
				targetPort.connections++;
				return;
			}
			if (targetPort is ValueInput && sourcePort is ValueOutput && TypeConverter.HasConvertion(sourcePort.type, targetPort.type))
			{
				ReplaceWith(typeof(BinderConnection<>).MakeGenericType(bindingType));
				targetPortID = targetPort.ID;
				targetPort.connections++;
				return;
			}
		}
		base.graph.RemoveConnection(this, recordUndo: false);
	}

	public static BinderConnection Create(Port source, Port target)
	{
		if (source == null || target == null)
		{
			return null;
		}
		if (source == target)
		{
			return null;
		}
		if (!source.CanAcceptConnections())
		{
			ParadoxNotion.Services.Logger.LogWarning("Source port can accept no more connections.", "Editor", source.parent);
			return null;
		}
		if (!target.CanAcceptConnections())
		{
			ParadoxNotion.Services.Logger.LogWarning("Target port can accept no more connections.", "Editor", source.parent);
			return null;
		}
		if (source.parent == target.parent)
		{
			ParadoxNotion.Services.Logger.LogWarning("Can't connect ports on the same parent node.", "Editor", source.parent);
			return null;
		}
		if (source is FlowOutput && !(target is FlowInput))
		{
			ParadoxNotion.Services.Logger.LogWarning("Flow ports can only be connected to other Flow ports.", "Editor", source.parent);
			return null;
		}
		if ((source is FlowInput && target is FlowInput) || (source is ValueInput && target is ValueInput))
		{
			ParadoxNotion.Services.Logger.LogWarning("Can't connect input to input.", "Editor", source.parent);
			return null;
		}
		if ((source is FlowOutput && target is FlowOutput) || (source is ValueOutput && target is ValueOutput))
		{
			ParadoxNotion.Services.Logger.LogWarning("Can't connect output to output.", "Editor", source.parent);
			return null;
		}
		if (!TypeConverter.HasConvertion(source.type, target.type))
		{
			ParadoxNotion.Services.Logger.LogWarning($"Can't connect ports. Type '{target.type.FriendlyName()}' is not assignable from Type '{source.type.FriendlyName()}' and there exists no automatic conversion for those types.", "Editor", source.parent);
			return null;
		}
		BinderConnection binderConnection = null;
		if (source is FlowOutput && target is FlowInput)
		{
			binderConnection = new BinderConnection();
		}
		if (source is ValueOutput && target is ValueInput)
		{
			binderConnection = (BinderConnection)Activator.CreateInstance(typeof(BinderConnection<>).RTMakeGenericType(target.type));
		}
		binderConnection?.OnCreate(source, target);
		return binderConnection;
	}

	public virtual void Bind()
	{
		if (base.isActive && sourcePort is FlowOutput && targetPort is FlowInput)
		{
			(sourcePort as FlowOutput).BindTo((FlowInput)targetPort);
		}
	}

	public virtual void UnBind()
	{
		if (sourcePort is FlowOutput)
		{
			(sourcePort as FlowOutput).UnBind();
		}
	}

	private void OnCreate(Port source, Port target)
	{
		base.sourceNode = source.parent;
		base.targetNode = target.parent;
		sourcePortID = source.ID;
		targetPortID = target.ID;
		base.sourceNode.outConnections.Add(this);
		base.targetNode.inConnections.Add(this);
		base.sourceNode.OnChildConnected(base.sourceNode.outConnections.Count - 1);
		base.targetNode.OnParentConnected(base.targetNode.inConnections.Count - 1);
		source.connections++;
		target.connections++;
		if (Application.isPlaying)
		{
			Bind();
		}
	}

	public override void OnDestroy()
	{
		if (sourcePort != null)
		{
			sourcePort.connections--;
		}
		if (targetPort != null)
		{
			targetPort.connections--;
		}
		if (Application.isPlaying)
		{
			UnBind();
		}
	}

	public BinderConnection ReplaceWith(Type t)
	{
		Port source = sourcePort;
		Port target = targetPort;
		base.graph.RemoveConnection(this);
		return Create(source, target);
	}
}
