using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TodoApi.DTOs;
using TodoApi.Models;
using TodoApi.Repositories;

namespace TodoApi.Endpoints;

public static class TodoEndpoints
{
    public static IEndpointRouteBuilder MapTodoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/todos").WithTags("Todos");

        group.MapGet("/", GetAll).WithName("GetAllTodos").WithSummary("Get all todos");
        group
            .MapGet("/{id:guid}", GetById)
            .WithName("GetTodoById")
            .WithSummary("Get a single todo");
        group.MapPost("/", Create).WithName("CreateTodo").WithSummary("Create a todo");
        group.MapPut("/{id:guid}", Update).WithName("UpdateTodo").WithSummary("Update a todo");
        group.MapDelete("/{id:guid}", Delete).WithName("DeleteTodo").WithSummary("Delete a todo");

        return app;
    }

    private static async Task<Ok<IReadOnlyList<TodoResponse>>> GetAll(
        [AsParameters] TodoQueryParams query,
        ITodoRepository repo
    )
    {
        var todos = await repo.GetAllAsync(query);
        return TypedResults.Ok(todos.Select(MapToResponse).ToList() as IReadOnlyList<TodoResponse>);
    }

    private static async Task<Results<Ok<TodoResponse>, NotFound<ProblemDetails>>> GetById(
        Guid id,
        ITodoRepository repo
    )
    {
        var todo = await repo.GetByIdAsync(id);
        if (todo is null)
            return TypedResults.NotFound(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"Todo with id '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );

        return TypedResults.Ok(MapToResponse(todo));
    }

    private static async Task<Results<Created<TodoResponse>, ValidationProblem>> Create(
        CreateTodoRequest request,
        IValidator<CreateTodoRequest> validator,
        ITodoRepository repo
    )
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        var todo = new Todo { Title = request.Title, Description = request.Description };
        var created = await repo.CreateAsync(todo);
        return TypedResults.Created($"/api/todos/{created.Id}", MapToResponse(created));
    }

    private static async Task<
        Results<Ok<TodoResponse>, NotFound<ProblemDetails>, ValidationProblem>
    > Update(
        Guid id,
        UpdateTodoRequest request,
        IValidator<UpdateTodoRequest> validator,
        ITodoRepository repo
    )
    {
        var result = await validator.ValidateAsync(request);
        if (!result.IsValid)
            return TypedResults.ValidationProblem(result.ToDictionary());

        var updated = await repo.UpdateAsync(
            id,
            t =>
            {
                t.Title = request.Title;
                t.Description = request.Description;
                t.IsCompleted = request.IsCompleted;
            }
        );

        if (updated is null)
            return TypedResults.NotFound(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"Todo with id '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );

        return TypedResults.Ok(MapToResponse(updated));
    }

    private static async Task<Results<NoContent, NotFound<ProblemDetails>>> Delete(
        Guid id,
        ITodoRepository repo
    )
    {
        var deleted = await repo.DeleteAsync(id);
        if (!deleted)
            return TypedResults.NotFound(
                new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = $"Todo with id '{id}' was not found.",
                    Status = StatusCodes.Status404NotFound,
                }
            );

        return TypedResults.NoContent();
    }

    private static TodoResponse MapToResponse(Todo t) =>
        new(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt, t.UpdatedAt);
}
