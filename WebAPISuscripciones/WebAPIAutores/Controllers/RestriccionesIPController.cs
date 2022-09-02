using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAutores.DTOs;
using WebAPIAutores.Entidades;

namespace WebAPIAutores.Controllers
{
    [ApiController]
    [Route("api/restriccionesip")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RestriccionesIPController : CustomBaseController
    {
        private readonly ApplicationDbContext context;

        public RestriccionesIPController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult> Post(CrearRestriccionIPDTO crearRestriccionIPDTO)
        {
            var llaveDB = await context.LlaveAPIs.FirstOrDefaultAsync(x => x.Id == crearRestriccionIPDTO.LaveId);

            if (llaveDB == null)
            {
                return NotFound();
            }

            var UsuarioId = ObtenerUsuarioId();

            if (llaveDB.usuarioId != UsuarioId)
            {
                return Forbid();
            }

            var restriccionIP = new RestriccionIP
            {
                LlaveId = llaveDB.Id,
                IP = crearRestriccionIPDTO.IP
            };

            context.Add(restriccionIP);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, ActualizarRestriccionIPDTO actualizarRestriccionIPDTO)
        {
            var restriccionDB = await context.RestriccionesIP
                .Include(x => x.Llave).FirstOrDefaultAsync();

            if (restriccionDB == null)
            {
                return NotFound();
            }

            var usuarioId = ObtenerUsuarioId();

            if (restriccionDB.Llave.usuarioId != usuarioId)
            {
                return Forbid();
            }

            restriccionDB.IP = actualizarRestriccionIPDTO.IP;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var restriccionDB = await context.RestriccionesIP.Include(x => x.Llave).FirstOrDefaultAsync(x => x.Id == id);

            if (restriccionDB == null)
            {
                return NotFound();
            }

            var usuarioId = ObtenerUsuarioId();

            if (usuarioId != restriccionDB.Llave.usuarioId) 
            {
                return Forbid();
            }

            context.Remove(restriccionDB);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}