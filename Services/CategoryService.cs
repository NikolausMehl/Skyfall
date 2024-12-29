using System.Text.Json;

namespace Skyfall.Services;

public interface ICategoryService
{
    List<Category> GetCategories();
    Category GetCategory(string categoryName);
    List<string> GetCategoryWords(string categoryName);
}

public class CategoryService(IWebHostEnvironment env) : ICategoryService
{
    private readonly JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public List<Category> GetCategories()
    {
        var path = Path.Combine(env.ContentRootPath, "categories.json");
        var jsonString = File.ReadAllText(path);

        var categories = JsonSerializer.Deserialize<CategoryList>(jsonString, jsonOptions);
        return categories?.Categories ?? new List<Category>();
    }

    public Category GetCategory(string categoryName)
    {
        List<Category> categories = GetCategories();
        return categories.FirstOrDefault(c => c.Name == categoryName) ?? new Category { Name = "Not found" };
    }

    public List<string> GetCategoryWords(string categoryName)
    {
        Category category = GetCategory(categoryName);
        return category.Words;
    }
}

public class CategoryList
{
    public List<Category> Categories { get; set; } = [];
}
