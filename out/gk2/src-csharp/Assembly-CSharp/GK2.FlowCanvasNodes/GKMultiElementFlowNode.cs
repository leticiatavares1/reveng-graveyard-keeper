using System;
using System.Collections.Generic;
using FlowCanvas;
using LinqTools;
using UnityEngine;

namespace GK2.FlowCanvasNodes;

public abstract class GKMultiElementFlowNode<T> : GKCustomFlowNode where T : Element
{
	public const string ID_PLAYER = "[Player]";

	public const string ID_WISP = "[Wisp]";

	public const string ID_SELF = "[Self]";

	public static Color CLR_PLAYER = Color.yellow;

	public static Color CLR_WISP = Color.cyan;

	public static Color CLR_SELF = Color.green;

	protected string[] wgoIdsArray;

	protected ValueInput<string[]> wgosIds;

	protected ValueInput<T[]> elements;

	protected void RegisterWgoIdsPorts()
	{
		wgosIds = AddValueInput<string[]>("WGOs List");
		wgosIds.SetDefaultAndSerializedValue(new string[0]);
		elements = AddValueInput<T[]>("Elements");
		elements.SetDefaultAndSerializedValue(Array.Empty<T>());
	}

	public void AddNewElement(T el)
	{
		List<T> list = elements.value.ToList();
		list.Add(el);
		elements.serializedValue = list.ToArray();
	}

	public void AddNewWgo(string WGOId = "")
	{
		List<string> list = wgosIds.value.ToList();
		list.Add(WGOId);
		wgosIds.serializedValue = list.ToArray();
	}

	public void AddNewWgoIfAbsent(string WGOId = "")
	{
		if (!wgosIds.value.Contains(WGOId))
		{
			switch (WGOId)
			{
			case "[Wisp]":
				return;
			case "[Self]":
				return;
			}
			AddNewWgo(WGOId);
		}
	}

	protected virtual void SeparateNode(int i)
	{
		GKMultiElementFlowNode<T> gKMultiElementFlowNode = base.flowGraph.AddNode<GKMultiElementFlowNode<T>>(base.position + Vector2.right * 200f);
		string[] value = wgosIds.value;
		foreach (string wGOId in value)
		{
			gKMultiElementFlowNode.AddNewWgoIfAbsent(wGOId);
		}
		List<T> list = elements.value.ToList();
		for (int k = i + 1; k < elements.value.Length; k++)
		{
			gKMultiElementFlowNode.AddNewElement(elements.value[k]);
			list.RemoveAt(i + 1);
		}
		elements.serializedValue = list.ToArray();
		RemoveUnusedWGOs();
		gKMultiElementFlowNode.RemoveUnusedWGOs();
	}

	protected void RemoveUnusedWGOs()
	{
		List<string> list = new List<string>();
		T[] value = elements.value;
		foreach (Element element in value)
		{
			if (element.wgoId != "[Self]" && element.wgoId != "[Player]" && element.wgoId != "[Wisp]" && !list.Contains(element.wgoId))
			{
				list.Add(element.wgoId);
			}
		}
		List<string> list2 = wgosIds.value.ToList();
		for (int j = 0; j < list2.Count; j++)
		{
			if (!list.Contains(list2[j]))
			{
				list2.RemoveAt(j);
				j--;
			}
		}
		wgosIds.serializedValue = list2.ToArray();
	}
}
