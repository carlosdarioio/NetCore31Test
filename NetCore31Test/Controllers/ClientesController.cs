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
    public class ClientesController : ControllerBase
    {
        private readonly InventoryContext _context;

        public ClientesController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> GetCliente(int? FROM, int LIMIT = 0)
        {
            
            var cliente = _context.Cliente.AsQueryable();

            if (FROM != null)
            {                
                cliente = cliente.Where(i => i.Id > FROM);
            }

            if (LIMIT > 0)
            {                
                cliente = cliente.Take(LIMIT);
            }

            return await cliente.ToListAsync();
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetCliente(int id,DateTime? fecha)
        {
            
            var cliente = await _context.Cliente.FindAsync(id);
            var poliza =  _context.Poliza.Where(t=>t.ClienteId==id).ToList();
            
            decimal totalLPS=0;
            if (cliente == null)
            {                
                return new JsonResult(new { success = 0, errorMsg = "Client Not Found", data = ""});
            }

            if (fecha != null)
            {
                var tasacambio = _context.Set<Tasacambio>().Where(t => t.FechaInicio == fecha).ToList();
                //return new JsonResult(new { success = 0, errorMsg = "", data = tasacambio });
                if (tasacambio.Count()> 0)
                {
                    totalLPS = poliza[0].SumaAsegurada * tasacambio[0].Tasa;
                }
                else {
                    return new JsonResult(new { success = 0, errorMsg = "Taza De Cambio no existe en la fecha establecida", data = "" });
                }
            }
            
            return new JsonResult(new { success = 1, errorMsg="",data=cliente, TotalLPS= totalLPS });
        }

        // PUT: api/Clientes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Cliente cliente)
        {
            if (id != cliente.Id)
            {
                //return BadRequest();
                return new JsonResult(new { success = 0, errorMsg = "Client Not Found", data = "" });
            }

            _context.Entry(cliente).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                return new JsonResult(new { success = 1, errorMsg = "", data = cliente });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(id))
                {                    
                    return new JsonResult(new { success = 0, errorMsg = "Client Not Found", data = "" });
                }
                else
                {                    
                    return new JsonResult(new { success = 0, errorMsg = "Throw", data = "" });
                }
            }

            //return NoContent();
        }

        // POST: api/Clientes
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Cliente>> PostCliente(Cliente cliente)
        {
            if (cliente.Nombre == null || cliente.Nombre.Length<=0)
            {
                return new JsonResult(new { success = 0, errorMsg = "Rellenar todos los datos", data = "" });
            }
            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = 1, errorMsg = "", data = cliente });
            //return CreatedAtAction("GetCliente", new { id = cliente.Id }, cliente);
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Cliente>> DeleteCliente(int id)
        {
            var cliente = await _context.Cliente.FindAsync(id);
            if (cliente == null)
            {                
                return new JsonResult(new { success = 0, errorMsg = "Client Not Found", data = "" });
            }

            _context.Cliente.Remove(cliente);
            await _context.SaveChangesAsync();
            
            return new JsonResult(new { success = 1, errorMsg = "", data = cliente });
        }

        private bool ClienteExists(int id)
        {
            return _context.Cliente.Any(e => e.Id == id);
        }
    }
}
