using CarvedRockFitness.Models;
using CarvedRockFitness.Repositories;
using Microsoft.Data.SqlClient;
using System.Data;

public class SqlCartRepository : ICartRepository
{
    private readonly string _connectionString;

    public SqlCartRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection string is missing.");
    }

    public async Task<List<CartItem>> GetCartAsync(string sessionId, string? userId)
    {
        var items = new List<CartItem>();
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = @"
            SELECT Id, UserId, ProductId, ProductName, Price, Quantity, AddedAt
            FROM CartItems
            WHERE UserId = @UserId OR (@UserId IS NULL AND UserId = @SessionId)";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
        command.Parameters.AddWithValue("@SessionId", sessionId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            items.Add(new CartItem
            {
                Id = reader.GetInt32(0),
                UserId = reader.IsDBNull(1) ? null : reader.GetString(1),
                ProductId = reader.GetInt32(2),
                ProductName = reader.GetString(3),
                Price = reader.GetDecimal(4),
                Quantity = reader.GetInt32(5),
                AddedAt = reader.GetDateTime(6)
            });
        }
        return items;
    }

    public async Task SaveCartAsync(string sessionId, string? userId, List<CartItem> items)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        foreach (var item in items)
        {
            if (item.Id == 0) // New item
            {
                var insertQuery = @"
                    INSERT INTO CartItems (UserId, ProductId, ProductName, Price, Quantity, AddedAt)
                    VALUES (@UserId, @ProductId, @ProductName, @Price, @Quantity, @AddedAt);
                    SELECT SCOPE_IDENTITY();";
                using var command = new SqlCommand(insertQuery, connection);
                command.Parameters.AddWithValue("@UserId", (object?)(userId ?? sessionId) ?? DBNull.Value);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@ProductName", item.ProductName);
                command.Parameters.AddWithValue("@Price", item.Price);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@AddedAt", item.AddedAt);
                item.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
            }
            else // Update existing item
            {
                var updateQuery = @"
                    UPDATE CartItems
                    SET Quantity = @Quantity
                    WHERE Id = @Id";
                using var command = new SqlCommand(updateQuery, connection);
                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                command.Parameters.AddWithValue("@Id", item.Id);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task ClearCartAsync(string sessionId, string? userId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        var query = "DELETE FROM CartItems WHERE UserId = @UserId OR (@UserId IS NULL AND UserId = @SessionId)";
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
        command.Parameters.AddWithValue("@SessionId", sessionId);
        await command.ExecuteNonQueryAsync();
    }
}