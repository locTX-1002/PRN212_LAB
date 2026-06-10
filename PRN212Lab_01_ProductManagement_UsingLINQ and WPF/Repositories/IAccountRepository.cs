using System.Collections.Generic;
using BusinessObjects;

namespace Repositories
{
    public interface IAccountRepository
    {
        AccountMember? GetAccountById(string loginInput);
        List<AccountMember> GetAccounts(); // Thêm mới
        void SaveAccount(AccountMember acc); // Thêm mới
        void UpdateAccount(AccountMember acc); // Thêm mới
        void DeleteAccount(AccountMember acc); // Thêm mới
    }
}