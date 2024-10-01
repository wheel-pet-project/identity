using Ardalis.SmartEnum.Dapper;
using Dapper;

namespace API.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddSqlMapperForEnums()
    {
        SqlMapper.AddTypeHandler(typeof(Role), new SmartEnumByValueTypeHandler<Domain.AccountAggregate.Role>());
        SqlMapper.AddTypeHandler(typeof(Status), new SmartEnumByValueTypeHandler<Domain.AccountAggregate.Status>());
    }
}