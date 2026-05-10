using BookStore.Controllers;
using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminBookController : Controller
    {
        
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AdminBookController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }
        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            BookVM vm = new BookVM();
            vm.Categories = _context.Categories.ToList();
            vm.Publishers = _context.Publishers.ToList();

            vm.Authors = await _context.Authors
                    .Select(a => new SelectListItem
                    {
                        Value = a.AuthorId.ToString(),
                        Text = a.Name
                    })
                    .ToListAsync();


            if (id != 0)
            {
                /*var book = _context.Books.Find(id);*/
                var book = _context.Books
                    .Include(b => b.BookAuthors)
                    .FirstOrDefault(b => b.BookId == id);

                if (book == null)
                    return NotFound();

                vm.BookId = book.BookId;
                vm.Title = book.Title;
                vm.Price = book.Price;
                vm.ImageUrl = book.ImageUrl;
                vm.CategoryId = book.CategoryId;
                vm.StockQuantity = book.StockQuantity;
                vm.PublisherId = book.PublisherId;
                
                vm.SelectedAuthorIds = book.BookAuthors
                    .Select(ba => ba.AuthorId)
                    .ToList();

            }
            /*ModelState.Clear();*/

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(BookVM vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = _context.Categories.ToList();
                vm.Publishers = _context.Publishers.ToList();
                vm.Authors = await _context.Authors
                .Select(a => new SelectListItem
                {
                    Value = a.AuthorId.ToString(),
                    Text = a.Name
                })
                   .ToListAsync();


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

            if (vm.BookId == 0)
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

                await _context.SaveChangesAsync();

                foreach (var authorId in vm.SelectedAuthorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.BookId,
                        AuthorId = authorId
                    });
                }
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

                var existingAuthors = _context.BookAuthors
                 .Where(ba => ba.BookId == book.BookId);

                _context.BookAuthors.RemoveRange(existingAuthors);

                foreach (var authorId in vm.SelectedAuthorIds)
                {
                    _context.BookAuthors.Add(new BookAuthor
                    {
                        BookId = book.BookId,
                        AuthorId = authorId
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index","UserBook", routeValues: new { area = "" });
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
