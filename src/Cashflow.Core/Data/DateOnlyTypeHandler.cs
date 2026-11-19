using System.Data;
using System.Globalization;
using Dapper;

namespace Cashflow.Core.Data;

public sealed class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
{
    public override DateOnly Parse(object value) =>
        value is DateOnly date ? date : DateOnly.FromDateTime(Convert.ToDateTime(value, CultureInfo.InvariantCulture));

    public override void SetValue(IDbDataParameter parameter, DateOnly value)
    {
        parameter.DbType = DbType.Date;
        parameter.Value = value;
    }
}
