using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E2.Data;
using E2.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using E2.DTO;
using Microsoft.IdentityModel.Tokens;
using E2.Firebase;

namespace E2.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductsController(ApplicationDbContext context,IWebHostEnvironment webHostEnvironment)
    {
        _webHostEnvironment = webHostEnvironment;
        _context = context;
    }
    private int GetUserId()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        
        return int.Parse(userId!);
    }

    // GET: api/Products
    [HttpGet("GetAllProducts")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] PaginationParams @params)
    {
        var allProduct = await _context.Products.Where(x => x.Deleted == false)
            .OrderBy(p => p.Id)
            .ToListAsync();
        var pageProduct =  allProduct.Skip((@params.Page - 1) * @params.ItemPerPage).Take(@params.ItemPerPage);
        int countProduct = allProduct.Count();
        foreach (var product in pageProduct)
        {
            product.GetProductRate();
        }
        return Ok(new
        {
            success = true,
            data = pageProduct,
            count = countProduct
        });
    }

    [HttpGet("GetMyProduct")]
    public async Task<ActionResult<IEnumerable<Product>>> GetMyProducts([FromQuery] PaginationParams @params)
    {
        
        var products = await _context.Products.Where(x => x.UserId == GetUserId() && x.Deleted == false)
            .OrderBy(p => p.Id).ToListAsync();
        var pageProduct = products.Skip((@params.Page - 1) * @params.ItemPerPage)
            .Take(@params.ItemPerPage);
        int countProduct = products.Count();    
        
        return Ok(new
        {
            success = true,
            data = pageProduct,
            count = countProduct
        });
    }
    // GET: api/Products/5
    [HttpGet("GetOneProduct/{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        product!.GetProductRate();
        
        if (product == null)
        {
            return NotFound(new
            {
                success = false,
                messaga = "there is no product with this id"
            });
        }

        if (!product.Deleted)
        {
            return Ok(new
            {
                success = true,
                data = product
            });
        }
        else
        {
            return NotFound(new
            {
                success = false,
                message = "the product deleted"
            });
        }
    }
    
    //PUT: api/Products/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("EditProduct/{id}")]
    public async Task<IActionResult> PutProduct(int id, [FromForm]ProductDto productDto, IFormFile? image)
    {
        if (productDto.Name.IsNullOrEmpty())
        {
            return BadRequest(new
            {
                success = false,
                message = "Name cannot be null"
            });
        }
        if (productDto.Description.IsNullOrEmpty())
        {
            return BadRequest(new
            {
                success = false,
                message = "Description cannot be null"
            });
        }
        if (productDto.Price == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "price cannot be null"
            });
        }
        if (productDto.Available == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "available cannot be null"
            });
        }
        var prod =  await _context.Products.FindAsync(id);
        int userLogedInId = GetUserId();
        if(prod == null)
        {
            return NotFound(new
            {
                success = false,
                message = "there is no product by this id"
            });
        }
        if (!prod.Deleted)
        {
            if (prod.UserId != userLogedInId)
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "you are not the owner"
                });
            }
            else
            {
                prod.Available = (int)productDto.Available;
                prod.Price = (float)productDto.Price;
                prod.Name = productDto.Name;
                prod.Description = productDto.Description;
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (prod.ImageUrl is not null)
                {
                    var oldImagePath = Path.Combine(wwwRootPath, prod.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }

                }
                if (image != null)
                {
                    string imageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    string imagePath = Path.Combine(wwwRootPath, @"images\product");
                    using (var imageStream = new FileStream(Path.Combine(imagePath, imageName), FileMode.Create))
                    {
                        image.CopyTo(imageStream);
                    }
                    prod.ImageUrl = wwwRootPath + @"\images\product\" + imageName;
                }
                _context.Entry(prod).State = EntityState.Detached;
                _context.Products.Update(prod);
                await _context.SaveChangesAsync();
                prod.GetProductRate();
                var deviceToken = _context.DeviceTokens.Where(p=> p.UserId == prod.UserId).ToList();
                if (deviceToken != null && deviceToken.Count > 0)
                {
                    foreach (var device in deviceToken)
                    {
                        string token = device.Token;

                        FirebaseCloudMessaging.SendNotification(token, "Product Updated", $"A product with the name '{prod.Name}' has been Updated.");
                    }
                }

                return Ok(new
                {
                    success = true,
                    data = prod
                });
            } 
        }
        else
        {
            return NotFound(new
            {
                success = false,
                message = "the product deleted"
            });
        }
        
    }
    
    // POST: api/Products
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost("AddProduct")]
    public async Task<ActionResult<Product>> PostProduct([FromForm] ProductDto productDto,IFormFile? image)
    {
        if (productDto.Name.IsNullOrEmpty())
        {
            return BadRequest(new
            {
                success = false,
                message = "Name cannot be null"
            });
        }
        if (productDto.Description.IsNullOrEmpty())
        {
            return BadRequest(new
            {
                success = false,
                message = "Description cannot be null"
            });
        }
        if (productDto.Price == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "price cannot be null"
            });
        }
        if (productDto.Available == null)
        {
            return BadRequest(new
            {
                success = false,
                message = "available cannot be null"
            });
        }

        Product prod = new(_context)
        {
            Price = (float)productDto.Price!,
            Available = (int)productDto.Available!,
            Name = productDto.Name,
            Description = productDto.Description,
            UserId = GetUserId()
        };
        
        string wwwRootPath = _webHostEnvironment.WebRootPath;
        if(image is null)
        {
            prod.ImageUrl = null;
        }
        else
        {
            string imageName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string imagePath = Path.Combine(wwwRootPath, @"images\product");
            using (var imageStream = new FileStream(Path.Combine(imagePath, imageName), FileMode.Create))
            {
                image.CopyTo(imageStream);
            }
            prod.ImageUrl = wwwRootPath + @"\images\product\" + imageName;
        }
        _context.Products.Add(prod);
        await _context.SaveChangesAsync();
        var rates = _context.ProductRates.Where(p => p.ProductId == prod.Id).Select(p => p.Rate);
        if (!rates.Any())
        {
            prod.Rate = 0;
            prod.RateCount = 0;

        }
        else
        {
            int count = rates.Count();
            prod.Rate = (float)Queryable.Average(rates);
            prod.RateCount = count;
        }
        var deviceToken = _context.DeviceTokens.Where(p => p.UserId == prod.UserId).ToList();
        if (deviceToken != null && deviceToken.Count > 0)
        {
            foreach (var device in deviceToken)
            {
                string token = device.Token;

                FirebaseCloudMessaging.SendNotification(token, "New Product Added", $"A new product with the name '{prod.Name}' has been added.");
            }
        }
        return Ok(new
        {
            success = true,
            data = prod
        });
    }


    // DELETE: api/Products/5
    [HttpDelete("DeleteProduct/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new
            {
                success = false,
                message = "product not found"
            });
        }
        int userLogedInId = GetUserId();
        if(product.UserId == userLogedInId)
        {
            product.Deleted = true;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            var deviceToken = _context.DeviceTokens.Where(p => p.UserId == product.UserId).ToList();
            if (deviceToken != null && deviceToken.Count > 0)
            {
                foreach (var device in deviceToken)
                {
                    string token = device.Token;

                    FirebaseCloudMessaging.SendNotification(token, "Product deleted", $"A  product with the name '{product.Name}' has been deleted.");
                }
            }
            return Ok(new
            {
                success = true,
                message = "deleted"
            });
        }
        else
        {
            return Unauthorized(new
            {
                success = false,
                message = "unAuthorized"
            });
        }

    }
    
}
