using System;
using System.ComponentModel.DataAnnotations;

namespace project1.ViewModels
{
	public class RegisterViewModel
	{
		[Required(ErrorMessage = "Name is required.")]
		public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(40, MinimumLength = 8, ErrorMessage ="The {0} must be at {2} and at max {1} characters long")]
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Compare("ConfirmPassword", ErrorMessage = "Password does not match.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }

	}
}

