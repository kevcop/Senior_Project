using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


//REDESIGN STRUCTURE AND FLOW TO USE THE LOGIN PAGE AS THE LANDING PAGE 
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
