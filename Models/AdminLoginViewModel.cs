using System.ComponentModel.DataAnnotations;

namespace sav.Models
{
    public class AdminLoginViewModel
    {
        [Required(ErrorMessage = "Le nom d'utilisateur est obligatoire")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Le mot de passe est obligatoire")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

}
