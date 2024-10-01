using System;
using Domain.AccountAggregate;

namespace Domain.Tests.AccountTests.Common;

public class TestingAccountCreator
{
    public Account CreateAccount(
        bool isValidId = true,
        bool isValidRole = true,
        bool isValidEmail = true,
        bool isValidPhone = true,
        bool isValidPassword = true,
        bool isValidStatus = true)
    {
        return new AccountFactory().CreateAccount(
            CreateId(isValidId),
            CreateRoleId(isValidRole),
            CreateEmail(isValidEmail),
            CreatePhone(isValidPhone),
            CreatePassword(isValidPassword),
            CreateStatus(isValidStatus));
    }

    // todo: remove this if we don't need it
    private Guid CreateId(bool isValidId) =>
        isValidId ? Guid.NewGuid() : Guid.Empty;

    private Role CreateRoleId(bool isValidRole) =>
        isValidRole ? Role.Customer : new Role("unsupported", 0);
    
    private string CreateEmail(bool isValidEmail) =>
        isValidEmail ? "email@mail.com" : "email.com";
    
    private string CreatePhone(bool isValidPhone) =>
        isValidPhone ? "+7-900-000-00-00" : "900-000-00";
    
    private string CreatePassword(bool isValidPassword) =>
        isValidPassword ? "somepassword" : "pass";
    
    private Status CreateStatus(bool isValidStatus) =>
        isValidStatus ? Status.Confirmed : new Status("unsupported", 0);
}