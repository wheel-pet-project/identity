using System;
using Domain.AccountAggregate;

namespace Domain.Tests.AccountTests.Common;

public class TestingAccountCreator
{
    public Account CreateAccount(
        bool isValidRole = true,
        bool isValidEmail = true,
        bool isValidPhone = true,
        bool isValidPassword = true)
    {
        return new Account(
            CreateRoleId(isValidRole),
            CreateEmail(isValidEmail),
            CreatePhone(isValidPhone),
            CreatePassword(isValidPassword));
    }

    private Role CreateRoleId(bool isValidRole) =>
        isValidRole ? Role.Customer : new Role("unsupported", 0);
    
    private string CreateEmail(bool isValidEmail) =>
        isValidEmail ? "email@mail.com" : "email.com";
    
    private string CreatePhone(bool isValidPhone) =>
        isValidPhone ? "+79008007060" : "69008007060";
    
    private string CreatePassword(bool isValidPassword) =>
        isValidPassword ? "$2a$11$vTQVeAnZdf4xt8chTfthQ.QNxzS6lZhZkjy7NKoLpuxVS6ZNt2WnG" : "notHash";
}