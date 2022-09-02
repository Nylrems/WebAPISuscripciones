using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebAPIAutores.DTOs;
using WebAPIAutores.Servicios;
using WebAPIAutores.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/llavesapi")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class LlavesAPIController : CustomBaseController
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly ServicioLlaves servicioLlaves;

        public LlavesAPIController(ApplicationDbContext context,
        IMapper mapper, ServicioLlaves servicioLlaves)
        {
            this.context = context;
            this.mapper = mapper;
            this.servicioLlaves = servicioLlaves;
        }

        [HttpGet]
        public async Task<List<LlaveDTO>> MisLlaves()
        {
            var usuarioId = ObtenerUsuarioId();
            var llaves = await context.LlaveAPIs
            .Include(x => x.RestriccionesDominio)
            .Include(x => x.RestriccionesIP).Where(x => x.usuarioId == usuarioId).ToListAsync();
            return mapper.Map<List<LlaveDTO>>(llaves);
        }

        [HttpPost]
        public async Task<ActionResult> CrearLlave(CrearLlaveDTO crearLlaveDTO)
        {
            var usuarioId = ObtenerUsuarioId();

            if (crearLlaveDTO.TipoLlave == Entidades.TipoLlave.Gratuita)
            {
                var elUsuarioYaTieneUnaLlaveGratuita = await context.LlaveAPIs
                    .AnyAsync(x => x.usuarioId == usuarioId && x.TipoLlave == Entidades.TipoLlave.Gratuita);

                if (elUsuarioYaTieneUnaLlaveGratuita)
                {
                    return BadRequest("El usuario ya tiene una llave gratuita");
                }
            }

            await servicioLlaves.CrearLlaves(usuarioId, crearLlaveDTO.TipoLlave);
            return NoContent();

        }

        [HttpPut]
        public async Task<ActionResult> ActualizarLlave(ActualizarLlaveDTO actualizarLlaveDTO)
        {
            var usuarioId = ObtenerUsuarioId();

            var llaveDB = await context.LlaveAPIs.FirstOrDefaultAsync(x => x.Id == actualizarLlaveDTO.LlaveId);

            if (llaveDB == null)
            {
                return NotFound();
            }

            if (usuarioId != llaveDB.usuarioId)
            {
                return Forbid();
            }

            if (actualizarLlaveDTO.ActualizarLlave)
            {
                llaveDB.Llave = servicioLlaves.GenerarLlave();
            }

            llaveDB.Activa = actualizarLlaveDTO.Activa;
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}