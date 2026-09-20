using System.Collections.Generic;
using LazyBearTechnology;
using UnityEngine;

[CreateAssetMenu(menuName = "GK2/NPCLifeSimulatorConfiguration", fileName = "NPCLifeSimulatorConfiguration")]
public class NPCLifeSimulatorConfiguration : LazySingletonSO<NPCLifeSimulatorConfiguration>
{
	[SerializeField]
	private List<NPCGroupPointOfInterestConfiguration> groupPointsOfInterestData;

	[SerializeField]
	private List<NPCPointOfInterestConfiguration> pointsOfInterestData;

	[SerializeField]
	private float minActionDelay;

	[SerializeField]
	private float maxActionDelay;

	public float MinActionDelay => minActionDelay;

	public float MaxActionDelay => maxActionDelay;

	public List<NPCGroupPointOfInterestConfiguration> AllGroups => groupPointsOfInterestData;

	public List<NPCPointOfInterestConfiguration> AllPonts => pointsOfInterestData;

	public NPCGroupPointOfInterestConfiguration GetGroupById(string groupId)
	{
		foreach (NPCGroupPointOfInterestConfiguration groupPointsOfInterestDatum in groupPointsOfInterestData)
		{
			if (groupPointsOfInterestDatum.Id == groupId)
			{
				return groupPointsOfInterestDatum;
			}
		}
		return null;
	}

	public NPCPointOfInterestConfiguration GetPointById(string id)
	{
		foreach (NPCPointOfInterestConfiguration pointsOfInterestDatum in pointsOfInterestData)
		{
			if (pointsOfInterestDatum.Id == id)
			{
				return pointsOfInterestDatum;
			}
		}
		return null;
	}
}
