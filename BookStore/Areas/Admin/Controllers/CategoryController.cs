using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(CategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> Add(string catName, IFormFile? imageFile)
        {
            await _categoryService.AddCategoryAsync(catName, imageFile);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _categoryService.DeleteCategoryAsync(id))
            {
                _logger.LogWarning("Delete category {CategoryId} blocked: category has associated books", id);
                TempData["Error"] = "Cannot delete category because it has books associated with it.";
            }

            return RedirectToAction("Index");
        }
    }
}
