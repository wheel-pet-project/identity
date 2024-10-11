using System;
using Domain.AccountAggregate;

namespace Domain.Tests.AccountTests.Common;

public class TestingAccountCreator
{
    public Account CreateAccount(
        bool isValidRole = true,
        bool isValidEmail = true,
        bool isValidPhone = true,
        bool isValidPassword = true,
        bool isValidStatus = true)
    {
        return new AccountFactory().CreateAccount(
            CreateRoleId(isValidRole),
            CreateStatus(isValidStatus),
            CreateEmail(isValidEmail),
            CreatePhone(isValidPhone),
            CreatePassword(isValidPassword));
    }

    // todo: remove this if we don't need it
    private Guid CreateId(bool isValidId) =>
        isValidId ? Guid.NewGuid() : Guid.Empty;

    private Role CreateRoleId(bool isValidRole) =>
        isValidRole ? Role.Customer : new Role("unsupported", 0);
    
    private string CreateEmail(bool isValidEmail) =>
        isValidEmail ? "email@mail.com" : "email.com";
    
    private string CreatePhone(bool isValidPhone) =>
        isValidPhone ? "+79008007060" : "69008007060";
    
    private string CreatePassword(bool isValidPassword) =>
        isValidPassword ? "somepassword" : "pass";
    
    private Status CreateStatus(bool isValidStatus) =>
        isValidStatus ? Status.PendingApproval : new Status("unsupported", 0);
}