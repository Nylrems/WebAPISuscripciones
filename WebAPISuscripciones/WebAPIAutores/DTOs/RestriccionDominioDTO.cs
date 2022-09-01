namespace WebAPIAutores.DTOs
{
    public class RestriccionDominioDTO
    {
        public int Id { get; set; }
        public string Dominio { get; set; }
        public List<RestriccionDominioDTO> restriccionDominio {get; set;}
    }
}