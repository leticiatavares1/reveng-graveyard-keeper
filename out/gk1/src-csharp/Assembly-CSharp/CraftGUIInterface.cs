using System.Collections.Generic;

public interface CraftGUIInterface : CraftInterface
{
	ButtonTipsStr GetButtonTips();

	GamepadNavigationController GetGamepadController();

	List<CraftItemGUI> GetItemsList();

	WorldGameObject GetCrafteryWGO();
}
