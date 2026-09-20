using System;
using UnityEngine;

[Serializable]
public class WorldSimpleObject : MonoBehaviour
{
	public enum WSOType
	{
		Nothing = -1,
		WallStraight,
		WallCorner,
		Floor,
		Spawner,
		InteriorFence,
		InteriorColumn
	}

	[SerializeField]
	public WSOType wso_type = WSOType.Nothing;

	[NonSerialized]
	public long unique_id = -1L;
}
