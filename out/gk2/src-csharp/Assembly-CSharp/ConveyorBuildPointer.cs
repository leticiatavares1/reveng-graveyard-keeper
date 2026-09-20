using System.Collections.Generic;
using UnityEngine;

public class ConveyorBuildPointer : WgoBuildPointer
{
	public override bool TryDoBuildAction()
	{
		if (shownAsActive)
		{
			takeResourcesAction?.Invoke();
			WgoData wgoData = null;
			if (GameBalance.Me.conveyorWgosCache.TryGetValue(buildData.WgoId, out var value))
			{
				wgoData = new ConveyorWgoData(value.conveyorType, buildData.WgoId, target.Data.Position, gameScene.Id);
				if (target.CanBeRotated() && target.MainWgoPart.WgoPartData.rotationIndex != -1)
				{
					wgoData.MainWgoPartData.variationId = target.MainWgoPart.WgoPartData.variationId;
					wgoData.MainWgoPartData.rotationIndex = target.MainWgoPart.WgoPartData.rotationIndex;
				}
				Wgo wgo = gameScene.AddWgoData(wgoData);
				if (buildData.Definition != null)
				{
					foreach (LazyExpression item in buildData.Definition.expressionAfterBuilding)
					{
						item.EvaluateBool(wgo.Data);
					}
				}
				MakeConnections(wgo);
				TryMakeAutoBuilds(wgo);
				return true;
			}
			Debug.LogError("ConveyorBuildPointer could not find wgo def [{buildData.WgoId}]");
			return false;
		}
		return false;
	}

