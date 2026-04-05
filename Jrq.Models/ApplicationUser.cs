using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Jrq.Models
{
    public class ApplicationUser:IdentityUser{
        [Required]
        public string Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public int? CompanyID { get; set; }
        [ForeignKey("CompanyID")]
        [ValidateNever]
        public Company Company { get; set; }
        //public string? CompanyName { get; set; }
        //public int? CompanyId { get; set; }
        //public virtual Company? Company { get; set; }
    }
}
