using Microsoft.AspNetCore.Mvc;
using sav.Models;
using sav.Repository;
using System.Threading.Tasks;

namespace sav.Controllers
{
    public class AdminController : Controller
    {
        private readonly IEmployeeRepository _employeeRepository;

        public AdminController(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        // Affiche le formulaire de connexion pour l'admin
        public IActionResult Login()
        {
            return View();
        }

        // Gère la soumission du formulaire de connexion de l'admin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            // Vérifie les identifiants de l'admin
            if (username == "admin" && password == "123")
            {
                // Connexion réussie, redirige vers le tableau de bord de l'admin
                return RedirectToAction("Dashboard");
            }

            // Si les identifiants sont incorrects, afficher un message d'erreur
            ModelState.AddModelError("", "Identifiant ou mot de passe invalide.");
            return View();
        }

        // Affiche le tableau de bord de l'admin après une connexion réussie
        public IActionResult Dashboard()
        {
            return View();
        }

        // Affiche la liste des employés
        public async Task<IActionResult> EmployeeIndex()
        {
            var employees = await _employeeRepository.GetAllEmployeesAsync();
            return View(employees);
        }

        // Affiche les détails d'un employé
        public async Task<IActionResult> EmployeeDetails(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // Affiche le formulaire pour ajouter un employé
        public IActionResult EmployeeCreate()
        {
            return View();
        }

        // Gère l'ajout d'un employé
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeCreate(Employee employee)
        {
            if (ModelState.IsValid)
            {
                await _employeeRepository.AddEmployeeAsync(employee);
                return RedirectToAction(nameof(EmployeeIndex));
            }
            return View(employee);
        }

        // Affiche le formulaire pour éditer un employé
        public async Task<IActionResult> EmployeeEdit(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // Gère la modification d'un employé
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeEdit(int id, Employee employee)
        {
            if (id != employee.EmployeeId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _employeeRepository.UpdateEmployeeAsync(employee);
                return RedirectToAction(nameof(EmployeeIndex));
            }
            return View(employee);
        }

        // Affiche la confirmation de suppression d'un employé
        public async Task<IActionResult> EmployeeDelete(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // Confirme la suppression d'un employé
        [HttpPost, ActionName("EmployeeDelete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EmployeeDeleteConfirmed(int id)
        {
            await _employeeRepository.DeleteEmployeeAsync(id);
            return RedirectToAction(nameof(EmployeeIndex));
        }
    }
}