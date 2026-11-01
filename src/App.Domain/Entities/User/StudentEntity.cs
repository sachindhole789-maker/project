namespace App.Domain.Entities.User;
// Properties map to stored procedure column names, then Infrastructure maps this row into a result DTO.
public sealed class StudentEntity
{
    public long CustomerId { get; init; }
    public string Name { get; init; } = "";
    public string Email { get; init; } = "";
    public string Mobile { get; init; } = "";
    public string City { get; init; } = "";
}
