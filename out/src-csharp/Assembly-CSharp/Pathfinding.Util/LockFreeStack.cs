using System.Threading;

namespace Pathfinding.Util;

public class LockFreeStack
{
	public Path head;

	public void Push(Path p)
	{
		do
		{
			p.next = head;
		}
		while (Interlocked.CompareExchange(ref head, p, p.next) != p.next);
	}

	public Path PopAll()
	{
		return Interlocked.Exchange(ref head, null);
	}
}
