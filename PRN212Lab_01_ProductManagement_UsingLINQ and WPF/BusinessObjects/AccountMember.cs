using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects
{
    public class AccountMember
    {
        [Key]
        public string MemberId { get; set; } = null!;

        [MaxLength(200)]
        public string MemberPassword { get; set; } = null!;

        public string FullName { get; set; } = null!;

        [EmailAddress]
        public string? EmailAddress { get; set; }

        public Role? MemberRole { get; set; }

        // Lockout
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}
