using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataAccessLayer.Models;
using DataAccessLayer;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

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

        public Customer? Customer { get; set; }
        public IList<Product> AvailableProducts { get; set; } = new List<Product>();
        public IList<OrderProduct> SelectedProducts { get; set; } = new List<OrderProduct>();

        public async Task<IActionResult> OnGetAsync(int customerId)
        {
            Customer = await _context.Customers.FindAsync(customerId);
            if (Customer == null)
            {
                return NotFound();
            }

            AvailableProducts = await _context.Products.ToListAsync();
            
            // Get selected products from session
            var selectedProductsJson = HttpContext.Session.GetString(SelectedProductsKey);
            if (!string.IsNullOrEmpty(selectedProductsJson))
            {
                var selectedProducts = JsonSerializer.Deserialize<List<OrderProduct>>(selectedProductsJson) ?? new List<OrderProduct>();
                foreach (var item in selectedProducts)
                {
                    item.Product = await _context.Products.FindAsync(item.ProductId);
                }
                SelectedProducts = selectedProducts;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddProductAsync(int customerId, int productId, int quantity)
        {
            if (quantity < 1) quantity = 1;

            var selectedProductsJson = HttpContext.Session.GetString(SelectedProductsKey);
            var selectedProducts = new List<OrderProduct>();

            if (!string.IsNullOrEmpty(selectedProductsJson))
            {
                selectedProducts = JsonSerializer.Deserialize<List<OrderProduct>>(selectedProductsJson) ?? new List<OrderProduct>();
            }

            var existingProduct = selectedProducts.FirstOrDefault(p => p.ProductId == productId);
            if (existingProduct != null)
            {
                existingProduct.Quantity += quantity;
            }
            else
            {
                selectedProducts.Add(new OrderProduct
                {
                    ProductId = productId,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetString(SelectedProductsKey, JsonSerializer.Serialize(selectedProducts));

            return RedirectToPage(new { customerId });
        }

        public async Task<IActionResult> OnPostRemoveProductAsync(int customerId, int productId)
        {
            var selectedProductsJson = HttpContext.Session.GetString(SelectedProductsKey);
            if (!string.IsNullOrEmpty(selectedProductsJson))
            {
                var selectedProducts = JsonSerializer.Deserialize<List<OrderProduct>>(selectedProductsJson) ?? new List<OrderProduct>();
                selectedProducts.RemoveAll(p => p.ProductId == productId);
                HttpContext.Session.SetString(SelectedProductsKey, JsonSerializer.Serialize(selectedProducts));
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

            var selectedProducts = JsonSerializer.Deserialize<List<OrderProduct>>(selectedProductsJson) ?? new List<OrderProduct>();
            
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            foreach (var item in selectedProducts)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    order.OrderProducts.Add(new OrderProduct
                    {
                        Product = product,
                        Quantity = item.Quantity
                    });
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Clear the session
            HttpContext.Session.Remove(SelectedProductsKey);

            return RedirectToPage("./OrderHistory", new { customerId });
        }
    }
} 