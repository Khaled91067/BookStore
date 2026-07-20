using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class BookController : Controller
    {
        private readonly BookService _bookService;

        public BookController(BookService bookService)
        {
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            var booksPage = await _bookService.GetAllBooksAsync(search, page);
            return View(booksPage);
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id, string? returnUrl)
        {
            BookVM? vm = await _bookService.GetAddEdit(id);

            if (vm == null)
                return NotFound();

            ViewBag.ReturnUrl = returnUrl;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(BookVM vm, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                await _bookService.LoadDataAsync(vm);
                ViewBag.ReturnUrl = returnUrl;
                return View(vm);
            }

            if (!await _bookService.SaveAsync(vm))
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl)
        {
            if (!await _bookService.DeleteAsync(id))
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
