using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

public class LazyButtonTipStrTester : MonoBehaviour
{
	[SerializeField]
	private PlatformType testPlatformType;

	[SerializeField]
	private GamepadType testGamepadType;

	[SerializeField]
	private LazyButtonTipsStr lazyButtonTips;

	[SerializeField]
	private List<GameKey> keysToTest;

	private void TestPlatformTips()
	{
		Platform.ForceDebugPlatform(testPlatformType);
		LazyInput.ForceGamepadActivityState(isActive: true);
		LazyInput.ForceDebugGamepadType(testGamepadType);
		LazySingletonSO<ControllerIconLibrary>.Instance.Init();
		List<LazyGameKeyTip> list = new List<LazyGameKeyTip>();
		foreach (GameKey item in keysToTest)
		{
			list.Add(new LazyGameKeyTip(item, "Test Active", active: true, gamepadOnly: false, translate: false));
			list.Add(new LazyGameKeyTip(item, "Test Inactive", active: false, gamepadOnly: false, translate: false));
		}
		lazyButtonTips.Print(list, "\n\n");
		Platform.ClearForcedPlatform();
		LazyInput.ClearForcedGamepadType();
		LazyInput.ClearGamepadActivityState();
	}

	private void ForcePlatformAndGamepadType()
	{
		Platform.ForceDebugPlatform(testPlatformType);
		LazyInput.ForceDebugGamepadType(testGamepadType);
		LazySingletonSO<ControllerIconLibrary>.Instance.Init();
	}
}
