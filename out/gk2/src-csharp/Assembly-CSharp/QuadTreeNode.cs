using System;
using System.Collections.Generic;
using UnityEngine;

public class QuadTreeNode<T>
{
	public Rect areaRect;

	protected readonly HashSet<T> objects;

	protected IQuadTreeObjectBounds<T> objectBounds;

	protected bool hasChildren;

	protected QuadTreeNode<T> leftTop;

	protected QuadTreeNode<T> rightTop;

	protected QuadTreeNode<T> rightBot;

	private QuadTreeNode<T> leftBot;

	protected int currentLevel;

	protected static float minLeafWidth;

	protected static float minLeafHeight;

	public QuadTreeNode(float x, float y, float width, float height, IQuadTreeObjectBounds<T> objectBounds, float minLeafWidth, float minLeafHeight, int currentLevel = 0)
	{
		areaRect = new Rect(x, y, width, height);
		objects = new HashSet<T>();
		this.objectBounds = objectBounds;
		QuadTreeNode<T>.minLeafWidth = minLeafWidth;
		QuadTreeNode<T>.minLeafHeight = minLeafHeight;
		this.currentLevel = currentLevel;
	}

	public virtual bool Insert(T obj)
	{
		List<QuadTreeNode<T>> insertedNodes;
		List<T> insertedObjects;
		return InsertInternal(obj, out insertedNodes, out insertedObjects);
	}

	public virtual bool Remove(T obj)
	{
		return objects.Remove(obj);
	}

	protected bool InsertInternal(T obj, out List<QuadTreeNode<T>> insertedNodes, out List<T> insertedObjects)
	{
		insertedNodes = new List<QuadTreeNode<T>>();
		insertedObjects = new List<T>();
		if (obj == null)
		{
			throw new ArgumentNullException("obj");
		}
		if (!IsObjectInside(obj))
		{
			return false;
		}
		if (hasChildren)
		{
			if (leftTop.InsertInternal(obj, out insertedNodes, out insertedObjects))
			{
				return true;
			}
			if (rightTop.InsertInternal(obj, out insertedNodes, out insertedObjects))
			{
				return true;
			}
			if (rightBot.InsertInternal(obj, out insertedNodes, out insertedObjects))
			{
				return true;
			}
			leftBot.InsertInternal(obj, out insertedNodes, out insertedObjects);
			return true;
		}
		objects.Add(obj);
		if ((areaRect.width > minLeafWidth || areaRect.height > minLeafHeight) && objects.Count > 1)
		{
			Quarter(out insertedNodes, out insertedObjects);
		}
		return true;
	}

	public void InsertRange(IEnumerable<T> objects)
	{
		foreach (T @object in objects)
		{
			Insert(@object);
		}
	}

	public virtual void Clear()
	{
		if (hasChildren)
		{
			leftTop.Clear();
			leftTop = null;
			rightTop.Clear();
			rightTop = null;
			rightBot.Clear();
			rightBot = null;
			leftBot.Clear();
			leftBot = null;
		}
		objects.Clear();
		hasChildren = false;
	}

	public int Count()
	{
		int num = 0;
		if (hasChildren)
		{
			num += leftTop.Count();
			num += rightTop.Count();
			num += rightBot.Count();
			return num + leftBot.Count();
		}
		return objects.Count;
	}

	public List<T> FindObjects(Rect searchRect)
	{
		List<T> list = new List<T>();
		if (hasChildren)
		{
			list.AddRange(leftTop.FindObjects(searchRect));
			list.AddRange(rightTop.FindObjects(searchRect));
			list.AddRange(rightBot.FindObjects(searchRect));
			list.AddRange(leftBot.FindObjects(searchRect));
		}
		else if (IsOverlapping(searchRect))
		{
			list.AddRange(objects);
		}
		return list;
	}

	protected bool IsObjectInside(T obj)
	{
		if (objectBounds.GetTop(obj) > areaRect.yMax)
		{
			return false;
		}
		if (objectBounds.GetBot(obj) < areaRect.yMin)
		{
			return false;
		}
		if (objectBounds.GetRight(obj) < areaRect.xMin)
		{
			return false;
		}
		if (objectBounds.GetLeft(obj) > areaRect.xMax)
		{
			return false;
		}
		return true;
	}

	protected bool IsOverlapping(Rect rect)
	{
		return areaRect.Overlaps(rect);
	}

	protected void Quarter(out List<QuadTreeNode<T>> insertedNodes, out List<T> insertedObjects)
	{
		insertedNodes = new List<QuadTreeNode<T>>();
		insertedObjects = new List<T>();
		if (areaRect.width < minLeafWidth && areaRect.height < minLeafHeight)
		{
			return;
		}
		int num = currentLevel + 1;
		hasChildren = true;
		float num2 = areaRect.width / 2f;
		float num3 = areaRect.height / 2f;
		leftTop = new QuadTreeNode<T>(areaRect.xMin, areaRect.yMin, num2, num3, objectBounds, minLeafHeight, minLeafHeight, num);
		rightTop = new QuadTreeNode<T>(areaRect.xMin + num2, areaRect.yMin, num2, num3, objectBounds, minLeafHeight, minLeafHeight, num);
		rightBot = new QuadTreeNode<T>(areaRect.xMin + num2, areaRect.yMin + num3, num2, num3, objectBounds, minLeafHeight, minLeafHeight, num);
		leftBot = new QuadTreeNode<T>(areaRect.xMin, areaRect.yMin + num3, num2, num3, objectBounds, minLeafHeight, minLeafHeight, num);
		foreach (T @object in objects)
		{
			if (InsertInternal(@object, out var insertedNodes2, out var insertedObjects2))
			{
				insertedNodes.AddRange(insertedNodes2);
				insertedObjects.AddRange(insertedObjects2);
			}
		}
		objects.Clear();
	}
}
