using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lunara.BuildingBlocks.EventContracts.V1;

/// <summary>
/// Provides JSON serialization and deserialization for typed integration event contracts.
/// All methods share a single <see cref="JsonSerializerOptions"/> instance with safe, consistent defaults.
/// </summary>
public static class EventJson
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Serializes <paramref name="payload"/> to a JSON string.
    /// </summary>
    /// <typeparam name="T">The event contract type to serialize.</typeparam>
    /// <param name="payload">The event payload instance to serialize.</param>
    /// <returns>A JSON string representation of <paramref name="payload"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="payload"/> is <c>null</c> for reference types.
    /// </exception>
    public static string Serialize<T>(T payload)
    {
        if (payload is null)
        {
            throw new ArgumentNullException(nameof(payload), "Event payload cannot be null.");
        }
        return JsonSerializer.Serialize(payload, _options);
    }

    /// <summary>
    /// Deserializes a JSON string to an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The event contract type to deserialize into.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized <typeparamref name="T"/> instance.</returns>
    /// <exception cref="JsonException">
    /// Thrown when <paramref name="json"/> is invalid or maps to a null value.
    /// </exception>
    public static T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options)
            ?? throw new JsonException($"Deserialization of {typeof(T).Name} produced a null result.");
    }
}
