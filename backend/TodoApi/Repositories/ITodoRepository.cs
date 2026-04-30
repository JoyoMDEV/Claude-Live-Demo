using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Repositories;

public interface ITodoRepository
{
    Task<IReadOnlyList<Todo>> GetAllAsync(TodoQueryParams query);
    Task<Todo?> GetByIdAsync(Guid id);
    Task<Todo> CreateAsync(Todo todo);
    Task<Todo?> UpdateAsync(Guid id, Action<Todo> update);
    Task<bool> DeleteAsync(Guid id);
}
