using System;

namespace FlowCanvas.Nodes;

[ContextDefinedOutputs(new Type[] { typeof(Item) })]
public class RelayItemOutput : RelayValueOutput<Item>
{
}
