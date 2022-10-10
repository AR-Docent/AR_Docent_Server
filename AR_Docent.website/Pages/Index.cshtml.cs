using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AR_Docent.website.Services;
using AR_Docent.website.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace AR_Docent.website.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public JsonFileProductsService ProductsService;
        public IEnumerable<Product>Products { get; private set; }

        public IndexModel(ILogger<IndexModel> logger,
            JsonFileProductsService productsService)
        {
            _logger = logger;
            ProductsService = productsService;
        }

        public void OnGet()
        {
            Products = ProductsService.GetProducts();
        }
    }
}
