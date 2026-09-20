using System;
using UnityEngine;

[Serializable]
public struct GoToDestinationModifierDebugInfo
{
	public string ModifierType;

	public bool IsValid;

	public bool IsStucked;

	public bool ShouldAnchorOnArrival;

	public Vector3 CurrentTargetPosition;

	public string TargetWgoId;

	public string TargetUniqueId;

	public Wgo TargetWgo;

	public DockPointData ActiveDockPoint;

	public DockPoint ActiveDockPointView;

	public DockPointData CustomDockPoint;

	public DockPoint CustomDockPointView;

	public bool WasDockPointValid;

	public string CapturePointName;

	public Vector3 ReservedSlotPosition;

	public bool SpearPositionSelected;

	public Vector3 SpearAttackOffsetLocal;

	public Vector3 SpearTargetPositionWhenSelected;

	private bool IsCombatEntityModifier => ModifierType == "CombatEntityDestinationModifier";

	private bool IsControlPointModifier => ModifierType == "ControlPointDestinationModifier";

	private bool IsSpearModifier => ModifierType == "SpearDestinationModifier";

	private bool HasTargetWgoView => TargetWgo != null;

	private bool HasActiveDockPoint => ActiveDockPoint != null;

	private bool HasCustomDockPoint => CustomDockPoint != null;

	private bool HasActiveDockPointView => ActiveDockPointView != null;

	private bool HasCustomDockPointView => CustomDockPointView != null;

	public static Wgo ResolveTargetWgoView(ICombatEntity targetEntity, WgoData wgoData = null)
	{
		if (targetEntity is Wgo result)
		{
			return result;
		}
		if (wgoData != null)
		{
			return GameScene.GetWgoViewGlobal(wgoData.UniqueId);
		}
		if (targetEntity != null)
		{
			return GameScene.GetWgoViewGlobal(targetEntity.CombatEntityUID);
		}
		return null;
	}

	public static DockPoint ResolveDockPointView(Wgo targetWgo, DockPointData dockPointData)
	{
		if (targetWgo == null || dockPointData == null)
		{
			return null;
		}
		WgoPart mainWgoPart = targetWgo.MainWgoPart;
		if (mainWgoPart == null)
		{
			return null;
		}
		foreach (DockPoint dockPoint in mainWgoPart.DockPoints)
		{
			if (mainWgoPart.GetDockPointData(dockPoint) == dockPointData)
			{
				return dockPoint;
			}
		}
		return null;
	}
}
