using System;

namespace FlowCanvas.Nodes;

[ContextDefinedOutputs(new Type[] { typeof(WorldGameObject) })]
public class RelayWGOOutput : RelayValueOutput<WorldGameObject>
{
}
