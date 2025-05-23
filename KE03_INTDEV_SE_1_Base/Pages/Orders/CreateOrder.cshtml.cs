using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccessLayer.Models;
using DataAccessLayer;
using System.Text.Json;

namespace KE03_INTDEV_SE_1_Base.Pages.Orders
{
    public class CreateOrderModel : PageModel
    {
        private readonly MatrixIncDbContext _context;
        private const string SelectedProductsKey = "SelectedProducts";

        public CreateOrderModel(MatrixIncDbContext context)
        {
            _context = context;
        }

        public Customer Customer { get; set; }
        public IList<Product> AvailableProducts { get; set; }
        public IList<Product> SelectedProducts { get; set; } = new List<Product>();

        public async Task<IActionResult> OnGetAsync(int customerId)
        {
            Customer = await _context.Customers.FindAsync(customerId);
            if (Customer == null)
            {
                return NotFound();
            }

            AvailableProducts = _context.Products.ToList();
            
            // Get selected products from session
            var selectedProductsJson = HttpContext.Session.GetString(SelectedProductsKey);
            if (!string.IsNullOrEmpty(selectedProductsJson))
            {
                var selectedProductIds = JsonSerializer.Deserialize<List<int>>(selectedProductsJson) ?? new List<int>();
                SelectedProducts = _context.Products.Where(p => selectedProductIds.Contains(p.Id)).ToList();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddProductAsync(int customerId, int productId)
        {
            var selectedProductsJson = HttpContext.Session.GetString(SelectedProductsKey);
            var selectedProductIds = new List<int>();

            if (!string.IsNullOrEmpty(selectedProductsJson))
            {
                selectedProductIds = JsonSerializer.Deserialize<List<int>>(selectedProductsJson) ?? new List<int>();
            }

            if (!selectedProductIds.Contains(productId))
            {
                selectedProductIds.Add(productId);
                HttpContext.Session.SetString(SelectedProductsKey, JsonSerializer.Serialize(selectedProductIds));
            }

            return RedirectToPage(new { customerId });
        }

        public async Task<IActionResult> OnPostRemoveProductAsync(int customerId, int productId)
        {
            var selectedProductsJson = HttpContext.Session.GetString(SelectedProductsKey);
            if (!string.IsNullOrEmpty(selectedProductsJson))
            {
                var selectedProductIds = JsonSerializer.Deserialize<List<int>>(selectedProductsJson) ?? new List<int>();
                selectedProductIds.Remove(productId);
                HttpContext.Session.SetString(SelectedProductsKey, JsonSerializer.Serialize(selectedProductIds));
            }

            return RedirectToPage(new { customerId });
        }

        public async Task<IActionResult> OnPostConfirmOrderAsync(int customerId)
        {
            var selectedProductsJson = HttpContext.Session.GetString(SelectedProductsKey);
            if (string.IsNullOrEmpty(selectedProductsJson))
            {
                return RedirectToPage(new { customerId });
            }

            var selectedProductIds = JsonSerializer.Deserialize<List<int>>(selectedProductsJson) ?? new List<int>();
            var products = _context.Products.Where(p => selectedProductIds.Contains(p.Id)).ToList();

            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow
            };

            // Add products to the order using the collection's Add method
            foreach (var product in products)
            {
                order.Products.Add(product);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear the session
            HttpContext.Session.Remove(SelectedProductsKey);

            return RedirectToPage("./OrderHistory", new { customerId });
        }
    }
} 