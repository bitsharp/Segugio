using Audit.Core;
using Microsoft.AspNetCore.Mvc;
using Segugio;
using Segugio.Ports;
using SenderClient.Data;

namespace SenderClient.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PersonaController : ControllerBase
    {
        private readonly AppDbDbContext _dbContext;
        private readonly ISegugioAuditor _segugioAuditor;

        public PersonaController(AppDbDbContext dbContext, ISegugioAuditor segugioAuditor)
        {
            _dbContext = dbContext;
            _segugioAuditor = segugioAuditor;
        }

        // GET: api/Persona
        [HttpGet]
        public IActionResult GetAll()
        {
            var tests = _dbContext.Tests.ToList();
            return Ok(tests);
        }

        // POST: api/Persona/Login
        [HttpPost]
        [Route("Login")]
        public IActionResult LogIn([FromBody] Persona persona)
        {
            using (var scope = _segugioAuditor.CreateScope("Login"))
            {
                // Logica della tua operazione
                var account = "MROSSI";
                var isLogged = true;
            }

            return Ok("Autenticazione effettuata con successo.");
        }

        // POST: api/Persona/Logout
        [HttpPost]
        [Route("Logout")]
        public IActionResult Logout([FromBody] Persona persona)
        {
            using (var scope = _segugioAuditor.CreateScope("Logout"))
            {
                // Logica della tua operazione
                var account = "MROSSI";
                var isLogged = false;
            }

            return Ok("Autenticazione effettuata con successo.");
        }
        
        // GET: api/Persona/Error
        [HttpGet]
        [Route("ErroreGenerato")]
        public IActionResult ErroreGenerato()
        {
            try
            {
                using (var scope = _segugioAuditor.CreateScope("Lettura dati utente"))
                {
                    throw new Exception("Errore generato");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok("");
        }


        // POST: api/test
        [HttpPost]
        public IActionResult Create([FromBody] Persona persona)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _dbContext.Tests.Add(persona);
            _dbContext.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = persona.Id }, persona);
        }

        // GET: api/test/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(long id)
        {
            var test = _dbContext.Tests.Find(id);
            if (test == null)
                return NotFound();

            return Ok(test);
        }

        // DELETE: api/test/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(long id)
        {
            var test = _dbContext.Tests.Find(id);
            if (test == null)
                return NotFound();

            _dbContext.Tests.Remove(test);
            _dbContext.SaveChanges();

            return NoContent();
        }
    }
}