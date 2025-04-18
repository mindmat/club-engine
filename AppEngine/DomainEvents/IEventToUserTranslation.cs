﻿namespace AppEngine.DomainEvents;

public interface IEventToUserTranslation<in TDomainEvent>
    where TDomainEvent : DomainEvent
{
    public string GetText(TDomainEvent domainEvent);
}