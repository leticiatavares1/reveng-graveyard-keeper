using System;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;

namespace FlowCanvas;

[SpoofAOT]
public abstract class Port
{
	[NonSerialized]
	public string additional_draw;

	[NonSerialized]
	public Rect additional_draw_rect;

	[NonSerialized]
	public Color additional_draw_color;

	public FlowNode parent { get; private set; }

	public string ID { get; private set; }

	public string name { get; private set; }

	public string displayName { get; private set; }

	public GUIContent displayContent { get; private set; }

	public Color displayColor { get; private set; }

	public Vector2 pos { get; set; }

	public float posOffsetY { get; set; }

	public int connections { get; set; }

	public bool isConnected => connections > 0;

	public abstract Type type { get; }

	public Port()
	{
	}

	public Port(FlowNode parent, string name, string ID)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = " ";
		}
		if (string.IsNullOrEmpty(ID))
		{
			ID = name;
		}
		this.parent = parent;
		this.name = name;
		this.ID = ID;
	}

	public bool CanAcceptConnections()
	{
		if (this is ValueOutput || (this is FlowOutput && !isConnected))
		{
			return true;
		}
		if (this is FlowInput || (this is ValueInput && !isConnected))
		{
			return true;
		}
		return false;
	}

	public bool IsFlowPort()
	{
		if (!(this is FlowInput))
		{
			return this is FlowOutput;
		}
		return true;
	}

	public bool IsValuePort()
	{
		if (!(this is ValueInput))
		{
			return this is ValueOutput;
		}
		return true;
	}

	public bool IsInputPort()
	{
		if (!(this is FlowInput))
		{
			return this is ValueInput;
		}
		return true;
	}

	public bool IsOutputPort()
	{
		if (!(this is FlowOutput))
		{
			return this is ValueOutput;
		}
		return true;
	}

	public bool IsWild()
	{
		return type == typeof(Wild);
	}

	public bool IsUnityObject()
	{
		return typeof(UnityEngine.Object).RTIsAssignableFrom(type);
	}

	public bool IsUnitySceneObject()
	{
		if (!typeof(Component).RTIsAssignableFrom(type))
		{
			return type == typeof(GameObject);
		}
		return true;
	}

	public bool IsDelegate()
	{
		return typeof(Delegate).RTIsAssignableFrom(type);
	}

	public bool IsEnumerableCollection()
	{
		return type.IsEnumerableCollection();
	}

	public bool IsVertical()
	{
		if (type == typeof(string))
		{
			return true;
		}
		if (displayName.Length > 0)
		{
			return displayName[displayName.Length - 1] == '\u00a0';
		}
		return false;
	}
}
