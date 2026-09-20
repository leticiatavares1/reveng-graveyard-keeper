using System;
using System.Collections.Generic;
using System.Reflection;
using LinqTools;
using NodeCanvas.Framework;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas;

public abstract class FlowNode : Node, ISerializationCallbackReceiver
{
	[AttributeUsage(AttributeTargets.Class)]
	public class ContextDefinedInputsAttribute : Attribute
	{
		public Type[] types;

		public ContextDefinedInputsAttribute(params Type[] types)
		{
			this.types = types;
		}
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class ContextDefinedOutputsAttribute : Attribute
	{
		public Type[] types;

		public ContextDefinedOutputsAttribute(params Type[] types)
		{
			this.types = types;
		}
	}

	[SerializeField]
	private Dictionary<string, object> _inputPortValues;

	protected Dictionary<string, Port> inputPorts = new Dictionary<string, Port>(StringComparer.Ordinal);

	protected Dictionary<string, Port> outputPorts = new Dictionary<string, Port>(StringComparer.Ordinal);

	public sealed override int maxInConnections => -1;

	public sealed override int maxOutConnections => -1;

	public sealed override bool allowAsPrime => false;

	public sealed override Type outConnectionType => typeof(BinderConnection);

	public sealed override Alignment2x2 commentsAlignment => Alignment2x2.Bottom;

	public override Alignment2x2 iconAlignment => Alignment2x2.Left;

	public FlowGraph flowGraph => (FlowGraph)base.graph;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		if (_inputPortValues == null)
		{
			_inputPortValues = new Dictionary<string, object>();
		}
		foreach (ValueInput item in inputPorts.Values.OfType<ValueInput>())
		{
			if (!item.isConnected)
			{
				_inputPortValues[item.ID] = item.serializedValue;
			}
		}
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
	}

	public sealed override void OnValidate(Graph flowGraph)
	{
		GatherPorts();
	}

	public sealed override void OnParentConnected(int i)
	{
		if (i < base.inConnections.Count && base.inConnections[i] is BinderConnection binderConnection)
		{
			TryHandleWildPortConnection(binderConnection.targetPort, binderConnection.sourcePort);
			OnPortConnected(binderConnection.targetPort, binderConnection.sourcePort);
		}
	}

	public sealed override void OnChildConnected(int i)
	{
		if (i < base.outConnections.Count && base.outConnections[i] is BinderConnection binderConnection)
		{
			TryHandleWildPortConnection(binderConnection.sourcePort, binderConnection.targetPort);
			OnPortConnected(binderConnection.sourcePort, binderConnection.targetPort);
		}
	}

	public sealed override void OnParentDisconnected(int i)
	{
		if (i < base.inConnections.Count && base.inConnections[i] is BinderConnection binderConnection)
		{
			OnPortDisconnected(binderConnection.targetPort, binderConnection.sourcePort);
		}
	}

	public sealed override void OnChildDisconnected(int i)
	{
		if (i < base.outConnections.Count && base.outConnections[i] is BinderConnection binderConnection)
		{
			OnPortDisconnected(binderConnection.sourcePort, binderConnection.targetPort);
		}
	}

	public virtual void OnPortConnected(Port port, Port otherPort)
	{
	}

	public virtual void OnPortDisconnected(Port port, Port otherPort)
	{
	}

