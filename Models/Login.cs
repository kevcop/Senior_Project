using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Senior_Project.Models
{
    public class Login
    {
        public int Id { get; set; }
        
        
        [Required]
        [EmailAddress]
        public string? Email { get; set;}

        [Required]
        //[RegularExpression(@"")] TO IMPLEMENT LATER
        public string? Password { get; set;}
    }
}
