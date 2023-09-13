using Bulky.Data.Repository;
using Bulky.Data.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace Bulky.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork unitOfWork;

        [BindProperty]
        public OrderVM OrderVM { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Details(int id)
        {
            OrderVM = new OrderVM
            {
                OrderHeader = unitOfWork.OrderHeader.Get(o => o.Id == id, includeProperties: "ApplicationUser"),
                OrderDetails = unitOfWork.OrderDetail.GetAll(o => o.OrderHeaderId == id, includeProperties: "Product")
            };
            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult UpdateOrder()
        {
            var orderHeader = unitOfWork.OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            
            orderHeader.Name = OrderVM.OrderHeader.Name;
            orderHeader.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeader.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeader.City = OrderVM.OrderHeader.City;
            orderHeader.State = OrderVM.OrderHeader.State;
            orderHeader.PostalCode = OrderVM.OrderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }

            unitOfWork.OrderHeader.Update(orderHeader);
            unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully!";

            return RedirectToAction(nameof(Details), new { id = orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusProcessing);
            unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully!";

            return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = unitOfWork.OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id);

            orderHeader.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusApprovedForDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }
            unitOfWork.OrderHeader.Update(orderHeader);
            unitOfWork.Save();

            unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusShipped);
            unitOfWork.Save();

            TempData["Success"] = "Order Shipped Successfully!";

            return RedirectToAction(nameof(Details), new { id = OrderVM.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = unitOfWork.OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                var refund = service.Create(options);

                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            unitOfWork.Save();

            TempData["Success"] = "Order Cancelled Successfully!";

            return RedirectToAction(nameof(Details), new { id = orderHeader.Id });
        }

        [HttpPost]
        [ActionName("Details")]
        public IActionResult PayNow()
        {
            OrderVM.OrderHeader = unitOfWork.OrderHeader.Get(o => o.Id == OrderVM.OrderHeader.Id, includeProperties: "ApplicationUser");
            OrderVM.OrderDetails = unitOfWork.OrderDetail.GetAll(o => o.OrderHeaderId == OrderVM.OrderHeader.Id, includeProperties: "Product");

            // CAPTURE PAYMENT USING STRIPE FOR A COMPANY
            // https://stripe.com/docs/api/checkout/sessions/create?lang=dotnet
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?id={OrderVM.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?id={OrderVM.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var cart in OrderVM.OrderDetails)
            {
                var sessionLineIten = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(cart.Price * 100), // $20.50 =? 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = cart.Product.Title
                        }
                    },
                    Quantity = cart.Count
                };
                options.LineItems.Add(sessionLineIten);
            }

            var service = new SessionService();
            var session = service.Create(options);

            unitOfWork.OrderHeader.UpdateStripePaymentID(OrderVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
            unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int id)
        {
            // USING STRIPE SESSION ID TO GET ORDER CONFIRMATION
            var orderHeader = unitOfWork.OrderHeader.Get(o => o.Id == id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApprovedForDelayedPayment)
            {
                // this is an order by company
                var service = new SessionService();
                var session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    unitOfWork.OrderHeader.UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    unitOfWork.OrderHeader.UpdateStatus(id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    unitOfWork.Save();
                }
            }

            return View(id);
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser");
            }
            else
            {
                var claimsIdentity=(ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaders = unitOfWork.OrderHeader.GetAll(o => o.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }

            switch (status)
            {
                case "pending":
                    orderHeaders = orderHeaders.Where(o => o.PaymentStatus == SD.PaymentStatusApprovedForDelayedPayment);
                    break;
                case "inprocess":
                    orderHeaders = orderHeaders.Where(o => o.PaymentStatus == SD.StatusProcessing);
                    break;
                case "completed":
                    orderHeaders = orderHeaders.Where(o => o.PaymentStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderHeaders = orderHeaders.Where(o => o.PaymentStatus == SD.StatusApproved || o.PaymentStatus == SD.PaymentStatusApprovedForDelayedPayment);
                    break;
                default:
                    break;
            }

            return Json(new { data = orderHeaders });
        }

        #endregion
    }
}
