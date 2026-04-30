using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.DTOs;
using TodoApi.Models;

namespace TodoApi.Repositories;

public sealed class TodoRepository(TodoDbContext db) : ITodoRepository
{
    public async Task<IReadOnlyList<Todo>> GetAllAsync(TodoQueryParams query)
    {
        var q = db.Todos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.ToLower();
            q = q.Where(t =>
                t.Title.ToLower().Contains(term)
                || (t.Description != null && t.Description.ToLower().Contains(term))
            );
        }

        var descending = string.Equals(query.Order, "desc", StringComparison.OrdinalIgnoreCase);

        q = query.SortBy?.ToLower() switch
        {
            "title" => descending ? q.OrderByDescending(t => t.Title) : q.OrderBy(t => t.Title),
            "iscompleted" => descending
                ? q.OrderByDescending(t => t.IsCompleted)
                : q.OrderBy(t => t.IsCompleted),
            _ => descending
                ? q.OrderByDescending(t => t.CreatedAt)
                : q.OrderBy(t => t.CreatedAt),
        };

        return await q.ToListAsync();
    }

    public async Task<Todo?> GetByIdAsync(Guid id) =>
        await db.Todos.FindAsync(id);

    public async Task<Todo> CreateAsync(Todo todo)
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync();
        return todo;
    }

    public async Task<Todo?> UpdateAsync(Guid id, Action<Todo> update)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null)
            return null;

        update(todo);
        todo.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
        return todo;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var todo = await db.Todos.FindAsync(id);
        if (todo is null)
            return false;

        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return true;
    }
}
