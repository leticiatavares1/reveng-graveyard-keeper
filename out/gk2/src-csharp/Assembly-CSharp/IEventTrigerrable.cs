public interface IEventTrigerrable
{
	string TriggerableId { get; }

	GlobalEventsSystem.Event.Type Type { get; }

	SGuid UniqueId { get; set; }

	bool OnTriggerPassed();
}
