using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FoodApi.Data;
using FoodApi.Models;
using ImageUploader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductsController : ControllerBase
    {
        private FoodDbContext _dbContext;
        public ProductsController(FoodDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        // GET: api/Products
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_dbContext.Products);
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok(_dbContext.Products.Find(id));
        }

        // GET: api/Products/ProductsByCategory/5
        [HttpGet("[action]/{categoryId}")]
        public IActionResult ProductsByCategory(int categoryId)
        {
            var products = from v in _dbContext.Products
                           where v.CategoryId == categoryId
                           select new
                           {
                               Id = v.Id,
                               Name = v.Name,
                               Price = v.Price,
                               Detail = v.Detail,
                               CategoryId = v.CategoryId,
                               ImageUrl = v.ImageUrl
                           };

            return Ok(products);
        }

        // GET: api/Products/PopularProducts
        [HttpGet("[action]")]
        public IActionResult PopularProducts()
        {
            var products = from v in _dbContext.Products
                           where v.IsPopularProduct == true
                           select new
                           {
                               Id = v.Id,
                               Name = v.Name,
                               Price = v.Price,
                               ImageUrl = v.ImageUrl
                           };

            return Ok(products);
        }


        // POST: api/Products
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromBody] Product product)
        {
            var stream = new MemoryStream(product.ImageArray);
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";
            var folder = "wwwroot";
            var response = FilesHelper.UploadImage(stream, folder, file);
            if (!response)
            {
                return BadRequest();
            }
            else
            {
                product.ImageUrl = file;
                _dbContext.Products.Add(product);
                _dbContext.SaveChanges();
                return StatusCode(StatusCodes.Status201Created);
            }
        }

        // PUT: api/Products/5
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] Product product)
        {
            var entity = _dbContext.Products.Find(id);
            if (entity == null)
            {
                return NotFound("No product found against this id...");
            }

            var stream = new MemoryStream(product.ImageArray);
            var guid = Guid.NewGuid().ToString();
            var file = $"{guid}.jpg";
            var folder = "wwwroot";
            var response = FilesHelper.UploadImage(stream, folder, file);
            if (!response)
            {
                return BadRequest();
            }
            else
            {
                entity.CategoryId = product.CategoryId;
                entity.Name = product.Name;
                entity.ImageUrl = file;
                entity.Price = product.Price;
                entity.Detail = product.Detail;
                _dbContext.SaveChanges();
                return Ok("Product Updated Successfully...");
            }
        }

        // DELETE: api/ApiWithActions/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _dbContext.Products.Find(id);
            if (product == null)
            {
                return NotFound("No product found against this id...");
            }
            else
            {
                _dbContext.Products.Remove(product);
                _dbContext.SaveChanges();
                return Ok("Product deleted...");
            }
        }
    }
}
