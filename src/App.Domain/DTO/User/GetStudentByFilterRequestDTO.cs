namespace App.Domain.DTO.User;
public sealed record GetStudentByFilterRequestDTO(string? Name, string? Email, int PageNumber = 1, int PageSize = 20);
