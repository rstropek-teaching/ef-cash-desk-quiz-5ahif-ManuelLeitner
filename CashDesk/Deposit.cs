using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashDesk {
    public class Deposit : IDeposit {
        public int Id { get; set; }

        [Required]
        public Membership Membership { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal Amount { get; set; }

        [NotMapped]
        IMembership IDeposit.Membership { get => Membership; }
    }
}
