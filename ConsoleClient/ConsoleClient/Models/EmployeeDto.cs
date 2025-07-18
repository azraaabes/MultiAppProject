using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleClient.Models
{
    internal class EmployeeDto
    {
        [JsonProperty("employeeid")]
        public int EmployeeId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("surname")]
        public string Surname { get; set; }

        [JsonProperty("employeemail")]
        public string EmployeeMail { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("roleid")]
        public int RoleId { get; set; }

        [JsonProperty("role")]
        public RoleDto Role { get; set; }
    }
}

