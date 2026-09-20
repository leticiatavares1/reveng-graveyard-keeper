using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NPCLifeSimulatorData
{
	[SerializeField]
	private List<NPCGroupPointOfInterestData> groupPointsOfInterestData = new List<NPCGroupPointOfInterestData>();

	[SerializeField]
	private List<NPCPointOfInterestData> pointsOfInterest = new List<NPCPointOfInterestData>();

	[SerializeField]
	private List<NPCPointOfInterestAnimationData> animationsData = new List<NPCPointOfInterestAnimationData>();

	[SerializeField]
	private List<NPCLifeSimulatorActionData> actionsData = new List<NPCLifeSimulatorActionData>();

	private Dictionary<string, NPCPointOfInterestData> cachedPoints;

	public List<NPCGroupPointOfInterestData> AllGroups => groupPointsOfInterestData;

	public List<NPCPointOfInterestData> AllPoints => pointsOfInterest;

	public List<NPCPointOfInterestAnimationData> AnimationDatas => animationsData;

	public List<NPCLifeSimulatorActionData> ActionsData => actionsData;

	public void PrepareForGame()
	{
		foreach (NPCPointOfInterestData item in pointsOfInterest)
		{
			item.GDPointData?.SetEnabledStateSilent(item.Enabled);
		}
		if (cachedPoints == null)
		{
			cachedPoints = new Dictionary<string, NPCPointOfInterestData>();
		}
		foreach (NPCPointOfInterestData item2 in pointsOfInterest)
		{
			cachedPoints.Add(item2.Id, item2);
		}
	}

	public void AddGroup(NPCGroupPointOfInterestData group)
	{
		groupPointsOfInterestData.Add(group);
	}

	public void AddPoint(NPCPointOfInterestData point)
	{
		pointsOfInterest.Add(point);
	}

	public void ClearData()
	{
		groupPointsOfInterestData.Clear();
		pointsOfInterest.Clear();
		cachedPoints?.Clear();
		actionsData.Clear();
		animationsData.Clear();
	}

	public NPCPointOfInterestData RollPoint(string group)
	{
		NPCGroupPointOfInterestData groupById = GetGroupById(group);
		List<NPCPointOfInterestData> list = new List<NPCPointOfInterestData>();
		foreach (string allPont in groupById.Configuration.AllPonts)
		{
			NPCPointOfInterestData pointById = GetPointById(allPont);
			if (pointById.Enabled && !pointById.IsOccupied)
			{
				list.Add(pointById);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		float num = 0f;
		foreach (NPCPointOfInterestData item in list)
		{
			num += item.Weight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		foreach (NPCPointOfInterestData item2 in list)
		{
			num3 += item2.Weight;
			if (num2 <= num3)
			{
				return item2;
			}
		}
		return list[list.Count - 1];
	}

	public NPCGroupPointOfInterestData GetGroupById(string groupId)
	{
		foreach (NPCGroupPointOfInterestData groupPointsOfInterestDatum in groupPointsOfInterestData)
		{
			if (groupPointsOfInterestDatum.Id == groupId)
			{
				return groupPointsOfInterestDatum;
			}
		}
		return null;
	}

	public NPCPointOfInterestData GetPointById(string pointId)
	{
		if (!cachedPoints.TryGetValue(pointId, out var value))
		{
			return null;
		}
		return value;
	}

	public NPCGroupPointOfInterestData GetGroupByWGOId(SGuid wgoId)
	{
		foreach (NPCGroupPointOfInterestData groupPointsOfInterestDatum in groupPointsOfInterestData)
		{
			foreach (SGuid wgo in groupPointsOfInterestDatum.Wgos)
			{
				if (wgo.Guid == wgoId.Guid)
				{
					return groupPointsOfInterestDatum;
				}
			}
		}
		return null;
	}

	public void AddAnimationData(NPCPointOfInterestAnimationData animationData)
	{
		animationsData.Add(animationData);
	}

	public void TryRemoveAnimationData(SGuid wgoId)
	{
		for (int num = animationsData.Count - 1; num >= 0; num--)
		{
			if (animationsData[num].WgoId.Guid == wgoId.Guid)
			{
				animationsData.RemoveAt(num);
			}
		}
	}

	public void TryRemoveActionData(SGuid wgoId)
	{
		for (int num = actionsData.Count - 1; num >= 0; num--)
		{
			if (actionsData[num].WgoId.Guid == wgoId.Guid)
			{
				actionsData.RemoveAt(num);
			}
		}
	}

	public void AddActionData(NPCLifeSimulatorActionData actionData)
	{
		TryRemoveActionData(actionData.WgoId);
		actionsData.Add(actionData);
	}
}
