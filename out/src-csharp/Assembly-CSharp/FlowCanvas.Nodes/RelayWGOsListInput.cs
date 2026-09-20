using System;
using System.Collections.Generic;

namespace FlowCanvas.Nodes;

[ContextDefinedInputs(new Type[] { typeof(List<WorldGameObject>) })]
public class RelayWGOsListInput : RelayValueInput<List<WorldGameObject>>
{
}
