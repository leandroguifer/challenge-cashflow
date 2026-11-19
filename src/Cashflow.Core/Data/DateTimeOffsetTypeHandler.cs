using System.Data;
using Dapper;

namespace Cashflow.Core.Data;

public sealed class DateTimeOffsetTypeHandler : SqlMapper.TypeHandler<DateTimeOffset>
{
    public override DateTimeOffset Parse(object value) => value switch
    {
        DateTimeOffset offset => offset,
        DateTime dateTime => new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)),
        _ => throw new InvalidCastException($"Cannot convert {value.GetType().Name} to DateTimeOffset.")
    };

    public override void SetValue(IDbDataParameter parameter, DateTimeOffset value) => parameter.Value = value;
}
