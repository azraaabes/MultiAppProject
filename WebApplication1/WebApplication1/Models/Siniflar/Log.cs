using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication1.Models.Siniflar
{
    public class Log
    {
        [Key]
        public int LogId { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(30)]
        public string Title { get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(50)]
        public string Message { get; set; }

        public DateTime Tarih { get; set; }

        public int? Employeeid { get; set; }
        public string User {  get; set; }

        [Column(TypeName = "Varchar")]
        [StringLength(30)]
        public string State { get; set; }

        [ForeignKey("Employeeid")]
        public virtual Employee Employee { get; set; }
    }
}