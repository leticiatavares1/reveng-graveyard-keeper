using UnityEngine;

public class ConveyorCellBuildConnector : BuildConnector
{
	[SerializeField]
	private Direction direction;

	public override bool TryConnect(Wgo other)
	{
		Vector3 to = base.transform.position - GetComponentInParent<Wgo>().transform.position;
		float num = 0f;
		switch (other.Data.Definition.conveyorType)
		{
		case ConveyorElementType.Cell:
			num = Mathf.Abs(Vector3.Angle(other.gameObject.GetComponentInChildren<BuildConnector>().transform.position - other.transform.position, to));
			if (num < 180f)
			{
				base.Parent.Connect(other.Data as ConveyorWgoData, ConveyorConnectionType.Out, direction);
			}
			break;
		case ConveyorElementType.Chest:
			base.Parent.Connect(other.Data as ConveyorWgoData, ConveyorConnectionType.Out, direction);
			break;
		case ConveyorElementType.Splitter:
		{
			BuildConnector[] componentsInChildren = other.gameObject.GetComponentsInChildren<BuildConnector>();
			foreach (BuildConnector buildConnector in componentsInChildren)
			{
				if (buildConnector.gameObject.activeSelf && buildConnector.ConnectionType == ConveyorConnectionType.In)
				{
					num = Mathf.Abs(Vector3.Angle(buildConnector.transform.position - other.transform.position, to));
					if (num.EqualsTo(180f) || num.EqualsTo(0f))
					{
						base.Parent.Connect(other.Data as ConveyorWgoData, ConveyorConnectionType.Out, direction);
					}
				}
			}
			break;
		}
		case ConveyorElementType.UndergroundCell:
			num = Mathf.Abs(Vector3.Angle(other.gameObject.GetComponentInChildren<BuildConnector>().transform.position - other.transform.position, to));
			if (num < 180f)
			{
				base.Parent.Connect(other.Data as ConveyorWgoData, ConveyorConnectionType.Out, direction);
			}
			break;
		case ConveyorElementType.StationCell:
			if (Mathf.Abs(Vector3.Angle(other.gameObject.GetComponentInChildren<BuildConnector>().transform.position - other.transform.position, to)).Equals(180f))
			{
				base.Parent.Connect(other.Data as ConveyorWgoData, ConveyorConnectionType.Out, direction);
			}
			break;
		}
		return false;
	}
}
