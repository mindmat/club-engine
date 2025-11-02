using System.Globalization;

using Microsoft.AspNetCore.Authorization;

namespace AppEngine.Internationalization;

[AllowAnonymous]
public class TranslationQuery : IRequest<IDictionary<string, string>>
{
    public string? Language { get; set; }
}

public class TranslationQueryHandler(Translator translator) : IRequestHandler<TranslationQuery, IDictionary<string, string>>
{
    public Task<IDictionary<string, string>> Handle(TranslationQuery query, CancellationToken cancellationToken)
    {
        var culture = query.Language == null
            ? CultureInfo.InvariantCulture
            : new CultureInfo(query.Language);
        var dict = translator.GetAllTranslations(culture);

        return Task.FromResult(dict);
    }
}