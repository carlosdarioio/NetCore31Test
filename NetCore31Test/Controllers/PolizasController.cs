using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCore31Test.Models;

namespace NetCore31Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PolizasController : ControllerBase
    {
        private readonly InventoryContext _context;

        public PolizasController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/Polizas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Poliza>>> GetPoliza(int? FROM, int LIMIT = 0)
        {
            var poliza = _context.Poliza.AsQueryable();
            if (FROM != null)
            {
                poliza = poliza.Where(i => i.Id > FROM);
            }

            if (LIMIT > 0)
            {
                poliza = poliza.Take(LIMIT);
            }

            return await poliza.ToListAsync();
        }

        // GET: api/Polizas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Poliza>> GetPoliza(int id)
        {
            var poliza = await _context.Poliza.FindAsync(id);

            if (poliza == null)
            {
                return new JsonResult(new { success = 0, errorMsg = "Poliza No Encontrada", data = "" });
            }

            return new JsonResult(new { success = 1, errorMsg = "", data = poliza });
        }

        // PUT: api/Polizas/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPoliza(int id, Poliza poliza)
        {
            if (id != poliza.Id)
            {                
                return new JsonResult(new { success = 0, errorMsg = "Solicitud incorrecta", data = "" });
            }

            _context.Entry(poliza).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = 1, errorMsg = "", data = poliza });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PolizaExists(id))
                {
                    return new JsonResult(new { success = 0, errorMsg = "Poliza No Encontrada", data = "" });
                }
                else
                {
                    return new JsonResult(new { success = 0, errorMsg = "Throw", data = "" });
                }
            }
            
        }

        // POST: api/Polizas
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Poliza>> PostPoliza(Poliza poliza)
        {
            //Emular Required ClienteId,Mondeda,SumaAsegurada
            if (poliza.ClienteId <=0 || poliza.Mondeda == null || poliza.SumaAsegurada <= 0)
            {
                return new JsonResult(new { success = 0, errorMsg = "Rellenar todos los datos", data = "" });
            }            
            
            _context.Poliza.Add(poliza);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = 1, errorMsg = "", data = poliza });            
        }

        // DELETE: api/Polizas/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Poliza>> DeletePoliza(int id)
        {
            var poliza = await _context.Poliza.FindAsync(id);
            if (poliza == null)
            {                
                return new JsonResult(new { success = 0, errorMsg = "Poliza No Encontrada", data = NotFound() });
            }

            _context.Poliza.Remove(poliza);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = 1, errorMsg = "", data = poliza });            
        }

        //funcion para valida id de poliza
        private bool PolizaExists(int id)
        {
            return _context.Poliza.Any(e => e.Id == id);
        }
    }
}
