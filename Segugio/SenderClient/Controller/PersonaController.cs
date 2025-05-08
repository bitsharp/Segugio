using Audit.Core;
using Microsoft.AspNetCore.Mvc;
using SenderClient.Data;

namespace SenderClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonaController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PersonaController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Persona
        [HttpGet]
        public IActionResult GetAll()
        {
            var tests = _context.Tests.ToList();
            return Ok(tests);
        }

        // POST: api/Persona/Login
        [HttpPost]
        [Route("Login")]
        public IActionResult LogIn([FromBody] Persona persona)
        {
            using (var scope = AuditScope.Create("Login", () => new { Data = "Esempio" }, new { Utente = "Admin" }))
            {
                // Logica della tua operazione
            }

            return Ok("Autenticazione effettuata con successo.");
        }

        // POST: api/Persona/Logout
        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout([FromBody] Persona persona)
        {
            using (var scope = AuditScope.Create("Login", () => new { Data = "Esempio" }, new { Utente = "Admin" }))
            {
                // Logica della tua operazione
            }

            return Ok("Autenticazione effettuata con successo.");
        }


        // GET: api/Persona/Error
        [HttpGet]
        [Route("ErroreGenerato")]
        public IActionResult ErroreGenerato()
        {
            using (var scope = AuditScope.Create("Login", () => new { Data = "Esempio" }, new { Utente = "Admin" }))
            {
                try
                {
                    throw new Exception("Errore generato");
                }
                catch (Exception e)
                {
                    return BadRequest(e.Message);
                }
            }

            return Ok("");
        }


        // POST: api/test
        [HttpPost]
        public IActionResult Create([FromBody] Persona persona)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Tests.Add(persona);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = persona.Id }, persona);
        }

        // GET: api/test/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            var test = _context.Tests.Find(id);
            if (test == null)
                return NotFound();

            return Ok(test);
        }

        // DELETE: api/test/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var test = _context.Tests.Find(id);
            if (test == null)
                return NotFound();

            _context.Tests.Remove(test);
            _context.SaveChanges();

            return NoContent();
        }
    }
}