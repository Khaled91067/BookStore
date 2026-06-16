using BookStore.Controllers;
using BookStore.Data;
using BookStore.Models;
using BookStore.Services.Implementaion;
using BookStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminBookController : Controller
    {
        
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly BookService _bookService;

        public AdminBookController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, BookService bookService) 
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _bookService = bookService;
        }

        [HttpGet]
        public async Task<IActionResult> AddEdit(int id)
        {
            

            BookVM vm = await _bookService.GetAddEdit(id);

            if(vm == null)
                return NotFound();

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> AddEdit(BookVM vm)
        {
            

            if (!ModelState.IsValid)
            {
                await _bookService.LoadDataAsync(vm);

                return View(vm);
            }

            if (!await _bookService.SaveAsync(vm))
            {
               return NotFound();
            }
            
            return RedirectToAction("Index","UserBook", routeValues: new { area = "" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            

            if (!await _bookService.DeleteAsync(id))
            {
                return NotFound();
            }

            return RedirectToAction("Index", "UserBook", routeValues: new { area = "" });
        }
        
    }
}
