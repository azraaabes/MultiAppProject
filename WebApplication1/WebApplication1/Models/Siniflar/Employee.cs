using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Siniflar
{
    public class Employee
    {
        [Key]
        public int Employeeid { get; set; }

        [Column(TypeName ="Varchar")]
        [StringLength(30)]
        public string Name { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(30)]
        public string Surname { get; set; }

        [Column(TypeName ="Varchar")]
        [StringLength(50)]
        public string Employeemail { get; set; }

        [Column(TypeName ="Varchar")]
        [StringLength(10)]
        public string Username { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(100)]
        public string Password { get; set; }

        public int Roleid { get; set; }

        [ForeignKey("Roleid")]
        public virtual Role Role { get; set; }
    }
}