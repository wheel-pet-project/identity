namespace Application.UseCases.Account.UpdatePassword;

public class UpdateAccountPasswordRequest(string accessToken, 
    string oldPassword, string newPassword)
{
    public string AccessToken { get; init; } = accessToken;
    
    public string OldPassword { get; init; } = oldPassword;
    
    public string NewPassword { get; init; } = newPassword;
}