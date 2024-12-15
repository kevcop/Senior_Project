using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
namespace Senior_Project.Models;
using Microsoft.EntityFrameworkCore;
using System.Web;
/// <summary>
/// Database to hold user information 
/// </summary>
public class Register
{
    // Unique identifier 
    [Key] // Primary key for this table

    public int Id { get; set; }
    // First name of a user 
    [StringLength(60, MinimumLength = 5)]
    [Required]
    [Display(Name = "First Name")]

    public string? firstName { get; set; }
    // Last name of a user 
    [Required]
    [Display(Name = "Last Name")]
     public string? lastName { get; set; }

    // The email address of a suer 
    [RegularExpression(@"^[a-zA-Z0-9._%±]+@[a-zA-Z0-9.-]+.[a-zA-Z]{2,}$")]
    [Display(Name = "Email Address")]
    [Required]

    public string ?emailAddress { get; set; }
    // The phone number of a user 
    [Required]
    [Display(Name = "Phone Number ")]
    public string? phoneNumber { get; set; }
    // A user's birthdate
    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Birth date")]

    public string? birthdate { get; set; }
    // A user's chosen username 
    [Required]
    [StringLength(30, MinimumLength = 1)]
    [RegularExpression("^[a-zA-Z0-9]+$")]
    public string? username { get; set; }
    // The user's chosen password 
    [Required]
    [DataType(DataType.Password)]
    
    [RegularExpression("^[a-zA-Z0-9]+$")]
    public string? password { get; set; }    

    //PLACEHOLDER, COULD ASK USERS WHAT THEY ARE INTERESTED IN AND DISPLAY CHOICES AFTER THE SIGN UP HAS BEEN COMPLETE 
    public string? interests {  get; set;}

    public string? maybe {  get; set; }
}
