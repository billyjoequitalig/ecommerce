using Jrq.DataAccess.Repository.IRepository;
using Jrq.Models;
using Jrq.Models.Models.ViewModels;
using Jrq.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Jrq.Ecommerce.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _UnitOfWork;
        [BindProperty]
        public ShoppingCartViewModel ShoppingCartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _UnitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            //var a = ShoppingCartVM.ShoppingCartList;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }
        public IActionResult Summary()
        {
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _UnitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product"),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId);
            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(ShoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (System.Security.Claims.ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            ShoppingCartVM.ShoppingCartList = _UnitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId,
                includeProperties: "Product");

            ShoppingCartVM.OrderHeader.OrderDate = System.DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ApplicationUser applicationUser = _UnitOfWork.ApplicationUser.Get(u => u.Id == userId);

            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            if (applicationUser.CompanyID.GetValueOrDefault() == 0)
            {//it is a regular account and we need to capture payment
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
                ShoppingCartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
            }
            _UnitOfWork.OrderHeader.Add(ShoppingCartVM.OrderHeader);
            _UnitOfWork.Save();
            foreach (var cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _UnitOfWork.OrderDetail.Add(orderDetail);
                _UnitOfWork.Save();
            }

            if (applicationUser.CompanyID.GetValueOrDefault() == 0)
            {//it is a regular account and we need to capture payment
                //Stripe Logic Here
                var domain = "https://localhost:7227/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                    Locale = "en-GB"
                };
                foreach (var item in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new Stripe.Checkout.SessionLineItemOptions
                    {
                        PriceData = new Stripe.Checkout.SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "php",
                            ProductData = new Stripe.Checkout.SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                var service = new Stripe.Checkout.SessionService();
                Stripe.Checkout.Session session = service.Create(options);
                _UnitOfWork.OrderHeader.UpdateStripePaymentID(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _UnitOfWork.Save();
                Response.Headers.Append("Location", session.Url);
                return new StatusCodeResult(303);
            }
            return RedirectToAction(nameof(OrderConfirmation), new { id = ShoppingCartVM.OrderHeader.Id });
        }
        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _UnitOfWork.OrderHeader.Get(u => u.Id == id, includeProperties: "ApplicationUser");
            if (orderHeader == null)
            {
                return NotFound(); // Handle invalid order ID
            }
            if (orderHeader.PaymentStatus!= StaticDetails.PaymentStatusApproved)
            { //this is an order by customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower()=="paid")
                {
                    _UnitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    _UnitOfWork.OrderHeader.UpdateStatus(id, StaticDetails.PaymentStatusApproved, StaticDetails.PaymentStatusApproved);
                    _UnitOfWork.Save(); 
                }
            }
            List<ShoppingCart> shoppingCarts = _UnitOfWork.ShoppingCart
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _UnitOfWork.ShoppingCart.RemoveRange(shoppingCarts);
            _UnitOfWork.Save();
            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _UnitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _UnitOfWork.ShoppingCart.Update(cartFromDb);
            _UnitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _UnitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            if (cartFromDb.Count <= 1)
            {
                _UnitOfWork.ShoppingCart.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _UnitOfWork.ShoppingCart.Update(cartFromDb);
            }
            _UnitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _UnitOfWork.ShoppingCart.Get(u => u.Id == cartId);
            _UnitOfWork.ShoppingCart.Remove(cartFromDb);
            _UnitOfWork.Save();
            return RedirectToAction(nameof(Index));

        }
        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
            {
                return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count <= 100)
                {
                    return shoppingCart.Product.Price;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
    }
}
