using Core.Domain.AccountAggregate;
using FluentResults;
using MediatR;

namespace Core.Application.UseCases.CreateAccount;

public record CreateAccountRequest(
    Guid CorrelationId,
    Role Role,
    string Email,
    string Phone,
    string Password) 
    : BaseRequest(CorrelationId), IRequest<Result<CreateAccountResponse>>;