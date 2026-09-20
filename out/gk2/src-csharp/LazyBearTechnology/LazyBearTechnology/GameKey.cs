using System;

namespace LazyBearTechnology;

[Serializable]
public class GameKey : Enumeration
{
	public static GameKey None = new GameKey(0);

	public static GameKey LeftClick = new GameKey(1);

	public static GameKey RightClick = new GameKey(2);

	public static GameKey DoubleClick = new GameKey(3);

	public static GameKey Left = new GameKey(4);

	public static GameKey Right = new GameKey(5);

	public static GameKey Up = new GameKey(6);

	public static GameKey Down = new GameKey(7);

	public static GameKey Select = new GameKey(8);

	public static GameKey Back = new GameKey(9);

	public static GameKey DpadLeft = new GameKey(10);

	public static GameKey DpadRight = new GameKey(11);

	public static GameKey DpadUp = new GameKey(12);

	public static GameKey DpadDown = new GameKey(13);

	public static GameKey Interaction = new GameKey(100);

	public static GameKey Action = new GameKey(101);

	public static GameKey CharacterWindow = new GameKey(102);

	public static GameKey Inventory = new GameKey(103);

	public static GameKey Inspirations = new GameKey(104);

	public static GameKey PrevTab = new GameKey(105);

	public static GameKey NextTab = new GameKey(106);

	public static GameKey PrevSubTab = new GameKey(107);

	public static GameKey NextSubTab = new GameKey(108);

	public static GameKey TechTree = new GameKey(109);

	public static GameKey Rotate = new GameKey(110);

	public static GameKey Fold = new GameKey(111);

	public static GameKey Build = new GameKey(112);

	public static GameKey SpeechSkip = new GameKey(113);

	public static GameKey Attack = new GameKey(114);

	public static GameKey Map = new GameKey(115);

	public static GameKey ChangeWeapon = new GameKey(116);

	public static GameKey QuestTree = new GameKey(117);

	public static GameKey LeftTrigger = new GameKey(140);

	public static GameKey RightTrigger = new GameKey(141);

	public static GameKey IncSlider = new GameKey(142);

	public static GameKey DecSlider = new GameKey(143);

	public static GameKey InGameMenu = new GameKey(144);

	public static GameKey ItemMove = new GameKey(145);

	public static GameKey AcceptVendorDeal = new GameKey(146);

	public static GameKey MoveAllItemsToPlayer = new GameKey(147);

	public static GameKey MoveAllItemsFromPlayer = new GameKey(148);

	public static GameKey ExtractBody = new GameKey(149);

	public static GameKey Plant = new GameKey(150);

	public static GameKey SelectPlantElement = new GameKey(151);

	public static GameKey ClearPlantElement = new GameKey(152);

	public static GameKey FoldAdditionalInfo = new GameKey(153);

	public static GameKey StartResurrection = new GameKey(161);

	public static GameKey SaveDelete = new GameKey(172);

	public static GameKey SaveImport = new GameKey(173);

	public static GameKey UseHotBarItem1 = new GameKey(181);

	public static GameKey UseHotBarItem2 = new GameKey(182);

	public static GameKey UseHotBarItem3 = new GameKey(183);

	public static GameKey UseHotBarItem4 = new GameKey(184);

	public static GameKey Dpad = new GameKey(185);

	public static GameKey MaxSlider = new GameKey(186);

	public static GameKey MinSlider = new GameKey(187);

	public static GameKey RightStick = new GameKey(191);

	public static GameKey LeftStick = new GameKey(192);

	public static GameKey StartCraft = new GameKey(201);

	public static GameKey AddCraftToQueue = new GameKey(202);

	public static GameKey AttackFocus = new GameKey(203);

	public static GameKey RightBumper = new GameKey(204);

	public static GameKey ZombieRollName = new GameKey(221);

	public static GameKey RightStickAsGameKey = new GameKey(225);

	public static GameKey CraftWindowZoneSwitch = new GameKey(230);

	public static GameKey CraftWindowQueueLeft = new GameKey(231);

	public static GameKey CraftWindowQueueRight = new GameKey(232);

	public static GameKey PrayStart = new GameKey(235);

	public static GameKey SpeechSkip2 = new GameKey(251);

	public static GameKey AlchemyStart = new GameKey(252);

	public static GameKey SurveyStart = new GameKey(253);

	public static GameKey SubmitBait = new GameKey(254);

	public static GameKey StartFight = new GameKey(255);

	public static GameKey OrderWindowTraders = new GameKey(256);

	public static GameKey EndPrefight = new GameKey(257);

	public static GameKey ItemCountWindow_Decrease = new GameKey(280);

	public static GameKey ItemCountWindow_Increase = new GameKey(281);

	public static GameKey ItemCountWindow_Max = new GameKey(282);

	public static GameKey ItemCountWindow_Min = new GameKey(283);

	public static GameKey ItemCountWindow_Apply = new GameKey(284);

	public static GameKey ItemCountWindow_Cancel = new GameKey(285);

	public static GameKey CheatJump = new GameKey(9998);

	public static GameKey CheatButton = new GameKey(9999);

	public GameKey(int value)
		: base(value)
	{
	}
}
