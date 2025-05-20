using CarvedRockFitness.Models;
using CarvedRockFitness.Repositories;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CarvedRockFitness.Repositories;

public class ProductRepository : IProductRepository
{

    private readonly string? _connectionString;
    private readonly bool _useSampleData;

    public ProductRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
        _useSampleData = string.IsNullOrEmpty(_connectionString);
    }

    public async Task<IEnumerable<Product?>> GetAllAsync()
    {
        if (_useSampleData)
        {
            return GetSampleData();
        }

        var products = new List<Product>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var command = new SqlCommand("SELECT Id, Name, Description, ImageUrl, Price FROM Products", connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.GetString("Description"),
                ImageUrl = reader.GetString("ImageUrl"),
                Price = reader.GetDecimal("Price"),
                Category = reader.GetString("Category")
            });
        }

        return products;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        if (_useSampleData)
        {
            return GetSampleData().FirstOrDefault(p => p.Id == id);
        }

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var command = new SqlCommand("SELECT Id, Name, Description, ImageUrl, Price FROM Products WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", id);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Product
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.GetString("Description"),
                ImageUrl = reader.GetString("ImageUrl"),
                Price = reader.GetDecimal("Price"),
                Category = reader.GetString("Category")
            };
        }
        return null;
    }

    public async Task<IEnumerable<Product?>> GetByCategoryAsync(string? category)
    {
        if (_useSampleData)
        {
            var sampleData = GetSampleData();
            return string.IsNullOrEmpty(category) ? sampleData : sampleData.Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        var products = new List<Product>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = string.IsNullOrEmpty(category)
        ? "SELECT Id, Name, Description, ImageUrl, Price, Category FROM Products"
        : "SELECT Id, Name, Description, ImageUrl, Price, Category FROM Products WHERE Category = @Category";
        var command = new SqlCommand(query, connection);
        if (!string.IsNullOrEmpty(category))
        {
            command.Parameters.AddWithValue("@Category", category);
        }

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32("Id"),
                Name = reader.GetString("Name"),
                Description = reader.GetString("Description"),
                ImageUrl = reader.GetString("ImageUrl"),
                Price = reader.GetDecimal("Price"),
                Category = reader.GetString("Category")
            });
        }

        return products;
    }

    private IEnumerable<Product> GetSampleData()
    {
        return new List<Product>
        {
        new Product { Id = 1, Name = "Sample Product 1", Description = "Sample Product Description 1", ImageUrl = "images/products/boots/shutterstock_66842440.jpg", Price = 9.99m, Category = "Clothing" },
        new Product { Id = 2, Name = "Sample Product 2", Description = "Sample Product Description 2", ImageUrl = "images/products/boots/shutterstock_475046062.jpg", Price = 19.99m, Category = "Clothing" },
        new Product { Id = 3, Name = "Sample Product 3", Description = "Sample Product Description 3", ImageUrl = "images/products/boots/shutterstock_1121278055.jpg", Price = 29.99m, Category = "Clothing" },
        new Product { Id = 4, Name = "Sample Product 4", Description = "Sample Product Description 4", ImageUrl = "images/products/boots/shutterstock_66842440.jpg", Price = 39.99m, Category = "Footwear" },
        new Product { Id = 5, Name = "Sample Product 5", Description = "Sample Product Description 5", ImageUrl = "images/products/boots/shutterstock_222721876.jpg", Price = 49.99m, Category = "Footwear" },
        new Product { Id = 6, Name = "Sample Product 6", Description = "Sample Product Description 6", ImageUrl = "images/products/boots/shutterstock_475046062.jpg", Price = 59.99m, Category = "Footwear" },
        new Product { Id = 7, Name = "Sample Product 7", Description = "Sample Product Description 7", ImageUrl = "images/products/climbing gear/shutterstock_6170527.jpg", Price = 69.99m, Category = "Equipment" },
        new Product { Id = 8, Name = "Sample Product 8", Description = "Sample Product Description 8", ImageUrl = "images/products/climbing gear/shutterstock_48040747.jpg", Price = 79.99m, Category = "Equipment" },
        new Product { Id = 9, Name = "Sample Product 9", Description = "Sample Product Description 9", ImageUrl = "images/products/climbing gear/shutterstock_64998481.jpg", Price = 89.99m, Category = "Equipment" }
        };
    }

}
