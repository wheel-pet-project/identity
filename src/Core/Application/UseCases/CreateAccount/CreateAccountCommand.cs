using Core.Domain.AccountAggregate;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.CreateAccount;

public record CreateAccountCommand(
    Role Role,
    string Email,
    string Phone,
    string Password) : IRequest<Result<CreateAccountResponse>>;