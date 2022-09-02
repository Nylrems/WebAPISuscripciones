namespace WebAPIAutores.DTOs
{
    public class LlaveDTO
    {
        public int id {get; set;}
        public string Llave { get; set; }
        public bool Activa { get; set; }
        public string TipoLlave { get; set; }
        public List<RestriccionDominioDTO> restriccionDominioDTOs {get; set;}
        public List<RestriccionIPDTO> restriccionIPDTOs {get; set;} 
    }
}