using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
namespace Senior_Project.Models
{
    public class Register
    {
        public int Id { get; set; }

        [StringLength(60, MinimumLength = 5)]
        
        [Display(Name = "First Name")]

        public string? firstName { get; set; }

        [Display(Name = "Last Name")]
         public string? lastName { get; set; }

        
        [RegularExpression(@"^[a-zA-Z0-9._%±]+@[a-zA-Z0-9.-]+.[a-zA-Z]{2,}$")]
        [Display(Name = "Email Address")]

        public string ?emailAddress { get; set; }
        
        [Display(Name = "Phone Number ")]
        public string? phoneNumber { get; set; }

        
        [DataType(DataType.Date)]
        [Display(Name = "Birth date")]

        public string? birthdate { get; set; }

        
        [StringLength(30, MinimumLength = 1)]
        [RegularExpression("^[a-zA-Z0-9]+$")]
        public string? username { get; set; }

        
        [DataType(DataType.Password)]
        [RegularExpression("^[a-zA-Z0-9]+$")]
        public string? password { get; set; }    

        //PLACEHOLDER, COULD ASK USERS WHAT THEY ARE INTERESTED IN AND DISPLAY CHOICES AFTER THE SIGN UP HAS BEEN COMPLETE 
        public string? interests {  get; set;}
    }


}
