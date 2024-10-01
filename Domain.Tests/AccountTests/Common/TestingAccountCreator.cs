using System;
using Domain.Account;

namespace Domain.Tests.AccountTests.Common;

public class TestingAccountCreator
{
    public Account.Account CreateAccount(
        bool isValidId = true,
        bool isValidRoleId = true,
        bool isValidEmail = true,
        bool isValidPhone = true,
        bool isValidPassword = true,
        bool isActive = true,
        bool isDeleted = false)
    {
        return new AccountFactory().CreateAccount(
            CreateId(isValidId),
            CreateRoleId(isValidRoleId),
            CreateEmail(isValidEmail),
            CreatePhone(isValidPhone),
            CreatePassword(isValidPassword),
            isActive,
            isDeleted);
    }

    // todo: remove this if we don't need it
    private Guid CreateId(bool isValidId) =>
        isValidId ? Guid.NewGuid() : Guid.Empty;

    private int CreateRoleId(bool isValidRoleId) => 
        isValidRoleId ? 1 : -1;
    
    private string CreateEmail(bool isValidEmail) =>
        isValidEmail ? "email@mail.com" : "email.com";
    
    private string CreatePhone(bool isValidPhone) =>
        isValidPhone ? "+7-900-000-00-00" : "900-000-00";
    
    private string CreatePassword(bool isValidPassword) =>
        isValidPassword ? "somepassword" : "pass";
}