using System;
using LazyBearTechnology;
using UnityEngine;

[Serializable]
public class NPCPointOfInterestData
{
	[SerializeField]
	private string id;

	[SerializeField]
	private bool enabled;

	[SerializeField]
	private SGuid occupiedBy;

	[SerializeField]
	private float weight;

	public string Id => id;

	public bool IsOccupied => !occupiedBy.IsEmpty;

	public NPCPointOfInterestConfiguration Configuration => LazySingletonSO<NPCLifeSimulatorConfiguration>.Instance.GetPointById(Id);

	public float Weight => weight;

	public bool Enabled
	{
		get
		{
			return enabled;
		}
		set
		{
			enabled = value;
			GDPointData gDPointData = GDPointData;
			if (gDPointData != null)
			{
				gDPointData.Enabled = enabled;
			}
		}
	}

	public GDPointData GDPointData
	{
		get
		{
			GDPointData gDPointDataById = MainGame.Instance.GameSave.worldData.gdPointsData.GetGDPointDataById(Id);
			if (gDPointDataById == null)
			{
				Debug.LogError("NPCPointOfInterestConfiguration:[" + Id + "] no linked gd point!!!");
			}
			return gDPointDataById;
		}
	}

	public NPCPointOfInterestData(NPCPointOfInterestConfiguration configuration)
	{
		id = configuration.Id;
		enabled = configuration.EnabledByDefault;
		occupiedBy = SGuid.Empty;
		weight = configuration.StartWeight;
	}

	public void Occupy(SGuid sGuid)
	{
		occupiedBy = sGuid;
	}

	public void Deoccupy()
	{
		Debug.Log("#npc_sim# NPCPointOfInterestData:[" + id + "] Deoccupy");
		occupiedBy = SGuid.Empty;
	}
}
