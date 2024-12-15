using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Senior_Project.Models
{
    /// <summary>
    /// Database for holding login credentials 
    /// </summary>
    public class Login
    {
        // Unique identifer of a login credential 
        public int Id { get; set; }
        
        // Email credential 
        [Required]
        [EmailAddress]
        public string? Email { get; set;}
        // Password credential 
        [Required]
        public string? Password { get; set;}
    }
}
