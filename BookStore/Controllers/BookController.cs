using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public BookController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment=webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            var books = _context.Books
        .Select(b => new BookVM
        {
            BookId = b.BookId,
            Title = b.Title,
            Price = b.Price,
            StockQuantity = b.StockQuantity,
            ImageUrl = b.ImageUrl
        })
        .ToList();

            return View(books);
        }

        public async Task<IActionResult> AddEdit(int id)
        {
            BookVM vm = new BookVM();
            vm.Categories = _context.Categories.ToList();
            vm.Publishers = _context.Publishers.ToList();


            return View(vm);


        }

        [HttpPost]
        public IActionResult AddEdit(BookVM vm)
        {
            if (!ModelState.IsValid) {
                vm.Categories = _context.Categories.ToList();
                vm.Publishers = _context.Publishers.ToList();

                return View(vm);
            }
                

            if (vm.ImageFile != null)
            {
                
                    
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + vm.ImageFile.FileName;
                    string path = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        vm.ImageFile.CopyTo(stream);
                    }
                    vm.ImageUrl = uniqueFileName;
               
               
            }



            var book = new Book
            { 
             
                Title = vm.Title,
                Price = vm.Price,
                ImageUrl = vm.ImageUrl,
                CategoryId = vm.CategoryId,
                
                StockQuantity = vm.StockQuantity,
                PublisherId = vm.PublisherId

            };

            _context.Books.Add(book);
            _context.SaveChanges();

            return RedirectToAction("Index");

            
            
           
        }


    }
}
