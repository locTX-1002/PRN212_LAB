using BusinessObjects;
using System.Collections.Generic;
using System.Linq;

namespace DataAccessLayer
{
    public class AccountDAO
    {
        public static AccountMember? GetAccountById(string loginInput)
        {
            using var db = new MyDbContext();
            return db.AccountMembers
                .FirstOrDefault(a => a.MemberId == loginInput || a.EmailAddress == loginInput);
        }

        public static List<AccountMember> GetAccounts()
        {
            using var db = new MyDbContext();
            return db.AccountMembers.ToList();
        }

        public static void SaveAccount(AccountMember acc)
        {
            using var db = new MyDbContext();
            db.AccountMembers.Add(acc);
            db.SaveChanges();
        }

        public static void UpdateAccount(AccountMember acc)
        {
            using var db = new MyDbContext();
            var existing = db.AccountMembers.Find(acc.MemberId);
            if (existing == null) return;
            existing.MemberPassword = acc.MemberPassword;
            existing.FullName = acc.FullName;
            existing.EmailAddress = acc.EmailAddress;
            existing.MemberRole = acc.MemberRole;
            existing.FailedAttempts = acc.FailedAttempts;
            existing.LockedUntil = acc.LockedUntil;
            db.SaveChanges();
        }

        public static void DeleteAccount(AccountMember acc)
        {
            using var db = new MyDbContext();
            var existing = db.AccountMembers.Find(acc.MemberId);
            if (existing == null) return;
            db.AccountMembers.Remove(existing);
            db.SaveChanges();
        }
    }
}
