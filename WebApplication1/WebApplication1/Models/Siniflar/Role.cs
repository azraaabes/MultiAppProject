using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Siniflar
{
    [Table("Roless")]
    public class Role
    {
        [Key]
        public int Roleid { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(30)]
        public string RoleName { get; set; }

        public ICollection<Employee> Employees { get; set; }

    }
}