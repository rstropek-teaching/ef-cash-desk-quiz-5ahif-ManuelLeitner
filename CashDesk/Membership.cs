using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashDesk {
    public class Membership : IMembership {

        public int Id { get; set; }

        [Required]
        public Member Member { get; set; }

        [Required]
        public DateTime Begin { get; set; }

        private DateTime? end;
        public DateTime? End {
            get => end; set {
                if (value != null) {
                    if (value > Begin) {
                        end = value;
                    } else
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [NotMapped]
        IMember IMembership.Member { get => Member; }

    }
}
