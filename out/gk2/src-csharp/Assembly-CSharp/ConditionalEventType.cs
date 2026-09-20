using System;

[Flags]
public enum ConditionalEventType
{
	None = 0,
	GameResChanged = 1,
	ItemsChanged = 2,
	CraftProgressChanged = 4,
	CraftStatusChanged = 8,
	WorkStateChanged = 0x10,
	ConveyorChanged = 0x20,
	FightingAgentChanged = 0x40,
	BuildingModeChanged = 0x80,
	CaretakerStateChanged = 0x100,
	DockPointStatusChanged = 0x200,
	TakenDockPointChanged = 0x400,
	HPChanged = 0x800,
	InteractableStateChanged = 0x1000,
	ConveyorSystemChanged = 0x2000,
	ParentWorkCondition = 0x4000,
	ZombieWorkerStateChanged = 0x8000,
	All = -1
}
