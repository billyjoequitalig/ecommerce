using Jrq.DataAccess.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jrq.Ecommerce.Areas.Customer.Components
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartCountViewComponent(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IViewComponentResult Invoke()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Content("0");
            }

            var cartCount = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId).Count();
            return Content(cartCount.ToString());
        }
    }
}