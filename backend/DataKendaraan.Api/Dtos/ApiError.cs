using System.Text.Json.Serialization;

namespace DataKendaraan.Api.Dtos;

public record ApiError(
    string Message,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    IDictionary<string, string[]>? Errors = null);
