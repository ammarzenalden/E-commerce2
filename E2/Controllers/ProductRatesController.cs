using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using E2.Data;
using E2.Models;
using System.Security.Claims;
using E2.DTO;
using Microsoft.AspNetCore.Authorization;

namespace E2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProductRatesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductRatesController(ApplicationDbContext context)
        {
            _context = context;
        }
        private int GetUserId()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId!);
        }
        // GET: api/ProductRates
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductRate>>> GetProductRates()
        {
          if (_context.ProductRates == null)
          {
              return NotFound();
          }
            return await _context.ProductRates.ToListAsync();
        }

        // GET: api/ProductRates/5
        [HttpGet("GetMyRate/{id}")]
        public IActionResult GetProductRate(int id)
        {
          
            var productRate = _context.ProductRates.Where(x => x.ProductId == id && x.UserId == GetUserId());
            if (!productRate.Any())
            {
                return Ok(new
                {
                    success = true,
                    data = 0
                });
            }
            else
            {
                if(productRate.Select(x => x.UserId).SingleOrDefault() == GetUserId())
                {
                    return Ok(new
                    {
                        success = true,
                        data = productRate.Select(x => x.Rate)
                    });
                }
                else
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "you are not the owner of this rate"
                    });
                }
            }

        }

        // PUT: api/ProductRates/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutProductRate(int id, ProductRate productRate)
        //{
        //    if (id != productRate.Id)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(productRate).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!ProductRateExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/ProductRates
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("AddRate")]
        public async Task<ActionResult<ProductRate>> PostProductRate(ProductRateDto productRateDto)
        {
            var productRate = _context.ProductRates.Where(x => x.ProductId == productRateDto.ProductId && x.UserId == GetUserId());
            var product = _context.Products.Find(productRateDto.ProductId);
            if (!productRate.Any())
            {
                var isBuyerProduct = _context.PaymentProducts.Where(p => p.BuyerId == GetUserId() && p.ProductId == product!.Id);

                if (isBuyerProduct.Any())
                {
                    ProductRate prodRate = new()
                    {
                        ProductId = productRateDto.ProductId,
                        UserId = GetUserId(),
                        Rate = productRateDto.Rate
                    };
                    _context.ProductRates.Add(prodRate);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        success = true,
                        data = prodRate
                    });
                }
                else
                {
                    return Ok(new
                    {
                        success = false,
                        message = "you need to buy this product first"
                    });
                }
            }
            else
            {
                int id = productRate.Select(x => x.Id).SingleOrDefault();
                ProductRate prodRate = await _context.ProductRates.FindAsync(id);
                prodRate!.Rate = productRateDto.Rate;
                _context.ProductRates.Update(prodRate);
                await _context.SaveChangesAsync();
                return Ok(new
                {
                    success = true,
                    data = prodRate
                });
            }
        }


        // DELETE: api/ProductRates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductRate(int id)
        {
            var rate = _context.ProductRates.Find(id);
            if(rate is null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "there no rate by this Id"
                });
            }
            else
            {
                if (rate.UserId == GetUserId())
                {
                    var product = _context.Products.Find(rate.ProductId);
                    product!.RateCount -= 1;
                    _context.ProductRates.Remove(rate);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        success = true
                    });
                }
                else
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "you are not the owner of this Rate"
                    });
                }
            }
        }

    }
}