	public void BindPorts()
	{
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			(base.outConnections[i] as BinderConnection).Bind();
		}
	}

	public void UnBindPorts()
	{
		for (int i = 0; i < base.outConnections.Count; i++)
		{
			(base.outConnections[i] as BinderConnection).UnBind();
		}
	}

	public Port GetInputPort(string ID)
	{
		Port value = null;
		if (!inputPorts.TryGetValue(ID, out value))
		{
			value = inputPorts.Values.FirstOrDefault((Port p) => p.name.SplitCamelCase() == ID);
		}
		return value;
	}

	public Port GetOutputPort(string ID)
	{
		Port value = null;
		if (!outputPorts.TryGetValue(ID, out value))
		{
			value = outputPorts.Values.FirstOrDefault((Port p) => p.name.SplitCamelCase() == ID);
		}
		return value;
	}

	public List<Port> GetOutputPorts()
	{
		return outputPorts.Values.ToList();
	}

	public BinderConnection GetInputConnectionForPortID(string ID)
	{
		return base.inConnections.OfType<BinderConnection>().FirstOrDefault((BinderConnection c) => c.targetPortID == ID);
	}

	public BinderConnection GetOutputConnectionForPortID(string ID)
	{
		return base.outConnections.OfType<BinderConnection>().FirstOrDefault((BinderConnection c) => c.sourcePortID == ID);
	}

	public Port GetFirstInputOfType(Type type)
	{
		return inputPorts.Values.OrderBy((Port p) => (!(p is FlowInput)) ? 1 : 0).FirstOrDefault((Port p) => p.type.RTIsAssignableFrom(type));
	}

	public Port GetFirstOutputOfType(Type type)
	{
		return outputPorts.Values.OrderBy((Port p) => (!(p is FlowInput)) ? 1 : 0).FirstOrDefault((Port p) => type.RTIsAssignableFrom(p.type));
	}

	public void AssignSelfInstancePort()
	{
		ValueInput valueInput = inputPorts.Values.OfType<ValueInput>().FirstOrDefault();
		if (valueInput != null && !valueInput.isConnected && valueInput.isDefaultValue)
		{
			UnityEngine.Object agentComponent = flowGraph.GetAgentComponent(valueInput.type);
			if (agentComponent != null)
			{
				valueInput.serializedValue = agentComponent;
			}
		}
	}

	public void GatherPorts()
	{
		inputPorts.Clear();
		outputPorts.Clear();
		RegisterPorts();
		DeserializeInputPortValues();
		ValidateConnections();
	}

	protected virtual void RegisterPorts()
	{
		TryAddReflectionBasedRegistrationForObject(this);
	}

	private void DeserializeInputPortValues()
	{
		if (_inputPortValues == null)
		{
			return;
		}
		foreach (KeyValuePair<string, object> pair in _inputPortValues)
		{
			Port value = null;
			if (!inputPorts.TryGetValue(pair.Key, out value))
			{
				value = inputPorts.Values.FirstOrDefault((Port p) => p.name.SplitCamelCase() == pair.Key);
			}
			if (value is ValueInput && pair.Value != null && value.type.RTIsAssignableFrom(pair.Value.GetType()))
			{
				(value as ValueInput).serializedValue = pair.Value;
			}
		}
	}

	private void ValidateConnections()
	{
		Connection[] array = base.outConnections.ToArray();
		foreach (Connection connection in array)
		{
			if (connection is BinderConnection)
			{
				(connection as BinderConnection).GatherAndValidateSourcePort();
			}
		}
		array = base.inConnections.ToArray();
		foreach (Connection connection2 in array)
		{
			if (connection2 is BinderConnection)
			{
				(connection2 as BinderConnection).GatherAndValidateTargetPort();
			}
		}
	}

	public FlowInput AddFlowInput(string name, string ID, FlowHandler pointer)
	{
		return AddFlowInput(name, pointer, ID);
	}

	public FlowInput AddFlowInput(string name, FlowHandler pointer, string ID = "")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = " ";
		}
		if (string.IsNullOrEmpty(ID))
		{
			ID = name;
		}
		Port port2 = (inputPorts[ID] = new FlowInput(this, name, ID, pointer));
		return (FlowInput)port2;
	}

	public FlowOutput AddFlowOutput(string name, string ID = "")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = " ";
		}
		if (string.IsNullOrEmpty(ID))
		{
			ID = name;
		}
		Port port2 = (outputPorts[ID] = new FlowOutput(this, name, ID));
		return (FlowOutput)port2;
	}

	public ValueInput<T> AddValueInput<T>(string name, string ID = "")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = " ";
		}
		if (string.IsNullOrEmpty(ID))
		{
			ID = name;
		}
		Port port2 = (inputPorts[ID] = new ValueInput<T>(this, name, ID));
		return (ValueInput<T>)port2;
	}

	public ValueInput<T> AddVerticalValueInput<T>(string name, string ID = "")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = " ";
		}
		if (string.IsNullOrEmpty(ID))
		{
			ID = name;
		}
		name += "\u00a0";
		Port port2 = (inputPorts[ID] = new ValueInput<T>(this, name, ID));
		return (ValueInput<T>)port2;
	}

	public ValueOutput<T> AddValueOutput<T>(string name, string ID, ValueHandler<T> getter)
	{
		return AddValueOutput(name, getter, ID);
	}

	public ValueOutput<T> AddValueOutput<T>(string name, ValueHandler<T> getter, string ID = "")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = " ";
		}
		if (string.IsNullOrEmpty(ID))
		{
			ID = name;
		}
		Port port2 = (outputPorts[ID] = new ValueOutput<T>(this, name, ID, getter));
		return (ValueOutput<T>)port2;
	}

	public ValueInput AddValueInput(string name, Type type, string ID = "")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = " ";
		}
		if (string.IsNullOrEmpty(ID))
		{
			ID = name;
		}
		Port port2 = (inputPorts[ID] = ValueInput.CreateInstance(type, this, name, ID));
		return (ValueInput)port2;
	}

	public ValueOutput AddValueOutput(string name, string ID, Type type, ValueHandlerObject getter)
	{
		return AddValueOutput(name, type, getter, ID);
	}

	public ValueOutput AddValueOutput(string name, Type type, ValueHandlerObject getter, string ID = "")
	{
		if (string.IsNullOrEmpty(name))
		{
			name = " ";
		}
		if (string.IsNullOrEmpty(ID))
		{
			ID = name;
		}
		Port port2 = (outputPorts[ID] = ValueOutput.CreateInstance(type, this, name, ID, getter));
		return (ValueOutput)port2;
	}

	private void TryAddReflectionBasedRegistrationForObject(object instance)
	{
		MethodInfo[] methods = instance.GetType().GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		foreach (MethodInfo method in methods)
		{
			TryAddMethodFlowInput(method, instance);
		}
		PropertyInfo[] properties = instance.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo prop in properties)
		{
			TryAddPropertyValueOutput(prop, instance);
		}
		FieldInfo[] fields = instance.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo field in fields)
		{
			TryAddFieldDelegateFlowOutput(field, instance);
			TryAddFieldDelegateValueInput(field, instance);
		}
	}

	public FlowInput TryAddMethodFlowInput(MethodInfo method, object instance)
	{
		ParameterInfo[] parameters = method.GetParameters();
		if (method.ReturnType == typeof(void) && parameters.Length == 1 && parameters[0].ParameterType == typeof(Flow))
		{
			NameAttribute nameAttribute = method.RTGetAttribute<NameAttribute>(inherited: false);
			string text = ((nameAttribute != null) ? nameAttribute.name : method.Name);
			FlowHandler pointer = method.RTCreateDelegate<FlowHandler>(instance);
			return AddFlowInput(text, pointer);
		}
		return null;
	}

	public FlowOutput TryAddFieldDelegateFlowOutput(FieldInfo field, object instance)
	{
		if (field.FieldType == typeof(FlowHandler))
		{
			NameAttribute nameAttribute = field.RTGetAttribute<NameAttribute>(inherited: false);
			string text = ((nameAttribute != null) ? nameAttribute.name : field.Name);
			FlowOutput flowOutput = AddFlowOutput(text);
			field.SetValue(instance, new FlowHandler(flowOutput.Call));
			return flowOutput;
		}
		return null;
	}

	public ValueInput TryAddFieldDelegateValueInput(FieldInfo field, object instance)
	{
		if (typeof(Delegate).RTIsAssignableFrom(field.FieldType))
		{
			MethodInfo method = field.FieldType.GetMethod("Invoke");
			ParameterInfo[] parameters = method.GetParameters();
			if (method.ReturnType != typeof(void) && parameters.Length == 0)
			{
				NameAttribute nameAttribute = field.RTGetAttribute<NameAttribute>(inherited: false);
				string text = ((nameAttribute != null) ? nameAttribute.name : field.Name);
				Type returnType = method.ReturnType;
				ValueInput valueInput = (ValueInput)Activator.CreateInstance(typeof(ValueInput<>).RTMakeGenericType(returnType), instance, text, text);
				Type type = typeof(ValueHandler<>).RTMakeGenericType(returnType);
				Delegate value = valueInput.GetType().GetMethod("get_value").RTCreateDelegate(type, valueInput);
				field.SetValue(instance, value);
				inputPorts[text] = valueInput;
				return valueInput;
			}
		}
		return null;
	}

	public ValueOutput TryAddPropertyValueOutput(PropertyInfo prop, object instance)
	{
		if (!prop.CanRead)
		{
			return null;
		}
		NameAttribute nameAttribute = prop.RTGetAttribute<NameAttribute>(inherited: false);
		string text = ((nameAttribute != null) ? nameAttribute.name : prop.Name);
		Type type = typeof(ValueHandler<>).RTMakeGenericType(prop.PropertyType);
		Delegate @delegate = prop.RTGetGetMethod().RTCreateDelegate(type, instance);
		ValueOutput valueOutput = (ValueOutput)Activator.CreateInstance(typeof(ValueOutput<>).RTMakeGenericType(prop.PropertyType), this, text, text, @delegate);
		Port port2 = (outputPorts[text] = valueOutput);
		return (ValueOutput)port2;
	}

	public FlowNode ReplaceWith(Type t)
	{
		if (!(base.graph.AddNode(t, base.nodePosition) is FlowNode flowNode))
		{
			return null;
		}
		Connection[] array = base.inConnections.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetTarget(flowNode);
		}
		array = base.outConnections.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetSource(flowNode);
		}
		if (_inputPortValues != null)
		{
			flowNode._inputPortValues = _inputPortValues.ToDictionary((KeyValuePair<string, object> k) => k.Key, (KeyValuePair<string, object> v) => v.Value);
		}
		flowNode.GatherPorts();
		base.graph.RemoveNode(this);
		return flowNode;
	}

	public virtual Type GetNodeWildDefinitionType()
	{
		return GetType().GetFirstGenericParameterConstraintType();
	}

	private void TryHandleWildPortConnection(Port port, Port otherPort)
	{
		Type nodeWildDefinitionType = GetNodeWildDefinitionType();
		Type type = GetType();
		Type type2 = TryGetNewGenericTypeForWild(nodeWildDefinitionType, port, otherPort, type, null);
		if (type2 != null)
		{
			ReplaceWith(type2);
		}
	}

	public static Type TryGetNewGenericTypeForWild(Type wildType, Port port, Port otherPort, Type content, Type context)
	{
		if (wildType == null || !content.IsGenericType)
		{
			return null;
		}
		Type[] genericArguments = content.GetGenericArguments();
		Type type = genericArguments.FirstOrDefault();
		if (type != wildType && type.IsGenericType)
		{
			return TryGetNewGenericTypeForWild(wildType, port, otherPort, type, content);
		}
		Type enumerableElementType;
		int num;
		Type type2;
		if (genericArguments.Length == 1 && type == wildType)
		{
			enumerableElementType = otherPort.type.GetEnumerableElementType();
			Type enumerableElementType2 = port.type.GetEnumerableElementType();
			if (enumerableElementType != null)
			{
				num = ((enumerableElementType2 != null) ? 1 : 0);
				if (num != 0)
				{
					type2 = enumerableElementType2;
					goto IL_0089;
				}
			}
			else
			{
				num = 0;
			}
			type2 = port.type;
			goto IL_0089;
		}
		goto IL_010d;
		IL_0089:
		Type type3 = type2;
		Type type4 = ((num != 0) ? enumerableElementType : otherPort.type);
		if (type3 == wildType && type4 != type3)
		{
			content = content.GetGenericTypeDefinition();
			type = content.GetGenericArguments().First();
			if (type4.IsAllowedByGenericArgument(type))
			{
				Type type5 = content.MakeGenericType(type4);
				if (context != null && context.IsGenericType)
				{
					type5 = context.GetGenericTypeDefinition().MakeGenericType(type5);
				}
				return type5;
			}
		}
		goto IL_010d;
		IL_010d:
		return null;
	}

	public static MethodInfo TryGetNewGenericMethodForWild(Type wildType, Port port, Port otherPort, MethodInfo content)
	{
		if (wildType == null || !content.IsGenericMethod)
		{
			return null;
		}
		Type[] genericArguments = content.GetGenericArguments();
		Type type = genericArguments.FirstOrDefault();
		Type enumerableElementType;
		int num;
		Type type2;
		if (genericArguments.Length == 1 && type == wildType)
		{
			enumerableElementType = otherPort.type.GetEnumerableElementType();
			Type enumerableElementType2 = port.type.GetEnumerableElementType();
			if (enumerableElementType != null)
			{
				num = ((enumerableElementType2 != null) ? 1 : 0);
				if (num != 0)
				{
					type2 = enumerableElementType2;
					goto IL_006b;
				}
			}
			else
			{
				num = 0;
			}
			type2 = port.type;
			goto IL_006b;
		}
		goto IL_00bc;
		IL_00bc:
		return null;
		IL_006b:
		Type type3 = type2;
		Type type4 = ((num != 0) ? enumerableElementType : otherPort.type);
		if (type3 == wildType && type4 != type3)
		{
			content = content.GetGenericMethodDefinition();
			type = content.GetGenericArguments().First();
			if (type4.IsAllowedByGenericArgument(type))
			{
				return content.MakeGenericMethod(type4);
			}
		}
		goto IL_00bc;
	}

	public void Call(FlowOutput port, Flow f)
	{
		port.Call(f);
	}
}
