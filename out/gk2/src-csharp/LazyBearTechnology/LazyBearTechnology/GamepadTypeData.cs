using System;
using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "GamepadTypeData", menuName = "Lazy/GamepadTypeData", order = 1)]
public class GamepadTypeData : LazySingletonSO<GamepadTypeData>
{
	public GamepadTypeConfiguration[] configurations = new GamepadTypeConfiguration[2]
	{
		new GamepadTypeConfiguration
		{
			gamepadType = GamepadType.Xbox_XboxController
		},
		new GamepadTypeConfiguration
		{
			gamepadType = GamepadType.Sony_DualShock
		}
	};

	public GamepadType GetTypeByGuid(Guid guid)
	{
		for (int i = 0; i < configurations.Length; i++)
		{
			for (int j = 0; j < configurations[i].maps.Length; j++)
			{
				if (configurations[i].maps[j].Guid == guid)
				{
					return configurations[i].gamepadType;
				}
			}
		}
		return GetDefaultTypeForPlatform();
	}

	private static GamepadType GetDefaultTypeForPlatform()
	{
		return GamepadType.Xbox_XboxController;
	}
}
