using WebAPIAutores.Entidades;

namespace WebAPIAutores.Servicios
{
    public class ServicioLlaves
    {
        private readonly ApplicationDbContext context;

        public ServicioLlaves(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task CrearLlaves(string usuarioId, TipoLlave tipoLlave)
        {
            var llave = Guid.NewGuid().ToString().Replace("-", "");

            var llaveAPI = new LlaveAPI
            {
                Activa = true,
                Llave = llave,
                TipoLlave = tipoLlave,
                usuarioId = usuarioId
            };

            context.Add(llaveAPI);
            await context.SaveChangesAsync();
        }
    }
}