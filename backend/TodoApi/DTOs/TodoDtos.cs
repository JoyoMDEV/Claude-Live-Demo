namespace TodoApi.DTOs;

public sealed record CreateTodoRequest(string Title, string? Description);

public sealed record UpdateTodoRequest(string Title, string? Description, bool IsCompleted);

public sealed record TodoResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public sealed record TodoQueryParams(
    string? Search = null,
    string? SortBy = null,
    string? Order = "asc"
);
