using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaveRadar.Types
{
    [Table("Subscriptions")]
    public class Subscription
    {
        [MaxLength(32)]
        public string Name { get; set; }

        [Key]
        [MaxLength(32)]
        public string Email { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime DateSubscribed { get; set; }
    }
}