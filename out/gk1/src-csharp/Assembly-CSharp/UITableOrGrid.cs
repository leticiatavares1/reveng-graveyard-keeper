using System;
using System.Collections.Generic;
using UnityEngine;

public class UITableOrGrid : MonoBehaviour
{
	private enum Type
	{
		Table,
		Grid,
		SimpleTable
	}

	private bool _inited;

	private Type _type;

	private UITable _table;

	private UIGrid _grid;

	private SimpleUITable _stable;

	private void CheckInit()
	{
		if (!_inited)
		{
			_inited = true;
			_table = GetComponent<UITable>();
			_grid = GetComponent<UIGrid>();
			_stable = GetComponent<SimpleUITable>();
			if (_table != null)
			{
				_type = Type.Table;
			}
			else if (_grid != null)
			{
				_type = Type.Grid;
			}
			else if (_stable != null)
			{
				_type = Type.SimpleTable;
			}
			else
			{
				Debug.LogException(new Exception("No table/grid component found on object " + base.name), this);
			}
		}
	}

	public void Reposition()
	{
		CheckInit();
		switch (_type)
		{
		case Type.Table:
			_table.Reposition();
			_table.repositionNow = true;
			break;
		case Type.Grid:
			_grid.Reposition();
			_grid.repositionNow = true;
			break;
		case Type.SimpleTable:
			_stable.Reposition();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public void DestroyChildren(Transform[] exceptions = null)
	{
		this.DestroyChildren<Transform>(exceptions);
	}

	public void DestroyChildren<T>(T[] exceptions = null) where T : Component
	{
		List<Transform> list = new List<Transform>();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Transform child = base.transform.GetChild(i);
			if (child.GetComponent<T>() == null)
			{
				continue;
			}
			if (exceptions != null)
			{
				bool flag = false;
				for (int j = 0; j < exceptions.Length; j++)
				{
					if (exceptions[j].transform == child)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			list.Add(child);
		}
		foreach (Transform item in list)
		{
			NGUITools.Destroy(item);
		}
	}
}
