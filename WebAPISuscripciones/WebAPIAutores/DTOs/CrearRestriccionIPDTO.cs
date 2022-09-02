using System.ComponentModel.DataAnnotations;

namespace WebAPIAutores.DTOs
{
    public class CrearRestriccionIPDTO
    {
        public int LaveId { get; set; }
        [Required]
        public string IP { get; set; }
    }
}