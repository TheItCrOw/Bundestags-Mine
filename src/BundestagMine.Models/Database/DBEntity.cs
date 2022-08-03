using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BundestagMine.Models.Database
{
    public class DBEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}
