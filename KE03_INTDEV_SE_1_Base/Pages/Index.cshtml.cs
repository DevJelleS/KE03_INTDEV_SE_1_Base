using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccessLayer.Models;
using DataAccessLayer;

namespace KE03_INTDEV_SE_1_Base.Pages
{
    public class IndexModel : PageModel
    {
        private readonly MatrixIncDbContext _context;

        public IndexModel(MatrixIncDbContext context)
        {
            _context = context;
        }

        public IList<Customer> Customers { get; set; } = new List<Customer>();

        public void OnGet()
        {
            Customers = _context.Customers.Where(c => c.Active).ToList();
        }
    }
}
