namespace BookStore.ViewModels
{
    public class OrderVM
    {
        public DateTime OrderDate { get; set; }
        public List<OrderItemVM> OrderItems { get; set; } 

        public IEnumerable<BookVM> Books { get; set; }
       
    }
}
