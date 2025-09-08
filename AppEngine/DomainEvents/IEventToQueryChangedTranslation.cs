using AppEngine.ReadModels;

namespace AppEngine.DomainEvents;

public interface IEventToQueryChangedTranslation<in TEvent>
    where TEvent : DomainEvent
{
    IEnumerable<QueryChanged> Translate(TEvent e);
}