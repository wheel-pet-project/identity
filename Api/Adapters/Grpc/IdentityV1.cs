using Core.Application.UseCases.Authenticate;
using Core.Application.UseCases.Authorize;
using Core.Application.UseCases.ConfirmEmail;
using Core.Application.UseCases.CreateAccount;
using Core.Application.UseCases.RecoverPassword;
using Core.Application.UseCases.RefreshAccessToken;
using Core.Application.UseCases.UpdatePassword;
using Core.Domain.SharedKernel.Errors;
using FluentResults;
using Grpc.Core;
using MediatR;
using Proto.IdentityV1;
using Status = Grpc.Core.Status;

namespace Api.Adapters.Grpc;

public class IdentityV1(IMediator mediator, Mapper.Mapper mapper)
    : Identity.IdentityBase
{
    public override async Task<CreateResponse> CreateAccount(
        CreateRequest request, ServerCallContext context)
    {
        var createAccountRequest = new CreateAccountRequest(
            Guid.Parse(request.CorId),
            mapper.RoleFromRequest(request.Role),
            request.Email,
            request.Phone,
            request.Pass);

        var result = await mediator.Send(createAccountRequest, context.CancellationToken);

        return result.IsSuccess
            ? new CreateResponse { AccId = result.Value.AccountId.ToString() }
            : ParseErrorToRpcException<CreateResponse>(result.Errors);
    }

    public override async Task<ConfirmEmailResponse> ConfirmEmail(ConfirmEmailRequest request,
        ServerCallContext context)
    {
        var confirmEmailRequest = new ConfirmAccountEmailRequest(
            Guid.Parse(request.CorId),
            Guid.Parse(request.AccId),
            Guid.Parse(request.ConfirmationTkn));

        var result = await mediator.Send(confirmEmailRequest, context.CancellationToken);

        return result.IsSuccess
            ? new ConfirmEmailResponse()
            : ParseErrorToRpcException<ConfirmEmailResponse>(result.Errors);
    }

    public override async Task<AuthenticateResponse> Authenticate(
        AuthenticateRequest request, ServerCallContext context)
    {
        var authenticateRequest = new AuthenticateAccountRequest(
            Guid.Parse(request.CorId),
            request.Email,
            request.Pass);

        var result = await mediator.Send(authenticateRequest, context.CancellationToken);

        return result.IsSuccess
            ? new AuthenticateResponse { Tkn = result.Value.AccessToken, RefreshTkn = result.Value.RefreshToken }
            : ParseErrorToRpcException<AuthenticateResponse>(result.Errors);
    }

    public override async Task<AuthorizeResponse> Authorize(
        AuthorizeRequest request, ServerCallContext context)
    {
        var authorizeRequest = new AuthorizeAccountRequest(Guid.Parse(request.CorId), request.Tkn);

        var result = await mediator.Send(authorizeRequest, context.CancellationToken);

        return result.IsSuccess
            ? new AuthorizeResponse
            {
                AccId = result.Value.AccountId.ToString(),
                Role = mapper.RoleToResponse(result.Value.Role),
                Status = mapper.StatusToResponse(result.Value.Status)
            }
            : ParseErrorToRpcException<AuthorizeResponse>(result.Errors);
    }

    public override async Task<RefreshAccessTokenResponse> RefreshAccessToken(
        RefreshAccessTokenRequest request, ServerCallContext context)
    {
        var refreshAccessTokenRequest =
            new RefreshAccountAccessTokenRequest(Guid.Parse(request.CorId), request.RefreshTkn);

        var result = await mediator.Send(refreshAccessTokenRequest, context.CancellationToken);

        return result.IsSuccess
            ? new RefreshAccessTokenResponse
            {
                Tkn = result.Value.AccessToken,
                RefreshTkn = result.Value.RefreshToken
            }
            : ParseErrorToRpcException<RefreshAccessTokenResponse>(result.Errors);
    }

    public override async Task<RecoverPasswordResponse> RecoverPassword(RecoverPasswordRequest request,
        ServerCallContext context)
    {
        var recoverPasswordRequest = new RecoverAccountPasswordRequest(Guid.Parse(request.CorId), request.Email);

        var result = await mediator.Send(recoverPasswordRequest, context.CancellationToken);

        return result.IsSuccess
            ? new RecoverPasswordResponse()
            : ParseErrorToRpcException<RecoverPasswordResponse>(result.Errors);
    }

    public override async Task<UpdatePasswordResponse> UpdatePassword(UpdatePasswordRequest request,
        ServerCallContext context)
    {
        var updatePasswordRequest = new UpdateAccountPasswordRequest(
            Guid.Parse(request.CorId),
            request.NewPass,
            request.Email,
            Guid.Parse(request.ResetTkn));

        var result = await mediator.Send(updatePasswordRequest, context.CancellationToken);

        return result.IsSuccess
            ? new UpdatePasswordResponse()
            : ParseErrorToRpcException<UpdatePasswordResponse>(result.Errors);
    }

    private T ParseErrorToRpcException<T>(List<IError> errors)
    {
        if (errors.Exists(x => x is NotFound))
            throw new RpcException(new Status(StatusCode.NotFound, string.Join(' ', errors.Select(x => x.Message))));

        if (errors.Exists(x => x is TransactionFail))
            throw new RpcException(new Status(StatusCode.Aborted, string.Join(' ', errors.Select(x => x.Message))));

        if (errors.Exists(x => x is AlreadyExists))
            throw new RpcException(new Status(StatusCode.InvalidArgument,
                string.Join(' ', errors.Select(x => x.Message))));
        
        throw new RpcException(new Status(StatusCode.InvalidArgument, string.Join(' ', errors.Select(x => x.Message))));
    }
}