using System;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas.Nodes;

[DoNotList]
[Icon("", false, "GetRuntimeIconType")]
public class ReflectedFieldNodeWrapper : FlowNode
{
	public enum AccessMode
	{
		GetField,
		SetField
	}

	[SerializeField]
	private string fieldName;

	[SerializeField]
	private Type targetType;

	[SerializeField]
	private AccessMode accessMode;

	private FieldInfo _field;

	private BaseReflectedFieldNode reflectedFieldNode { get; set; }

	private FieldInfo field
	{
		get
		{
			if (_field != null)
			{
				return _field;
			}
			return _field = ((targetType != null) ? targetType.GetField(fieldName) : null);
		}
	}

	public override string name
	{
		get
		{
			if (field != null)
			{
				bool flag = accessMode == AccessMode.GetField;
				bool isStatic = field.IsStatic;
				if (field.IsConstant())
				{
					return $"{field.DeclaringType.FriendlyName()}.{field.Name}";
				}
				if (isStatic)
				{
					return string.Format("{0} {1}.{2}", flag ? "Get" : "Set", field.DeclaringType.FriendlyName(), field.Name);
				}
				return string.Format("{0} {1}", flag ? "Get" : "Set", field.Name);
			}
			return string.Format("* Missing '{0}.{1}' *", (targetType != null) ? targetType.Name : "null", fieldName);
		}
	}

	private Type GetRuntimeIconType()
	{
		if (!(field != null))
		{
			return null;
		}
		return field.DeclaringType;
	}

	public void SetField(FieldInfo newField, AccessMode mode, object instance = null)
	{
		if (newField == null)
		{
			return;
		}
		newField = newField.GetBaseDefinition();
		fieldName = newField.Name;
		targetType = newField.DeclaringType;
		accessMode = mode;
		GatherPorts();
		if (instance != null && !newField.IsStatic)
		{
			ValueInput valueInput = (ValueInput)GetFirstInputOfType(instance.GetType());
			if (valueInput != null)
			{
				valueInput.serializedValue = instance;
			}
		}
	}

	protected override void RegisterPorts()
	{
		if (!(field == null))
		{
			reflectedFieldNode = BaseReflectedFieldNode.GetFieldNode(field);
			if (reflectedFieldNode != null)
			{
				reflectedFieldNode.RegisterPorts(this, accessMode);
			}
		}
	}
}
