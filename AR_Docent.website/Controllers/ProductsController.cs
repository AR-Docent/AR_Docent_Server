using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AR_Docent.website.Models;
using AR_Docent.website.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AR_Docent.website.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        public JsonFileProductsService ProductsService { get; private set; }

        public ProductsController(JsonFileProductsService productsService)
        {
            this.ProductsService = productsService;
        }
        /*
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return ProductsService.GetProducts();
        }
        */

        //[HttpPatch] "[FromBody]"
        [Route("Rate")]
        [HttpGet]
        public ActionResult Get(
	        [FromQuery]string productId,
	        [FromQuery]int rating)
        {
            ProductsService.AddRating(productId, rating);
            return Ok();
        }

    }
}
