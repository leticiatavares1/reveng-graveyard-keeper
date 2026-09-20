public interface IQuadTreeObjectBounds<in T>
{
	float GetTop(T obj);

	float GetRight(T obj);

	float GetBot(T obj);

	float GetLeft(T obj);
}
