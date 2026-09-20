using System.Collections.Generic;

public class QuadTreeRoot<T> : QuadTreeNode<T>
{
	private readonly Dictionary<T, QuadTreeNode<T>> objectsPerNodes;

	public QuadTreeRoot(float x, float y, float width, float height, IQuadTreeObjectBounds<T> objectBounds, float minLeafWidth, float minLeafHeight, int currentLevel = 0)
		: base(x, y, width, height, objectBounds, minLeafHeight, minLeafHeight, currentLevel)
	{
		objectsPerNodes = new Dictionary<T, QuadTreeNode<T>>();
	}

	public override bool Insert(T obj)
	{
		if (InsertInternal(obj, out var insertedNodes, out var insertedObjects))
		{
			for (int i = 0; i < insertedNodes.Count; i++)
			{
				objectsPerNodes.TryAdd(insertedObjects[i], insertedNodes[i]);
			}
			return true;
		}
		return false;
	}

	public override bool Remove(T obj)
	{
		if (objectsPerNodes.TryGetValue(obj, out var _))
		{
			return base.Remove(obj);
		}
		return false;
	}

	public override void Clear()
	{
		base.Clear();
		objectsPerNodes.Clear();
	}
}
