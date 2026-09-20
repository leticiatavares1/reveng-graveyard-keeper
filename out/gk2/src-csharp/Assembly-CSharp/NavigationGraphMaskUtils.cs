using System.Collections.Generic;
using Pathfinding;

public static class NavigationGraphMaskUtils
{
	public static GraphMask ToGraphMask(LazyConsts.Navigation.Graph graph)
	{
		if (graph == LazyConsts.Navigation.Graph.None)
		{
			return default(GraphMask);
		}
		return GraphMask.FromGraphIndex((uint)graph);
	}

	public static GraphMask ToGraphMask(IEnumerable<LazyConsts.Navigation.Graph> graphs)
	{
		GraphMask result = default(GraphMask);
		if (graphs == null)
		{
			return result;
		}
		foreach (LazyConsts.Navigation.Graph graph in graphs)
		{
			if (graph != LazyConsts.Navigation.Graph.None)
			{
				result |= GraphMask.FromGraphIndex((uint)graph);
			}
		}
		return result;
	}

	public static GraphMask ToGraphMask(IEnumerable<LazyConsts.Navigation.Graph> graphs, LazyConsts.Navigation.Graph fallbackGraph)
	{
		GraphMask graphMask = ToGraphMask(graphs);
		if (!(graphMask == default(GraphMask)))
		{
			return graphMask;
		}
		return ToGraphMask(fallbackGraph);
	}

	public static GraphMask ToCutGraphMask(LazyConsts.Navigation.Graph graph)
	{
		GraphMask graphMask = ToGraphMask(graph);
		if (graphMask == default(GraphMask))
		{
			return graphMask;
		}
		return graphMask | ToGraphMask(LazyConsts.Navigation.Graph.RuinedTemple);
	}
}
