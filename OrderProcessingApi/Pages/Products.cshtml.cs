using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrderProcessingApi.Models;
using OrderProcessingApi.Repositories;

public class ProductsModel : PageModel
{
    private readonly IProductRepository _productRepository;

    [BindProperty]
    public Product NewProduct { get; set; } = new();

    [BindProperty]
    public Product EditProduct { get; set; } = new();

    public List<Product> Products { get; set; } = new();

    public bool IsEditing { get; set; } = false;

    public ProductsModel(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task OnGetAsync()
    {
        Products = (await _productRepository.GetAllAsync()).ToList();
    }

   
    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            Products = (await _productRepository.GetAllAsync()).ToList();
            return Page();
        }

        await _productRepository.AddAsync(NewProduct);

        // Reload the product list immediately after adding
        Products = (await _productRepository.GetAllAsync()).ToList();

        // Clear the new product model (optional)
        NewProduct = new Product();

        return Page();  // Return the same page with updated Products list
    }

    // Load all products and check if we are editing a product
    public async Task OnGetEditAsync(Guid ProductId)
    {
        // Load all products
        Products = (await _productRepository.GetAllAsync()).ToList();

        // Load the product for editing
        var product = await _productRepository.GetByIdAsync(ProductId);
        if (product != null)
        {
            EditProduct = product;
            IsEditing = true;
        }
    }

    // Handler for adding a new product
    public async Task<IActionResult> OnPostAddAsync()
    {
        if (ModelState.IsValid)
        {
            Products = (await _productRepository.GetAllAsync()).ToList();
            return Page();
        }
        await _productRepository.AddAsync(NewProduct);
        return Page();
    }

    // Handler for updating an existing product
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (ModelState.IsValid)
        {
            Products = (await _productRepository.GetAllAsync()).ToList();
            return Page();
        }
        await _productRepository.UpdateAsync(EditProduct);
        return RedirectToPage();
    }

    // Handler for deleting a product
    public async Task<IActionResult> OnPostDeleteAsync(Guid ProductId)
    {
        await _productRepository.DeleteAsync(ProductId);
        return RedirectToPage();
    }

}
