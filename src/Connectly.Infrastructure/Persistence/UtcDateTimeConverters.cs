using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Connectly.Infrastructure.Persistence;

// SQL Server returns DateTime with Kind=Unspecified, which System.Text.Json serializes WITHOUT a "Z".
// Browsers would then parse it as local time. These converters mark every stored DateTime as UTC.
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

public class NullableUtcDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
    v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
