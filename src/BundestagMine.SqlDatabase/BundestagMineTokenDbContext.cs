using BundestagMine.Models.Database.MongoDB;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BundestagMine.SqlDatabase
{
    public class BundestagMineTokenDbContext : DbContext
    {
        public BundestagMineTokenDbContext(DbContextOptions<BundestagMineTokenDbContext> options) : base(options)
        {

        }

        public DbSet<Token> Token { get; set; }
    }
}
