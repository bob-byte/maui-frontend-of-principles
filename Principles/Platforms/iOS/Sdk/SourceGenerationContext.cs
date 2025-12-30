using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace Principles;

[JsonSerializable(typeof(LookupResponse))]
internal sealed partial class SourceGenerationContext : JsonSerializerContext;