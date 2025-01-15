using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sav.Models;
using sav.ViewModels;


namespace sav.Controllers
{
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ClaimController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Afficher le formulaire pour ajouter un Claim
        public IActionResult Create()
        {
            // Charger tous les articles disponibles
            ViewBag.Articles = new SelectList(_context.Article, "ArticleId", "Name");
            return View();
        }
        // Action pour afficher les réclamations
        public IActionResult ViewClaims()
        {
            var claims = _context.Claim.ToList();  // Récupérer toutes les réclamations depuis la base de données

            return View(claims);  // Passer la liste des réclamations à la vue
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClaimViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Vérification de l'article
                    var article = await _context.Article.FindAsync(model.ArticleId);
                    if (article == null)
                    {
                        ModelState.AddModelError("ArticleId", "L'article sélectionné n'existe pas.");
                        return View(model);
                    }

                    // Récupérer le client statique "Safa Charfi"
                    var client = await _context.Client.FirstOrDefaultAsync(c => c.Name == "Safa Charfi");
                    if (client == null)
                    {
                        ModelState.AddModelError("", "Le client 'Safa Charfi' n'existe pas.");
                        return View(model);
                    }

                    // Créer et sauvegarder le Claim
                    var claim = new Claim
                    {
                        ClientId = client.ClientId,
                        ArticleId = model.ArticleId,
                        Description = model.Description,
                        Status = "En attente",  // Définir le statut par défaut ici
                        Date = DateTime.Now,
                    };

                    _context.Claim.Add(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Votre réclamation a été ajoutée avec succès.";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'ajout du Claim : {ex.Message}");
                    ModelState.AddModelError("", "Une erreur s'est produite. Veuillez réessayer.");
                }
            }
            return View(model);
        }


        // Afficher la liste des réclamations
        public async Task<IActionResult> Index()
        {
            // Inclure les données des Clients et Articles associés pour les afficher
            var claims = await _context.Claim
                .Include(c => c.Client)
                .Include(c => c.Article)
                .ToListAsync();

            return View(claims);
        }

        // Modifier un Claim (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claim.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            // Charger les listes pour les articles et clients
            ViewBag.Articles = new SelectList(_context.Article, "ArticleId", "Name", claim.ArticleId);
            ViewBag.Clients = new SelectList(_context.Client, "ClientId", "Name", claim.ClientId);

            var model = new ClaimViewModel
            {
                
                ArticleId = claim.ArticleId,
                ClientId = claim.ClientId,
                Description = claim.Description,
                Status = claim.Status,
                
            };

            return View(model);
        }

        // Modifier un Claim (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClaimViewModel model)
        {
            

            if (ModelState.IsValid)
            {
                try
                {
                    var claim = await _context.Claim.FindAsync(id);
                    if (claim == null)
                    {
                        return NotFound();
                    }

                    claim.ArticleId = model.ArticleId;
                    claim.ClientId = model.ClientId;
                    claim.Description = model.Description;
                    claim.Status = model.Status;

                    _context.Update(claim);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Réclamation modifiée avec succès.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la modification : {ex.Message}");
                    ModelState.AddModelError("", "Une erreur s'est produite. Veuillez réessayer.");
                }
            }

            // Recharger les listes en cas d'erreur
            ViewBag.Articles = new SelectList(_context.Article, "ArticleId", "Name", model.ArticleId);
            ViewBag.Clients = new SelectList(_context.Client, "ClientId", "Name", model.ClientId);
            return View(model);
        }

        // Supprimer un Claim (GET)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var claim = await _context.Claim
                .Include(c => c.Client)
                .Include(c => c.Article)
                .FirstOrDefaultAsync(m => m.ClaimId == id);

            if (claim == null)
            {
                return NotFound();
            }

            return View(claim);
        }

        // Supprimer un Claim (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var claim = await _context.Claim.FindAsync(id);
            if (claim != null)
            {
                _context.Claim.Remove(claim);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Réclamation supprimée avec succès.";
            }

            return RedirectToAction(nameof(Index));
        }
        // Afficher la liste des réclamations en attente
        public async Task<IActionResult> PendingClaims()
        {
            var claims = await _context.Claim
                .Include(c => c.Client)
                .Include(c => c.Article)
                .Where(c => c.Status == "En attente") // Filtrer par statut
                .ToListAsync();

            return View(claims);
        }

        // Afficher les détails de la réclamation et les options d'intervention
        public async Task<IActionResult> ProcessClaim(int id)
        {
            var claim = await _context.Claim.Include(c => c.Client).Include(c => c.Article).FirstOrDefaultAsync(c => c.ClaimId == id);
            if (claim == null)
            {
                return NotFound();
            }

            // Vérifiez si l'article est sous garantie
            bool isUnderWarranty = claim.Article.IsUnderWarranty;
            ViewBag.IsUnderWarranty = isUnderWarranty;

            // Charger tous les employés disponibles
            ViewBag.Employees = new SelectList(_context.Employees, "EmployeeId", "Name");

            return View(claim);
        }

        // Gérer l'intervention technique
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessClaim(int id, int employeeId, decimal laborCost, List<int> sparePartIds)
        {
            var claim = await _context.Claim.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            // Créer l'intervention technique
            var intervention = new TechnicalIntervention
            {
                ClaimId = claim.ClaimId,
                InterventionDate = DateTime.Now,
                IsWarranty = claim.Article.IsUnderWarranty,
                LaborCost = laborCost,
                SparePartsUsed = _context.SparePart.Where(sp => sparePartIds.Contains(sp.SparePartId)).ToList()
            };

            // Calculer le coût total
            if (intervention.IsWarranty)
            {
                intervention.TotalCost = 0; // Gratuit
            }
            else
            {
                var sparePartsCost = intervention.SparePartsUsed.Sum(sp => sp.Price);
                intervention.TotalCost = sparePartsCost + intervention.LaborCost;
            }

            // Ajouter l'intervention et enregistrer
            _context.TechnicalIntervention.Add(intervention);
            await _context.SaveChangesAsync();

            // Mettre à jour l'état de la réclamation si nécessaire
            claim.Status = "Traitée"; // Mettre à jour le statut
            _context.Update(claim);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Intervention technique associée avec succès.";
            return RedirectToAction(nameof(PendingClaims));
        }


    }
}