	public void MakeConnections(Wgo builtWgo)
	{
		Physics.SyncTransforms();
		if (!(builtWgo.Data is ConveyorWgoData conveyorWgoData))
		{
			return;
		}
		ConnectivityRestriction connectivityRestriction = null;
		Collider[] array = new Collider[10];
		if (Physics.OverlapBoxNonAlloc(builtWgo.transform.position, builtWgo.MainWgoPart.WgoPartData.BakedData.ChunkBounds.withoutShadows.extents / 4f, array, Quaternion.identity) > 0)
		{
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (!(collider == null))
				{
					Wgo componentInParent = collider.GetComponentInParent<Wgo>();
					if ((!(componentInParent != null) || (!componentInParent.Data.isTempObject && !(componentInParent == builtWgo))) && collider.TryGetComponent<ConnectivityRestriction>(out var component))
					{
						connectivityRestriction = component;
						break;
					}
				}
			}
		}
		BuildConnector[] componentsInChildren = builtWgo.GetComponentsInChildren<BuildConnector>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (!componentsInChildren[j].gameObject.activeInHierarchy)
			{
				continue;
			}
			Collider[] array3 = new Collider[10];
			if (Physics.OverlapBoxNonAlloc(componentsInChildren[j].BoxCollider.bounds.center, componentsInChildren[j].BoxCollider.bounds.extents / 2f, array3, Quaternion.identity, 524288) <= 0)
			{
				continue;
			}
			Collider[] array2 = array3;
			foreach (Collider collider2 in array2)
			{
				if (collider2 == null)
				{
					continue;
				}
				Wgo componentInParent2 = collider2.GetComponentInParent<Wgo>();
				if ((!(componentInParent2 != null) || (!componentInParent2.Data.isTempObject && !(componentInParent2 == builtWgo))) && componentInParent2 != null && componentInParent2.Data is ConveyorWgoData)
				{
					if (componentInParent2.Data.Definition.conveyorType != ConveyorElementType.UndergroundCell)
					{
						componentsInChildren[j].TryConnect(componentInParent2);
						break;
					}
					if ((builtWgo.Data.Definition.conveyorType == ConveyorElementType.UndergroundCell || !(connectivityRestriction != null) || !connectivityRestriction.disabledTypes.Contains(componentInParent2.Data.Definition.conveyorType)) && collider2.TryGetComponent<ConveyorCellSequenceIdentifier>(out var component2) && component2.isStartElement)
					{
						componentsInChildren[j].TryConnect(componentInParent2);
						break;
					}
				}
			}
		}
		BoxCollider[] componentsInChildren2 = builtWgo.GetComponentsInChildren<BoxCollider>();
		List<BoxCollider> list = new List<BoxCollider>();
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			if (componentsInChildren2[k].gameObject.layer == 19)
			{
				list.Add(componentsInChildren2[k]);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		bool flag = builtWgo.Data.Definition.conveyorType == ConveyorElementType.UndergroundCell;
		foreach (BoxCollider item in list)
		{
			if (flag && (!item.TryGetComponent<ConveyorCellSequenceIdentifier>(out var component3) || !component3.isStartElement))
			{
				continue;
			}
			Collider[] array4 = new Collider[20];
			Vector3 halfExtents = item.bounds.extents / 2f;
			halfExtents.y = 0.2f;
			if (Physics.OverlapBoxNonAlloc(item.bounds.center, halfExtents, array4, Quaternion.identity, 134217728) <= 0)
			{
				continue;
			}
			for (int l = 0; l < array4.Length; l++)
			{
				if (!(array4[l] != null))
				{
					continue;
				}
				Wgo componentInParent3 = array4[l].GetComponentInParent<Wgo>();
				if (!(componentInParent3 != null) || (!componentInParent3.Data.isTempObject && !(componentInParent3 == builtWgo)))
				{
					BuildConnector component4 = array4[l].GetComponent<BuildConnector>();
					if (!(component4 == null))
					{
						component4.TryConnect(builtWgo);
					}
				}
			}
		}
		conveyorWgoData.ConveyorComponent.UpdateWgoPartState();
		MainGame.Instance.conveyorSystem.AddConveyorObject(conveyorWgoData.ConveyorComponent);
		builtWgo.GetComponentInChildren<ConveyorAnimator>()?.TryRegister();
	}

	public static void TryMakeConnectionsOutsideBuildSystem(Wgo builtWgo)
	{
		Physics.SyncTransforms();
		if (!(builtWgo.Data is ConveyorWgoData conveyorWgoData))
		{
			return;
		}
		ConnectivityRestriction connectivityRestriction = null;
		Collider[] array = new Collider[10];
		if (Physics.OverlapBoxNonAlloc(builtWgo.transform.position, builtWgo.MainWgoPart.WgoPartData.BakedData.ChunkBounds.withoutShadows.extents / 4f, array, Quaternion.identity) > 0)
		{
			Collider[] array2 = array;
			foreach (Collider collider in array2)
			{
				if (!(collider == null))
				{
					Wgo componentInParent = collider.GetComponentInParent<Wgo>();
					if ((!(componentInParent != null) || (!componentInParent.Data.isTempObject && !(componentInParent == builtWgo))) && collider.TryGetComponent<ConnectivityRestriction>(out var component))
					{
						connectivityRestriction = component;
						break;
					}
				}
			}
		}
		BuildConnector[] componentsInChildren = builtWgo.GetComponentsInChildren<BuildConnector>();
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (!componentsInChildren[j].gameObject.activeInHierarchy)
			{
				continue;
			}
			Collider[] array3 = new Collider[10];
			if (Physics.OverlapBoxNonAlloc(componentsInChildren[j].BoxCollider.bounds.center, componentsInChildren[j].BoxCollider.bounds.extents / 2f, array3, Quaternion.identity, 524288) <= 0)
			{
				continue;
			}
			Collider[] array2 = array3;
			foreach (Collider collider2 in array2)
			{
				if (collider2 == null)
				{
					continue;
				}
				Wgo componentInParent2 = collider2.GetComponentInParent<Wgo>();
				if ((!(componentInParent2 != null) || (!componentInParent2.Data.isTempObject && !(componentInParent2 == builtWgo))) && componentInParent2 != null && componentInParent2.Data is ConveyorWgoData)
				{
					if (componentInParent2.Data.Definition.conveyorType != ConveyorElementType.UndergroundCell)
					{
						componentsInChildren[j].TryConnect(componentInParent2);
						break;
					}
					if ((builtWgo.Data.Definition.conveyorType == ConveyorElementType.UndergroundCell || !(connectivityRestriction != null) || !connectivityRestriction.disabledTypes.Contains(componentInParent2.Data.Definition.conveyorType)) && collider2.TryGetComponent<ConveyorCellSequenceIdentifier>(out var component2) && component2.isStartElement)
					{
						componentsInChildren[j].TryConnect(componentInParent2);
						break;
					}
				}
			}
		}
		BoxCollider[] componentsInChildren2 = builtWgo.GetComponentsInChildren<BoxCollider>();
		List<BoxCollider> list = new List<BoxCollider>();
		for (int k = 0; k < componentsInChildren2.Length; k++)
		{
			if (componentsInChildren2[k].gameObject.layer == 19)
			{
				list.Add(componentsInChildren2[k]);
			}
		}
		if (list.Count == 0)
		{
			return;
		}
		bool flag = builtWgo.Data.Definition.conveyorType == ConveyorElementType.UndergroundCell;
		foreach (BoxCollider item in list)
		{
			if (flag && (!item.TryGetComponent<ConveyorCellSequenceIdentifier>(out var component3) || !component3.isStartElement))
			{
				continue;
			}
			Collider[] array4 = new Collider[20];
			Vector3 halfExtents = item.bounds.extents / 2f;
			halfExtents.y = 0.2f;
			if (Physics.OverlapBoxNonAlloc(item.bounds.center, halfExtents, array4, Quaternion.identity, 134217728) <= 0)
			{
				continue;
			}
			for (int l = 0; l < array4.Length; l++)
			{
				if (!(array4[l] != null))
				{
					continue;
				}
				Wgo componentInParent3 = array4[l].GetComponentInParent<Wgo>();
				if (!(componentInParent3 != null) || (!componentInParent3.Data.isTempObject && !(componentInParent3 == builtWgo)))
				{
					BuildConnector component4 = array4[l].GetComponent<BuildConnector>();
					if (!(component4 == null))
					{
						component4.TryConnect(builtWgo);
					}
				}
			}
		}
		conveyorWgoData.ConveyorComponent.UpdateWgoPartState();
		MainGame.Instance.conveyorSystem.AddConveyorObject(conveyorWgoData.ConveyorComponent);
		builtWgo.GetComponentInChildren<ConveyorAnimator>()?.TryRegister();
		TryMakeAutoBuildsOutsideBuildSystem(builtWgo);
	}

	private static void TryMakeAutoBuildsOutsideBuildSystem(Wgo builtWgo)
	{
		ConveyorCellAutoBuilder componentInChildren = builtWgo.GetComponentInChildren<ConveyorCellAutoBuilder>();
		if (!(componentInChildren != null))
		{
			return;
		}
		Wgo wgo = componentInChildren.AutoBuildCell();
		if (wgo != null)
		{
			TryMakeConnectionsOutsideBuildSystem(wgo);
			if (builtWgo.Data is ConveyorWgoData conveyorWgoData)
			{
				wgo.Data.SetGameRes("conveyor_build_is_not_removable", 1);
				conveyorWgoData.HardConnectedWGOs.Add(new SGuid(wgo.Data.UniqueId.Guid));
			}
		}
	}

	private void TryMakeAutoBuilds(Wgo builtWgo)
	{
		ConveyorCellAutoBuilder componentInChildren = builtWgo.GetComponentInChildren<ConveyorCellAutoBuilder>();
		if (!(componentInChildren != null))
		{
			return;
		}
		Wgo wgo = componentInChildren.AutoBuildCell();
		if (wgo != null)
		{
			MakeConnections(wgo);
			if (builtWgo.Data is ConveyorWgoData conveyorWgoData)
			{
				wgo.Data.SetGameRes("conveyor_build_is_not_removable", 1);
				conveyorWgoData.HardConnectedWGOs.Add(new SGuid(wgo.Data.UniqueId.Guid));
			}
		}
	}
}
