using System;

namespace FlowCanvas.Nodes;

[ContextDefinedInputs(new Type[] { typeof(Item) })]
public class RelayItemInput : RelayValueInput<Item>
{
}
