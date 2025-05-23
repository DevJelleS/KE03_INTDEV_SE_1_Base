using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccessLayer.Models;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace KE03_INTDEV_SE_1_Base.Pages.Orders
{
    public class OrderHistoryModel : PageModel
    {
        private readonly MatrixIncDbContext _context;

        public OrderHistoryModel(MatrixIncDbContext context)
        {
            _context = context;
        }

        public Customer Customer { get; set; }
        public IList<Order> Orders { get; set; } = new List<Order>();

        public async Task<IActionResult> OnGetAsync(int customerId)
        {
            Customer = await _context.Customers.FindAsync(customerId);
            if (Customer == null)
            {
                return NotFound();
            }

            Orders = await _context.Orders
                .Include(o => o.Products)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Page();
        }
    }
} 