using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashDesk {
    /// <inheritdoc />
    public class DataAccess : IDataAccess {
        public MemberContext MemberContext { get; private set; }

        /// <inheritdoc />
        public void InitializeDatabase() {
            if (MemberContext == null) {
                MemberContext = new MemberContext();
            } else
                throw new InvalidOperationException($"{nameof(InitializeDatabase)} has already been called");
        }

        private void EnsureInitialized() {
            if (MemberContext == null)
                throw new InvalidOperationException($"{nameof(InitializeDatabase)} has not been called before");
        }

        private async Task<Member> GetMemberAsync(int memberNumber) {

            var member = await MemberContext.Members.FindAsync(memberNumber);
            if (member == null)
                throw new ArgumentException($"Unknown {nameof(memberNumber)}");

            return member;
        }

        private async Task<Membership> GetMembershipAsync(int memberNumber) {

            var memberhip = await MemberContext.Memberships.FirstOrDefaultAsync(m => m.Member.MemberNumber == memberNumber);
            if (memberhip == null)
                throw new NoMemberException("The member is currently not an active member.");
            return memberhip;
        }

        /// <inheritdoc />
        /// <remarks>
        /// <see cref="DuplicateNameException"/> hasn't been delt with the way should be.
        /// </remarks>
        public async Task<int> AddMemberAsync(string firstName, string lastName, DateTime birthday) {
            EnsureInitialized();
            EntityEntry<Member> next;
            try {
                next = MemberContext.Add(new Member() { FirstName = firstName, LastName = lastName, Birthday = birthday });
                await MemberContext.SaveChangesAsync();
            } catch (InvalidOperationException) {
                throw new DuplicateNameException("Member with the same last name already exists.");
            }
            await next.ReloadAsync();
            return next.Entity.MemberNumber;
        }

        /// <inheritdoc />
        public async Task DeleteMemberAsync(int memberNumber) {
            EnsureInitialized();
            MemberContext.Members.Remove(await MemberContext.Members.FindAsync(memberNumber));
        }

        /// <inheritdoc />
        public async Task<IMembership> JoinMemberAsync(int memberNumber) {
            EnsureInitialized();

            var runningMembershipsCount = MemberContext.Memberships.Where(m => m.End == null || m.End > DateTime.Now).Count();
            if (runningMembershipsCount > 0)
                throw new AlreadyMemberException("The member is already an active member.");

            var next = MemberContext.Add(new Membership() { Member = await GetMemberAsync(memberNumber), Begin = DateTime.Now });
            await MemberContext.SaveChangesAsync();
            await next.ReloadAsync();
            return next.Entity;
        }

        /// <inheritdoc />
        public async Task<IMembership> CancelMembershipAsync(int memberNumber) {
            EnsureInitialized();
            var membership = await GetMembershipAsync(memberNumber);
            membership.End = DateTime.Now;
            await MemberContext.SaveChangesAsync();
            return membership;
        }

        /// <inheritdoc />
        /// <remarks>
        /// The check for amount is implemented on the db.
        /// A mapping to <see cref="ArgumentException"/> hasn't been implemented yet.
        /// </remarks>
        public async Task DepositAsync(int memberNumber, decimal amount) {
            EnsureInitialized();
            var membership = await GetMembershipAsync(memberNumber);

            await MemberContext.Deposits.AddAsync(new Deposit() { Membership = membership, Amount = amount });

            await MemberContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public IEnumerable<IDepositStatistics> GetDepositStatistics() {
            EnsureInitialized();
            return MemberContext.Deposits
                .GroupBy(d => new { year = d.Membership.Begin.Year, member = d.Membership.Member })
                .Select(v => new DepositStatistics() { Year = v.Key.year, TotalAmount = v.Sum(d => d.Amount), Member = v.Key.member });
        }

        /// <inheritdoc />
        public void Dispose() { }
    }
}
