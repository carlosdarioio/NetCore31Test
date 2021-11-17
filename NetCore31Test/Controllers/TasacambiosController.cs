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
    public class TasacambiosController : ControllerBase
    {
        private readonly InventoryContext _context;

        public TasacambiosController(InventoryContext context)
        {
            _context = context;
        }

        // GET: api/Tasacambios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Tasacambio>>> GetTasacambio()
        {
            return await _context.Tasacambio.ToListAsync();
        }

        // GET: api/Tasacambios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Tasacambio>> GetTasacambio(int id)
        {
            var tasacambio = await _context.Tasacambio.FindAsync(id);

            if (tasacambio == null)
            {                
                return new JsonResult(new { success = 0, errorMsg = "TasaDeCambios No Encontrada", data = NotFound() });
            }

            return tasacambio;
        }

        // PUT: api/Tasacambios/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTasacambio(int id, Tasacambio tasacambio)
        {            

            //------------------------------------------ VALIDANDO
            var Actuales = _context.Set<Tasacambio>().Local.ToList();            
            //Que no vayan duplicados (misma fecha inicial).
            foreach (var item in Actuales.Where(t => t.FechaInicio==tasacambio.FechaInicio))
            {
                if (item.Id > 0)
                {
                    return new JsonResult(new { success = 0, errorMsg = "Tasa De Cambios Con Fecha Inicial Duplicadas", data = "" });
                }
            }

            //Que no vayan duplicados(fecha final).
            foreach (var item in Actuales.Where(t => t.FechaFinal == tasacambio.FechaFinal))
            {
                if (item.Id > 0)
                {
                    return new JsonResult(new { success = 0, errorMsg = "Tasa De Cambios Con Fecha Final Duplicadas", data = "" });
                }
            }

            //Que la tasa no sea cero            
            if (tasacambio.Tasa <= 0)
            {
                return new JsonResult(new { success = 0, errorMsg = "La Tasa no puede ser menor o igual a cero", data = "" });
            }

            //Solo el registro con fecha inicial más reciente puede tener fecha final NULL(indefinida).            
            foreach (var item in Actuales.OrderBy(t => t.FechaInicio).Take((Actuales.Count() - 1)))
            {
                if (item.FechaFinal > tasacambio.FechaFinal && tasacambio.FechaFinal==null)
                {
                    return new JsonResult(new { success = 0, errorMsg = "Solo la fecha final puede tener la fecha indefinida", data = "" });
                }
            }

            //no duplicar rango de fecha  en la BD
            //pendiente validacion correcta
            foreach (var item in Actuales.Where(t => tasacambio.FechaInicio >= t.FechaInicio  && tasacambio.FechaFinal <= t.FechaFinal))
            {
                if (item.Id != tasacambio.Id)
                {
                    return new JsonResult(new { success = 0, errorMsg = "No puede duplicar rango de fechas", data = "" });
                }
            }

            //------------------------------------------ END VALIDANDO


            _context.Entry(tasacambio).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return new JsonResult(new { success = 1, errorMsg = "", data = tasacambio });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TasacambioExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            //return NoContent();
        }

        // POST: api/Tasacambios
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Tasacambio>> PostTasacambio(Tasacambio[] tasacambio)
        {
            //Que no vayan duplicados (misma fecha inicial).
            foreach (var item in tasacambio.GroupBy(t => t.FechaInicio).Where(t => t.Count() > 1))
            {
                if (item.Count() > 0)
                {
                    return new JsonResult(new { success = 0, errorMsg = "Tasa De Cambios Con Fecha Inicial Duplicadas", data = "" });
                }
            }

            //Que no vayan duplicados(fecha final).
            foreach (var item in tasacambio.GroupBy(t => t.FechaFinal).Where(t => t.Count() > 1))
            {
                if (item.Count() > 0)
                {
                    return new JsonResult(new { success = 0, errorMsg = "Tasa De Cambios Con Fecha Final Duplicadas", data = "" });
                }
            }

            //Que la tasa no sea cero
            foreach (var item in tasacambio.Where(t => t.Tasa <= 0))
            {
                if (item.Tasa <= 0)
                {
                    return new JsonResult(new { success = 0, errorMsg = "La Tasa no puede ser menor o igual a cero", data = "" });
                }
            }


            //Solo el registro con fecha inicial más reciente puede tener fecha final NULL(indefinida).
            foreach (var item in tasacambio.OrderBy(t => t.FechaInicio).Take((tasacambio.Count() - 1)))
            {
                if (item.FechaFinal == null)
                {
                    return new JsonResult(new { success = 0, errorMsg = "Solo la fecha final puede tener la fecha indefinida", data = "" });
                }
            }

            //no duplicar rango de fecha            
            DateTime FechaInicio;
            DateTime? FechaFinal;
            for (int i = 0; i < tasacambio.Length; i++)
            {
                FechaInicio = tasacambio[i].FechaInicio;
                FechaFinal = tasacambio[i].FechaFinal;

                for (int i2 = 0; i2 < tasacambio.Length; i2++)
                {
                    if (i!=i2 && tasacambio[i2].FechaInicio >= FechaInicio && tasacambio[i2].FechaFinal<= FechaFinal)
                    {
                        return new JsonResult(new { success = 0, errorMsg = "TasaDeCambios Con Fecha Duplicadas", data = "" });
                    }
                }
            }

            //Validar para cada objeto de la lista que no se interponga con las fechas de los 
            //registros existentes en la BD
            //pendiente validacion correcta
            var Actuales = await _context.Tasacambio.ToListAsync();
            for (int i = 0; i < tasacambio.Length; i++)
            {
                FechaInicio = tasacambio[i].FechaInicio;
                FechaFinal = tasacambio[i].FechaFinal;

                for (int i2 = 0; i2 < Actuales.Count; i2++)
                {
                    if (FechaInicio  >= Actuales[i2].FechaInicio && FechaFinal <= Actuales[i2].FechaFinal)
                    {
                        return new JsonResult(new { success = 0, errorMsg = "TasaDeCambios Con Fecha Duplicadas en base de datos", data = "" });
                    }
                }
            }

            //insert 
            foreach (var item in tasacambio)
            {
                if (item.FechaFinal == null)
                { 
                item.FechaFinal= DateTime.UtcNow.Date;
                }
                    _context.Tasacambio.Add(item);
                await _context.SaveChangesAsync();
            }
            
            return new JsonResult(new { success = 1, errorMsg = "", data = tasacambio });
            
        }

        // DELETE: api/Tasacambios/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Tasacambio>> DeleteTasacambio(int id)
        {
            var tasacambio = await _context.Tasacambio.FindAsync(id);
            if (tasacambio == null)
            {
                return new JsonResult(new { success = 0, errorMsg = "Tasa No Encontrada", data = NotFound() });
            }

            _context.Tasacambio.Remove(tasacambio);
            await _context.SaveChangesAsync();

            return new JsonResult(new { success = 1, errorMsg = "", data = tasacambio });
        }

        //funcion para valida id de Tasa de cambio
        private bool TasacambioExists(int id)
        {
            return _context.Tasacambio.Any(e => e.Id == id);
        }
    }
}
