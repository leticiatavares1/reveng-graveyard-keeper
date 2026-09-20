using System;
using System.Collections.Generic;
using LazyBearTechnology;
using Sirenix.Serialization;
using UnityEngine;

[Serializable]
public class WsoData : ObjectLinkedToDefinition<WSODef>
{
	[SerializeField]
	private SGuid uniqueId = new SGuid();

	[SerializeField]
	private Vector3 position;

	[SerializeField]
	private Vector3 scale = Vector3.one;

	[SerializeField]
	private string worldId;

	[SerializeField]
	private bool isHidden;

	[SerializeField]
	private string customTag;

	[SerializeReference]
	private List<WsoComponentDataBase> componentData = new List<WsoComponentDataBase>();

	public SGuid UniqueId => uniqueId;

	public Vector3 Position
	{
		get
		{
			return position;
		}
		set
		{
			position = value;
		}
	}

	public Vector3 Scale
	{
		get
		{
			return scale;
		}
		set
		{
			scale = value;
		}
	}

	public string WorldId
	{
		get
		{
			return worldId;
		}
		set
		{
			worldId = value;
		}
	}

	public bool IsHidden
	{
		get
		{
			return isHidden;
		}
		set
		{
			if (isHidden != value)
			{
				isHidden = value;
				this.OnHiddenStateChanged?.Invoke(isHidden);
			}
		}
	}

	public string CustomTag
	{
		get
		{
			return customTag;
		}
		set
		{
			customTag = value;
		}
	}

	public IReadOnlyList<WsoComponentDataBase> ComponentData => componentData;

	public event Action<bool> OnHiddenStateChanged;

	public event Action OnRepairStateChanged;

	public WsoData CreateDataFromMe(Vector3 globalOffset, string gameSceneId, bool copySGuid)
	{
		WsoData wsoData = new WsoData(this, copySGuid ? UniqueId : null);
		wsoData.Position += globalOffset;
		wsoData.WorldId = gameSceneId;
		return wsoData;
	}

	public WsoData()
	{
		uniqueId = new SGuid();
	}

	public WsoData(WSODef wsoDef)
		: this(wsoDef, null)
	{
	}

	public WsoData(WSODef wsoDef, SGuid newUniqueId)
	{
		if (!SGuid.IsNullOrEmpty(newUniqueId))
		{
			uniqueId.SetGuid(newUniqueId);
		}
		if (wsoDef == null)
		{
			Debug.LogError("wsoDef is null, that mustn't be happened");
		}
		else
		{
			id = wsoDef.id;
		}
	}

	public WsoData(WSODef wsoDef, SGuid newUniqueId, Vector3 position, string worldId)
		: this(wsoDef, newUniqueId)
	{
		this.position = position;
		this.worldId = worldId;
	}

	public WsoData(WsoData source, SGuid newUniqueId = null)
		: base(source.id)
	{
		if (!SGuid.IsNullOrEmpty(newUniqueId))
		{
			uniqueId.SetGuid(newUniqueId);
		}
		else
		{
			uniqueId = new SGuid();
		}
		position = source.position;
		scale = source.scale;
		worldId = source.worldId;
		isHidden = source.isHidden;
		customTag = source.customTag;
		foreach (WsoComponentDataBase componentDatum in source.componentData)
		{
			if (SerializationUtility.CreateCopy(componentDatum) is WsoComponentDataBase item)
			{
				componentData.Add(item);
			}
		}
	}

	public T GetComponentData<T>() where T : WsoComponentDataBase
	{
		foreach (WsoComponentDataBase componentDatum in componentData)
		{
			if (componentDatum is T result)
			{
				return result;
			}
		}
		return null;
	}

	public List<T> GetAllComponentData<T>() where T : WsoComponentDataBase
	{
		List<T> list = new List<T>();
		foreach (WsoComponentDataBase componentDatum in componentData)
		{
			if (componentDatum is T item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void AddComponentData(WsoComponentDataBase data)
	{
		if (data != null)
		{
			componentData.Add(data);
		}
	}

	public bool RemoveComponentData(WsoComponentDataBase data)
	{
		return componentData.Remove(data);
	}

	public WsoRepairablePartData GetOrCreateRepairablePartData()
	{
		WsoRepairablePartData wsoRepairablePartData = GetComponentData<WsoRepairablePartData>();
		if (wsoRepairablePartData != null)
		{
			return wsoRepairablePartData;
		}
		WsoRepairablePartData wsoRepairablePartData2 = new WsoRepairablePartData();
		componentData.Add(wsoRepairablePartData2);
		return wsoRepairablePartData2;
	}

	public bool HasRepairablePartData()
	{
		return GetComponentData<WsoRepairablePartData>() != null;
	}

	public void PrepareForGame()
	{
		foreach (WsoComponentDataBase componentDatum in componentData)
		{
			componentDatum.PrepareForGame();
		}
	}

	public void Cleanup()
	{
		foreach (WsoComponentDataBase componentDatum in componentData)
		{
			componentDatum.Cleanup();
		}
	}

	public void NotifyRepairStateChanged()
	{
		this.OnRepairStateChanged?.Invoke();
	}
}
