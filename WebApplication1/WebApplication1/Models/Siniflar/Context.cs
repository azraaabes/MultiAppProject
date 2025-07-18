using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Web;
using System.Data.Entity;

using System.Data;

namespace WebApplication1.Models.Siniflar
{
    public class Context : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Log> Logs { get; set; }

        public DbSet<Role> Roles { get; set; }

    }
}