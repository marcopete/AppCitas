using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        //Propiedad de campo que requiere valor, largo minimo de campo
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(8, MinimumLength = 4, ErrorMessage = "tu contraseña debe ser entre 4 y 8")]
        public string Password { get; set; }
    }
}