
using System.ComponentModel.DataAnnotations;

namespace Kabutar_WPF.ViewModels;

public class ForgotePasswordViewModel
{
    [Required]
    public string Password { get; set; }

    [Required]
    public string ConfirmPassword { get; set; }
}
