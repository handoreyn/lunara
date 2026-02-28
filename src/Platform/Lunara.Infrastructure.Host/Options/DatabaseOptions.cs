namespace Lunara.Infrastructure.Host.Options;

/// <summary>Relational database connectivity options.</summary>
public sealed class DatabaseOptions
{
    /// <summary>The configuration section key used when binding from <c>appsettings.json</c>.</summary>
    public const string SectionKey = "Database";

    /// <summary>Gets or sets the ADO.NET / ORM connection string.</summary>
    public string ConnectionString { get; set; } = string.Empty;
}
