using System;
using Xunit;

namespace CashDesk.Tests {
    public class TestInitialization {
        [Fact]
        public void MultipleInitializations() {
            using (var dal = new DataAccess()) {
                dal.InitializeDatabase();
                Assert.Throws<InvalidOperationException>(() => dal.InitializeDatabase());
            }
        }

        [Fact]
        public void NoInitialization() {
            using (var dal = new DataAccess()) {
                Assert.ThrowsAsync<InvalidOperationException>(async () => await dal.AddMemberAsync("A", "B", DateTime.Today));
                Assert.ThrowsAsync<InvalidOperationException>(async () => await dal.DeleteMemberAsync(0));
                Assert.ThrowsAsync<InvalidOperationException>(async () => await dal.JoinMemberAsync(0));
                Assert.ThrowsAsync<InvalidOperationException>(async () => await dal.CancelMembershipAsync(0));
                Assert.ThrowsAsync<InvalidOperationException>(async () => await dal.DepositAsync(0, 1M));
                Assert.Throws<InvalidOperationException>(() => dal.GetDepositStatistics());
            }
        }
    }
}
