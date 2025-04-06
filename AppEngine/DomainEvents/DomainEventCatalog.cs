using AppEngine.Internationalization;

namespace AppEngine.DomainEvents;

public class DomainEventCatalog
{
    public List<DomainEventCatalogItem> DomainEventTypes { get; }

    public DomainEventCatalog(IEnumerable<Type> domainEventTypes, Translator translator)
    {
        DomainEventTypes = domainEventTypes.Where(typ => typ != typeof(DomainEvent))
                                           .Select(det => new DomainEventCatalogItem
                                                          {
                                                              TypeName = det.FullName,
                                                              UserText = TranslateType(det.FullName, translator)
                                                          })
                                           .ToList();
    }

    private static string TranslateType(string type, Translator translator)
    {
        return translator.GetResourceString(type.Replace('.', '_'));
    }
}

public class DomainEventCatalogItem
{
    public string? TypeName { get; set; }
    public string UserText { get; set; }
}