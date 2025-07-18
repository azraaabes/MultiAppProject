using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Webapiproje.Models
{
    
        [Table("Employees")] // MVC projenin tablosu da buysa aynı kalmalı
        public class Employee
        {
            [Key]
            public int Employeeid { get; set; }

            [Column(TypeName = "Varchar")]
            [StringLength(30)]
            public string Name { get; set; }

            [Column(TypeName = "Varchar")]
            [StringLength(30)]
            public string Surname { get; set; }

            [Column(TypeName = "Varchar")]
            [StringLength(50)]
            public string Employeemail { get; set; }

            [Column(TypeName = "Varchar")]
            [StringLength(10)]
            public string Username { get; set; }

            [Column(TypeName = "Varchar")]
            [StringLength(100)]
            public string Password { get; set; }

            // Foreign key
            public int Roleid { get; set; }

            [ForeignKey("Roleid")]
            public Role Role { get; set; }
        }
}
