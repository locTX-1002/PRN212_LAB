using System.Collections.Generic;
using BusinessObjects;
using DataAccessLayer;

namespace Repositories
{
    public class AccountRepository : IAccountRepository
    {
        public AccountMember? GetAccountById(string loginInput) => AccountDAO.GetAccountById(loginInput);
        public List<AccountMember> GetAccounts() => AccountDAO.GetAccounts();
        public void SaveAccount(AccountMember acc) => AccountDAO.SaveAccount(acc);
        public void UpdateAccount(AccountMember acc) => AccountDAO.UpdateAccount(acc);
        public void DeleteAccount(AccountMember acc) => AccountDAO.DeleteAccount(acc);
    }
}