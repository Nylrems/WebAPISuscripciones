using Microsoft.EntityFrameworkCore;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Middlewares
{
    public static class LimitarPeticionesMiddlewareExtensions
    {
        public static IApplicationBuilder UseLimitarPeticiones(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LimitarPeticionesMiddleware>();
        }
    }

    public class LimitarPeticionesMiddleware
    {
        private readonly RequestDelegate siguiente;
        private readonly IConfiguration configuration;

        public LimitarPeticionesMiddleware(RequestDelegate siguiente, IConfiguration configuration)
        {
            this.siguiente = siguiente;
            this.configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext httpContext, ApplicationDbContext context)
        {
            var limitarPeticionesConfiguracion = new LimitarPeticionesConfiguracion();
            configuration.GetRequiredSection("limitarPeticiones").Bind(limitarPeticionesConfiguracion);

            var ruta = httpContext.Request.Path.ToString();
            var estaLaRutaEnListaBlanca = limitarPeticionesConfiguracion.ListaBlancaRutas.Any(x => ruta.Contains(x));

            if (estaLaRutaEnListaBlanca)
            {
                await siguiente(httpContext);
                return;
            }

            var llavesStringValues = httpContext.Request.Headers["X-Api-Key"];

            if (llavesStringValues.Count == 0)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Debe proveer la llave en la cabecera X-Api-Key");
                return;
            }

            if (llavesStringValues.Count > 1)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("Sólo debe estar una llave presente");
                return;
            }
            var llave = llavesStringValues[0];

            var llaveDB = await context.LlaveAPIs
            .Include(x => x.RestriccionesDominio)
            .Include(x => x.RestriccionesIP)
            .FirstOrDefaultAsync(x => x.Llave == llave);

            if (llaveDB == null)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave no existe");
                return;
            }

            if (!llaveDB.Activa)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync("La llave no existe");
                return;
            }

            if (llaveDB.TipoLlave == TipoLlave.Gratuita)
            {
                var hoy = DateTime.Today;
                var manana = hoy.AddDays(1);
                var cantidadDePeticionesRealizadasHoy = await context.Peticiones
                .CountAsync(x => x.LlaveId == llaveDB.Id && x.FechaPeticion >= hoy && x.FechaPeticion < manana);

                if (cantidadDePeticionesRealizadasHoy >= limitarPeticionesConfiguracion.PeticionesPorDiaGratuito)
                {
                    httpContext.Response.StatusCode = 429;
                    await httpContext.Response.WriteAsync("Has excedido el límite de  peticiones diarias. Si quieres hacer más peticiones, actualizar tu cuenta a premium");
                    return;
                }
            }

            var superaRestricciones = PeticionSuperaAlgunaDeLasPeticiones(llaveDB, httpContext);

            if (!superaRestricciones)
            {
                httpContext.Response.StatusCode = 403;
                return;
            }

            var peticion = new Peticion() { LlaveId = llaveDB.Id, FechaPeticion = DateTime.UtcNow };
            context.Add(peticion);
            await context.SaveChangesAsync();

            await siguiente(httpContext);
        }

        private bool PeticionSuperaAlgunaDeLasPeticiones(LlaveAPI llaveAPI, HttpContext httpContext)
        {
            var hayRestricciones = llaveAPI.RestriccionesDominio.Any() || llaveAPI.RestriccionesIP.Any();

            if (!hayRestricciones)
            {
                return true;
            }

            var peticionSuperaLasRestriccionesDeDominio =
                    PeticionSuperaLasRestriccionesDeDominio(llaveAPI.RestriccionesDominio, httpContext);

            var peticionSuperaLasRestriccionesDeIP = 
                    PeticionSuperaLasRestriccionesDeIP(llaveAPI.RestriccionesIP, httpContext);

            return peticionSuperaLasRestriccionesDeDominio || peticionSuperaLasRestriccionesDeIP;
        }

        private bool PeticionSuperaLasRestriccionesDeIP(List<RestriccionIP> restricciones, HttpContext httpContext)
        {
            if (restricciones == null || restricciones.Count == 0)
            {
                return false;
            }

            var IP = httpContext.Connection.RemoteIpAddress.ToString();

            if (IP == string.Empty)
            {
                return false;
            }

            var superaRestricciones = restricciones.Any(x => x.IP == IP);
            return superaRestricciones;  
        }

        private bool PeticionSuperaLasRestriccionesDeDominio(List<RestriccionDominio> restricciones, HttpContext httpContext)
        {
            if (restricciones == null || restricciones.Count == 0)
            {
                return false;
            }
            var referer = httpContext.Request.Headers["Referer"].ToString();

            if (referer == string.Empty)
            {
                return false;
            }

            Uri myUri = new Uri(referer);
            string host = myUri.Host;

            var superRestriccion = restricciones.Any(x => x.Dominio == host);
            return superRestriccion;
        }
    }
}