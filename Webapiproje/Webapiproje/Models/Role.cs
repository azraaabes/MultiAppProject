using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Text.Json.Serialization;
namespace Webapiproje.Models
{

    [Table("Roless")]
    public class Role {
         
            public int Roleid { get; set; }

            public string RoleName { get; set; }

        [JsonIgnore]
        public ICollection<Employee>? Employees { get; set; }
        
        }
}
