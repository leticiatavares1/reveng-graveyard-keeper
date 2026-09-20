using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu]
public class GameMapSettings : ScriptableObject
{
	[Serializable]
	public enum GameMapZoneType
	{
		Ground = 0,
		Sand = 50,
		Gravel = 100,
		Water = 200,
		Rocks = 255
	}

	[Serializable]
	public struct GameMapZoneDescription
	{
		public GameMapZoneType zone_type;

		public Color color;
	}

	public GameMapZoneDescription[] zones;

	public Color GetColorOfType(GameMapZoneType t)
	{
		GameMapZoneDescription[] array = zones;
		for (int i = 0; i < array.Length; i++)
		{
			GameMapZoneDescription gameMapZoneDescription = array[i];
			if (gameMapZoneDescription.zone_type == t)
			{
				return gameMapZoneDescription.color;
			}
		}
		return Color.cyan;
	}
}
