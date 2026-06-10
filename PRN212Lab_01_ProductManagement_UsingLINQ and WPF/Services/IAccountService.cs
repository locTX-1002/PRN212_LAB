using System.Collections.Generic;
using BusinessObjects;

namespace Services
{
    public enum LoginResult
    {
        Success,
        InvalidCredentials,
        NoPermission,
        LockedOut
    }

    public class LoginOutcome
    {
        public LoginResult Result { get; init; }
        public AccountMember? Account { get; init; }
        public int RemainingLockSeconds { get; init; }
    }

    public interface IAccountService
    {
        AccountMember? GetAccountById(string loginInput);
        List<AccountMember> GetAccounts();
        void SaveAccount(AccountMember acc);
        void UpdateAccount(AccountMember acc);
        void DeleteAccount(AccountMember acc);

        LoginOutcome AttemptLogin(string login, string password, Role? requiredRole = null);
        bool ChangePassword(string memberId, string oldPassword, string newPassword);
    }
}
