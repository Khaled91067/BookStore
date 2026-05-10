using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            BookVM vm = new BookVM();
            vm.Categories = _context.Categories.ToList();
            vm.Publishers = _context.Publishers.ToList();


            
            if (id != 0)
            {
                var book = _context.Books.Find(id);

                if (book == null)
                    return NotFound();

                vm.BookId = book.BookId;
                vm.Title = book.Title;
                vm.Price = book.Price;
                vm.ImageUrl = book.ImageUrl;
                vm.CategoryId = book.CategoryId;
                vm.StockQuantity = book.StockQuantity;
                vm.PublisherId = book.PublisherId;
            }
            ModelState.Clear();
            Console.WriteLine($"ID: {id}");
           
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

            string? fileName = vm.ImageUrl;

            if (vm.ImageFile != null)
            {
                
                    
                    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "Images");
                    var newFileName = Guid.NewGuid().ToString() + "_" + vm.ImageFile.FileName;
                    string path = Path.Combine(uploadsFolder, newFileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        vm.ImageFile.CopyTo(stream);
                    }
                    fileName = newFileName;
               
               
            }

            if(vm.BookId == 0)
            {
                var book = new Book
                {

                    Title = vm.Title,
                    Price = vm.Price,
                    ImageUrl = fileName,
                    CategoryId = vm.CategoryId,
                    
                    StockQuantity = vm.StockQuantity,
                    PublisherId = vm.PublisherId

                };
                _context.Books.Add(book);
            }
            else
            {
                var book = _context.Books.Find(vm.BookId);

                if (book == null)
                    return NotFound();

                book.Title = vm.Title;
                book.Price = vm.Price;
                book.ImageUrl = fileName;
                book.CategoryId = vm.CategoryId;
                book.StockQuantity = vm.StockQuantity;
                book.PublisherId = vm.PublisherId;

            }     

            _context.SaveChanges();
            return RedirectToAction("Index");           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null)
                return NotFound();

            
            if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                var path = Path.Combine(_webHostEnvironment.WebRootPath, "images", book.ImageUrl);

                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}
