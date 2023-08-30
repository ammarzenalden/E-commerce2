using E2.Data;
using E2.DTO;
using E2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E2.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = "Bearer")]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }
    private int GetUserId()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userId!);
    }
    [HttpPost("buy")]
    public async Task<ActionResult<Payment>> PostPayment(PaymentDto[] orders)
    {
        List<PaymentProduct> allPayments = new();
        float totalPrice = 0;
        int totalProduct = 0;
        foreach (var order in orders)
        {

            var prod = await _context.Products.FindAsync(order.ProductId);
            if (prod!.Deleted)
            {
                return BadRequest("there is no product");
            }
            if(prod.Available == 0 )
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"product whit id: {prod.Id} is out of stock"
                });
            }
            if (prod.Available < order.Amount)
            {
                return BadRequest(new
                {
                    success = false,
                    message = $"the requested quantity is not available of product whit id: {prod.Id}"
                });
            }

        }
        Payment payment = new();
        payment.DateOfSell = DateTime.UtcNow;
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        foreach (var order in orders)
        {
            var prod = await _context.Products.FindAsync(order.ProductId);
            prod!.Available = prod.Available - order.Amount;
            PaymentProduct paymentProduct = new();
            paymentProduct.ProductId = order.ProductId;
            paymentProduct.Amount = order.Amount;
            paymentProduct.PriceOfSell = prod.Price;
            paymentProduct.BuyerId = GetUserId();
            _context.Products.Update(prod);
            paymentProduct.PaymentId = payment.Id;
            paymentProduct.SellerId = prod.UserId;
            paymentProduct.DateOfsell = payment.DateOfSell;
            await _context.PaymentProducts.AddAsync(paymentProduct);
            allPayments.Add(paymentProduct);
            totalPrice += prod.Price * order.Amount;
            totalProduct += order.Amount;
        }
        
        await _context.SaveChangesAsync();
        
        return Ok(new
        {
            success = true,
            data = allPayments,
            price = totalPrice,
            numberOfProduct = totalProduct
        });

    }
    [HttpGet("GetAllPaymentsForseller")]
    public async Task<ActionResult<Payment>> GetPayments()
    {
        var paymetnProduct = await _context.PaymentProducts.Where(x => x.SellerId == GetUserId()).ToListAsync();
        int userSignedIn = GetUserId();
        if (!paymetnProduct.Any())
        {
            return NotFound(new
            {
                success = false,
                message = "there is no payments"
            });
        }
        return Ok(new
        {
            success = true,
            thePayment = paymetnProduct
        });
        
        
        
    }
    [HttpGet("GetPaymetnWithDetails/{id}")]
    public async Task<ActionResult<Payment>> GetPaymentDetails(int id)
    {
        int userSignedIn = GetUserId();
        var paymentProduct = await _context.PaymentProducts.Where(p => p.PaymentId == id && p.BuyerId == userSignedIn ).ToListAsync();
        DataResponse<List<PaymentProduct>> res = new(data:paymentProduct,message:"");
        return Ok(res);
    }
    [HttpGet("GetAllPaymentsForBuyer")]
    public async Task<ActionResult<IEnumerable<Payment>>> GetAllPayments([FromQuery] PaginationParams @params)
    {
        int userSignedIn = GetUserId();
        var paymentProduct = await _context.PaymentProducts.Where(p => p.BuyerId == userSignedIn).ToListAsync();
        List<Payment> payments = new();
        if (paymentProduct.Any())
        {
            var payment = await _context.Payments.OrderBy(p=>p.Id).ToListAsync();
            var paymentIds = paymentProduct.Select(p => p.PaymentId).Distinct().ToList();
            foreach (int item in paymentIds)
            {
                payments.Add(payment.Where(x => x.Id == item).SingleOrDefault()!);
            }
            payments = payments.Skip((@params.Page - 1) * @params.ItemPerPage).Take(@params.ItemPerPage).ToList();
            DataResponse<List<Payment>> res = new(data: payments, message: "");
            return Ok(res);
        }
        else
        {
            DataResponse<List<PaymentProduct>> res = new(data: paymentProduct, message: "");
            return Ok(res);
        }

        
    }
}
