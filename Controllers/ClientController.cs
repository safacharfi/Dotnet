using Microsoft.AspNetCore.Mvc;
using sav.Models;
using sav.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace sav.Controllers
{
    public class ClientController : Controller
    {
        private readonly IClientRepository _clientRepository;
        private readonly ApplicationDbContext _context;

        public ClientController(IClientRepository clientRepository, ApplicationDbContext context)
        {
            _clientRepository = clientRepository;
            _context = context;
        }

        // GET: /Client
        // Liste de tous les clients
        public async Task<IActionResult> Index()
        {
            var clients = await _clientRepository.GetAllClientsAsync();
            return View(clients);
        }

        // GET: /Client/Details/{id}
        // Affiche les détails d'un client
        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientRepository.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // GET: /Client/Authenticate
        // Affiche la page d'authentification ou d'inscription
        public IActionResult Authenticate()
        {
            return View();
        }

        // POST: /Client/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            var client = await _clientRepository.AuthenticateClientAsync(email, password);
            if (client != null)
            {
                HttpContext.Session.SetString("ClientEmail", client.Email);
                return RedirectToAction("ClientHomePage"); // Redirige vers la page d'accueil
            }

            ModelState.AddModelError("", "Invalid email or password.");
            return View("Authenticate");
        }

        // GET: /Client/ClientHomePage
        // Page d'accueil du client après authentification
        public async Task<IActionResult> ClientHomePage()
        {
            var clientEmail = HttpContext.Session.GetString("ClientEmail");
            if (string.IsNullOrEmpty(clientEmail))
            {
                return RedirectToAction("Authenticate", "Client");
            }

            var client = await _clientRepository.GetClientByEmailAsync(clientEmail);
            if (client == null)
            {
                return NotFound("Client not found.");
            }

            return View(client);
        }

        // GET: /Client/Signup
        // Affiche le formulaire d'inscription
        public IActionResult Signup()
        {
            return View();
        }

        // POST: /Client/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(Client client)
        {
            if (ModelState.IsValid)
            {
                var existingClient = await _clientRepository.GetClientByEmailAsync(client.Email);
                if (existingClient != null)
                {
                    ModelState.AddModelError("Email", "This email is already registered.");
                    return View(client);
                }

                await _clientRepository.AddClientAsync(client);
                await _clientRepository.SaveChangesAsync();
                return RedirectToAction("Authenticate");
            }

            return View(client);
        }

        // GET: /Client/Create
        // Affiche le formulaire pour ajouter un client
        public IActionResult Create()
        {
            return View(new Client());
        }

        // POST: /Client/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Client client)
        {
            if (ModelState.IsValid)
            {
                await _clientRepository.AddClientAsync(client);
                await _clientRepository.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: /Client/Edit/{id}
        // Affiche la vue d'édition
        public IActionResult Edit(int id)
        {
            var client = _context.Client.FirstOrDefault(c => c.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: /Client/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Client client)
        {
            if (ModelState.IsValid)
            {
                _context.Update(client);
                _context.SaveChanges();
                return RedirectToAction("ClientHomePage", new { id = client.ClientId });
            }
            return View(client);
        }

        // GET: /Client/ClientHomePageById/{id}
        // Page d'accueil d'un client par ID
        public IActionResult ClientHomePageById(int id)
        {
            var client = _context.Client.FirstOrDefault(c => c.ClientId == id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // GET: /Client/Delete/{id}
        // Affiche la confirmation de suppression d'un client
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientRepository.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }
            return View(client);
        }

        // POST: /Client/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _clientRepository.DeleteClientAsync(id);
            await _clientRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Client/SubmitClaim
        public IActionResult SubmitClaim()
        {
            return RedirectToAction("Create", "Claim");
        }

        // GET: /Client/ViewClaims
        public async Task<IActionResult> ViewClaims()
        {
            var clientEmail = HttpContext.Session.GetString("ClientEmail");
            if (string.IsNullOrEmpty(clientEmail))
            {
                return RedirectToAction("Authenticate", "Client");
            }

            var client = await _clientRepository.GetClientByEmailAsync(clientEmail);
            if (client == null)
            {
                return NotFound();
            }

            var claims = await _context.Claim
                .Where(c => c.ClientId == client.ClientId)
                .Include(c => c.Article)
                .ToListAsync();

            return View(claims);
        }

        // GET: /Client/GetTotalInterventionAmount/{id}
        public async Task<IActionResult> GetTotalInterventionAmount(int id)
        {
            var client = await _clientRepository.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound();
            }

            var totalAmount = await _context.TechnicalIntervention
                .Where(ti => ti.Claim.ClientId == client.ClientId)
                .SumAsync(ti => ti.TotalCost);

            ViewBag.TotalInterventionAmount = totalAmount;
            return View("Details", client);
        }
    }
}