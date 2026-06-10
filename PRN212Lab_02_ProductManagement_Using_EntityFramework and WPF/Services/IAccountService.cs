using BusinessObjects;

namespace Services;

public interface IAccountService
{
    AccountMember? GetAccountById(string accountID);
}
