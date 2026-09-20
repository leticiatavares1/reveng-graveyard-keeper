using UnityEngine;

public class BuildConnector : MonoBehaviour
{
	[SerializeField]
	private BoxCollider boxCollider;

	[SerializeField]
	private ConveyorConnectionType connectionType;

	private ConveyorComponent parent;

	public ConveyorConnectionType ConnectionType => connectionType;

	protected ConveyorComponent Parent => (GetComponentInParent<Wgo>().Data as ConveyorWgoData)?.ConveyorComponent;

	public BoxCollider BoxCollider
	{
		get
		{
			if (boxCollider == null)
			{
				boxCollider = GetComponent<BoxCollider>();
			}
			return boxCollider;
		}
	}

	public void TryDisconnect(ConveyorWgoData conveyorWgoData)
	{
		if (Parent != null)
		{
			Parent.Disconnect(conveyorWgoData, connectionType);
		}
	}

	public virtual bool TryConnect(Wgo conveyorWgo)
	{
		if (Parent == null)
		{
			return false;
		}
		return Parent.Connect(conveyorWgo.Data as ConveyorWgoData, connectionType);
	}
}
