using System;
using System.Collections.Generic;
using BusinessObjects;
using Repositories;

namespace Services
{
    public class AccountService : IAccountService
    {
        private const int MaxFailedAttempts = 5;
        private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(5);

        private readonly IAccountRepository repo;

        public AccountService() : this(new AccountRepository()) { }
        public AccountService(IAccountRepository accountRepository) { repo = accountRepository; }

        public AccountMember? GetAccountById(string loginInput) => repo.GetAccountById(loginInput);
        public List<AccountMember> GetAccounts() => repo.GetAccounts();
        public void SaveAccount(AccountMember acc) => repo.SaveAccount(acc);
        public void UpdateAccount(AccountMember acc) => repo.UpdateAccount(acc);
        public void DeleteAccount(AccountMember acc) => repo.DeleteAccount(acc);

        public LoginOutcome AttemptLogin(string login, string password, Role? requiredRole = null)
        {
            var account = repo.GetAccountById(login);
            if (account == null)
                return new LoginOutcome { Result = LoginResult.InvalidCredentials };

            // Locked?
            if (account.LockedUntil.HasValue && account.LockedUntil.Value > DateTime.Now)
            {
                var remaining = (int)(account.LockedUntil.Value - DateTime.Now).TotalSeconds;
                return new LoginOutcome { Result = LoginResult.LockedOut, RemainingLockSeconds = remaining };
            }

            if (!PasswordHasher.Verify(password, account.MemberPassword))
            {
                account.FailedAttempts++;
                if (account.FailedAttempts >= MaxFailedAttempts)
                {
                    account.LockedUntil = DateTime.Now.Add(LockDuration);
                    account.FailedAttempts = 0;
                }
                repo.UpdateAccount(account);
                return new LoginOutcome { Result = LoginResult.InvalidCredentials };
            }

            // Auto-upgrade plain → hash
            if (!PasswordHasher.IsHashed(account.MemberPassword))
                account.MemberPassword = PasswordHasher.Hash(password);

            // Reset failure tracking
            account.FailedAttempts = 0;
            account.LockedUntil = null;
            repo.UpdateAccount(account);

            if (requiredRole.HasValue && account.MemberRole != requiredRole.Value)
                return new LoginOutcome { Result = LoginResult.NoPermission, Account = account };

            return new LoginOutcome { Result = LoginResult.Success, Account = account };
        }

        public bool ChangePassword(string memberId, string oldPassword, string newPassword)
        {
            var account = repo.GetAccountById(memberId);
            if (account == null) return false;
            if (!PasswordHasher.Verify(oldPassword, account.MemberPassword)) return false;

            account.MemberPassword = PasswordHasher.Hash(newPassword);
            repo.UpdateAccount(account);
            return true;
        }
    }
}
