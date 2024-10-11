using Ardalis.SmartEnum.Dapper;
using Dapper;
using Domain.AccountAggregate;

namespace API.Extensions;

public static class SqlMapperRegister
{
    public static void Register()
    {
        SqlMapper.AddTypeHandler(typeof(Role), new SmartEnumByValueTypeHandler<Role>());
        SqlMapper.AddTypeHandler(typeof(Status), new SmartEnumByValueTypeHandler<Status>());
    }
}