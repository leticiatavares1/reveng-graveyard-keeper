public interface IProxyClassTargeted<T> where T : class
{
	T Target { get; set; }
}
