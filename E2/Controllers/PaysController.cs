using E2.Data;
using E2.DTO;
using E2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PayPal.Api;
using PayPalCheckoutSdk.Orders;
using System;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using static NuGet.Packaging.PackagingConstants;

namespace E2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PaysController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaysController(ApplicationDbContext context,  IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        private int GetUserId()
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId!);
        }
        [HttpPost("buy")]
        public async Task<ActionResult<Models.Payment>> PostPayment(PaymentDto[] orders)
        {
            // Existing code ...
            //List<PaymentProduct> allPayments = new();
            float totalPrice = 0;
            //int totalProduct = 0;
            foreach (var order in orders)
            {
                
                var prod = await _context.Products.FindAsync(order.ProductId);
                if (prod!.Deleted)
                {
                    return BadRequest("there is no product");
                }
                if (prod.Available == 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"product :{prod.Name} is out of stock"
                    });
                }
                if (prod.Available < order.Amount)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"the requested quantity is not available of product :{prod.Name}"
                    });
                }

            }
            //Models.Payment payment = new();
            //payment.DateOfSell = DateTime.UtcNow;
            //_context.Payments.Add(payment);
            //await _context.SaveChangesAsync();
            ItemList itemList = new ItemList();
            itemList.items = new List<PayPal.Api.Item>();
            List<PaymentDto> products = new();
            foreach (var order in orders)
            {
                
                var prod = await _context.Products.FindAsync(order.ProductId);
                totalPrice += prod!.Price * order.Amount;
                products.Add(order);
                itemList.items.Add(new PayPal.Api.Item
                {
                    name = prod.Name,
                    price = prod.Price.ToString(),
                    currency = "USD",
                    quantity = order.Amount.ToString(),
                    
                });
            }
            Dictionary<string,int> buyerid = new Dictionary<string,int>();
            buyerid.Add("buyerid", GetUserId());
            var buyeridJson = JsonConvert.SerializeObject(buyerid);
            var productsJson = JsonConvert.SerializeObject(products);
            // Initialize PayPal configuration
            var config = new Dictionary<string, string>
                {
                    { "mode", "sandbox" }, // Use "live" for production
                    { "clientId", _configuration.GetValue<string>("PayPal:ClientId")! },
                    { "clientSecret", _configuration.GetValue<string>("PayPal:clientSecret")! }
                };

            // Create API context
            var apiContext = new APIContext(new OAuthTokenCredential(config).GetAccessToken())
            {
                Config = config
            };

            // Create payment details
            var ede = new PayPal.Api.Payment
            {
                intent = "sale",
                payer = new PayPal.Api.Payer { payment_method = "paypal" },
                transactions = new List<Transaction>
        {
            new Transaction
            {
                amount = new Amount
                {
                    currency = "USD", // Set the appropriate currency code
                    total = totalPrice.ToString(),
                    // Pass the total price of the order
                },
                description = "Payment for products",
                item_list = itemList,
                custom = productsJson,
                invoice_number = buyeridJson
                
                // Provide a description for the payment
            }
        },
                redirect_urls = new RedirectUrls
                {
                    return_url = _configuration.GetValue<string>("PayPal:RedirectUrl")!,
                    cancel_url = _configuration.GetValue<string>("PayPal:RedirectUrl")!
                }
            };

            // Create the payment
            var createdPayment =  ede.Create(apiContext);

            // Extract the approval URL
            var approvalUrl = createdPayment.links.FirstOrDefault(l => l.rel == "approval_url");

            if (approvalUrl != null)
            {
                // Redirect the user to the PayPal approval URL
                return Ok(approvalUrl.href);
            }
            else
            {
                // Failed to create payment
                return BadRequest("Failed to create PayPal payment.");
            }
        }
        [HttpGet("paypal-complete")]
        [AllowAnonymous]
        public async Task<IActionResult> PayPalComplete(string paymentId, string PayerID)
        {
            // Initialize PayPal configuration
            var config = new Dictionary<string, string>
                {
                    { "mode", "sandbox" }, // Use "live" for production
                    { "clientId", _configuration.GetValue<string>("PayPal:ClientId")! },
                    { "clientSecret", _configuration.GetValue<string>("PayPal:clientSecret")! }
                };

            // Create API context
            var apiContext = new APIContext(new OAuthTokenCredential(config).GetAccessToken())
            {
                Config = config
            };

            // Get the payment details
            var payment =  PayPal.Api.Payment.Get(apiContext, paymentId);
            // Execute the payment
            var execution = new PaymentExecution { payer_id = PayerID };
            payment.Execute(apiContext, execution);

            // Process the executedPayment and perform necessary actions
            // For example, update the payment status in your database

            var productJson = payment.transactions[0].custom;
            var buyerid = payment.transactions[0].invoice_number;
            
            List<PaymentDto> products = JsonConvert.DeserializeObject<List<PaymentDto>>(productJson)!;
            Dictionary<string, string> b = JsonConvert.DeserializeObject<Dictionary<string, string>>(buyerid)!;
            int buyer = int.Parse(b["buyerid"]);
            float totalPrice = 0;
            int totalProduct = 0;
            Models.Payment pay = new();
            pay.DateOfSell = DateTime.UtcNow;
            _context.Payments.Add(pay);
            await _context.SaveChangesAsync();
            foreach (var order in products)
            {
                
                var prod = await _context.Products.FindAsync(order.ProductId);
                prod!.Available = prod.Available - order.Amount;
                PaymentProduct paymentProduct = new();
                paymentProduct.ProductId = order.ProductId;
                paymentProduct.Amount = order.Amount;
                paymentProduct.PriceOfSell = prod.Price;
                paymentProduct.BuyerId = buyer;
                _context.Products.Update(prod);
                paymentProduct.PaymentId = pay.Id;
                paymentProduct.SellerId = prod.UserId;
                paymentProduct.DateOfsell = pay.DateOfSell;
                await _context.PaymentProducts.AddAsync(paymentProduct);
                totalPrice += prod.Price * order.Amount;
                totalProduct += order.Amount;
            }
            pay.TotalPrice = totalPrice;
            pay.NumberOfProduct = totalProduct;
            _context.Payments.Update(pay);
            await _context.SaveChangesAsync();
            return Ok("Payment completed successfully.");
        }


    }
}
