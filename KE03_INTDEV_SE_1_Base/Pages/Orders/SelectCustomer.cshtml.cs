using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccessLayer.Models;
using DataAccessLayer;

namespace KE03_INTDEV_SE_1_Base.Pages.Orders
{
    public class SelectCustomerModel : PageModel
    {
        private readonly MatrixIncDbContext _context;

        public SelectCustomerModel(MatrixIncDbContext context)
        {
            _context = context;
        }

        public IList<Customer> Customers { get; set; } = new List<Customer>();

        public async Task OnGetAsync()
        {
            Customers = _context.Customers.Where(c => c.Active).ToList();
        }
    }
} 