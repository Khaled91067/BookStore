using BookStore.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace BookStore.ViewModels
{
    public class CheckOutVM
    {
     
        public int OrderId { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public List<OrderItemVM> OrderItems { get; set; } = new List<OrderItemVM>();
        public string FirstName { get; set; }
        public string LastName { get; set; }

        /*public List<SelectListItem> Countries { get; set; } = new();*/

        public int SelectedCountryId { get; set; }
        public string PhoneNumber { get; set; }

        public string Address { get; set; }
        
        public string Email { get; set; }

    }
}
