namespace Munaqasat.Web.Domain.Entities;

public class Language
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string NativeName { get; set; } = "";
    public TextDirection Direction { get; set; } = TextDirection.Ltr;
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
}

public class UiTranslation : IHasTimestamps, ITranslation
{
    public string Id { get; set; } = Ids.New();
    public string Namespace { get; set; } = "";
    public string Key { get; set; } = "";
    public string LanguageCode { get; set; } = "";
    public Language Language { get; set; } = null!;
    public string Value { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
