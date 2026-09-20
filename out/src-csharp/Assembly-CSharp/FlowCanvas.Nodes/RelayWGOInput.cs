using System;

namespace FlowCanvas.Nodes;

[ContextDefinedInputs(new Type[] { typeof(WorldGameObject) })]
public class RelayWGOInput : RelayValueInput<WorldGameObject>
{
}
