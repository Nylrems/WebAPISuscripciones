using System.ComponentModel.DataAnnotations;

namespace WebAPIAutores.DTOs
{
    public class ActualizarRestriccionesDominioDTO
    {
        [Required]
        public string Dominio { get; set; }
    }
}