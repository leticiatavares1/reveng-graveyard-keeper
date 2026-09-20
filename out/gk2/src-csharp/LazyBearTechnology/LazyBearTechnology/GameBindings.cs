using System.Collections.Generic;
using UnityEngine;

namespace LazyBearTechnology;

[CreateAssetMenu(fileName = "GameBindings", menuName = "Lazy/GameBindings", order = 1)]
public class GameBindings : LazySingletonSO<GameBindings>
{
	public List<KeyBinding> keyBindings;

	public List<GamepadBinding> gamepadBindings;

	public List<HoldableElement> canBeHoldedForRepeatPress;

	public List<HoldedGroup> holdedGroups;

	public List<BindingAlias> bindingAliases;
}
