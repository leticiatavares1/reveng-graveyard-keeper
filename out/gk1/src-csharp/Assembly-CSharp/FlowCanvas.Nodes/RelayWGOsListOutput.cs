using System;
using System.Collections.Generic;

namespace FlowCanvas.Nodes;

[ContextDefinedOutputs(new Type[] { typeof(List<WorldGameObject>) })]
public class RelayWGOsListOutput : RelayValueOutput<List<WorldGameObject>>
{
}
