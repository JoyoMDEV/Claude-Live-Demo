using System.Net;
using System.Net.Http.Json;
using TodoApi.DTOs;
using Xunit;

namespace TodoApi.Tests;

public sealed class TodoEndpointsTests(TodoApiFactory factory)
    : IClassFixture<TodoApiFactory>,
        IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        _client.Dispose();
        return Task.CompletedTask;
    }

    // ── GET /api/todos ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ReturnsEmptyList_WhenNoTodos()
    {
        var response = await _client.GetAsync("/api/todos");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();
        Assert.NotNull(todos);
        Assert.Empty(todos);
    }

    [Fact]
    public async Task GetAll_ReturnsTodos_AfterCreation()
    {
        await _client.PostAsJsonAsync(
            "/api/todos",
            new CreateTodoRequest("Test Todo", "Some description")
        );

        var response = await _client.GetAsync("/api/todos");
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(todos);
        Assert.Single(todos);
        Assert.Equal("Test Todo", todos[0].Title);
    }

    [Fact]
    public async Task GetAll_FiltersBySearch()
    {
        await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest("Alpha Todo", null));
        await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest("Beta Todo", null));

        var response = await _client.GetAsync("/api/todos?search=alpha");
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(todos);
        Assert.All(todos, t => Assert.Contains("Alpha", t.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetAll_SortsByTitle_Ascending()
    {
        await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest("Zebra", null));
        await _client.PostAsJsonAsync("/api/todos", new CreateTodoRequest("Apple", null));

        var response = await _client.GetAsync("/api/todos?sortBy=title&order=asc");
        var todos = await response.Content.ReadFromJsonAsync<List<TodoResponse>>();

        Assert.NotNull(todos);
        Assert.True(todos.Count >= 2);
        var titles = todos.Select(t => t.Title).ToList();
        Assert.Equal(titles.OrderBy(t => t).ToList(), titles);
    }

    // ── GET /api/todos/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsTodo_WhenExists()
    {
        var created = await CreateTodoAsync("Get by Id Todo", "desc");

        var response = await _client.GetAsync($"/api/todos/{created.Id}");
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(todo);
        Assert.Equal(created.Id, todo.Id);
        Assert.Equal("Get by Id Todo", todo.Title);
    }

    [Fact]
    public async Task GetById_Returns404_WhenNotFound()
    {
        var response = await _client.GetAsync($"/api/todos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── POST /api/todos ─────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsTodo_WithValidRequest()
    {
        var request = new CreateTodoRequest("My New Todo", "Optional description");

        var response = await _client.PostAsJsonAsync("/api/todos", request);
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(todo);
        Assert.NotEqual(Guid.Empty, todo.Id);
        Assert.Equal("My New Todo", todo.Title);
        Assert.Equal("Optional description", todo.Description);
        Assert.False(todo.IsCompleted);
    }

    [Fact]
    public async Task Create_Returns422_WhenTitleIsEmpty()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/todos",
            new CreateTodoRequest(string.Empty, null)
        );

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns422_WhenTitleExceeds200Chars()
    {
        var longTitle = new string('A', 201);

        var response = await _client.PostAsJsonAsync(
            "/api/todos",
            new CreateTodoRequest(longTitle, null)
        );

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Create_AcceptsNullDescription()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/todos",
            new CreateTodoRequest("No description todo", null)
        );

        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(todo);
        Assert.Null(todo.Description);
    }

    // ── PUT /api/todos/{id} ─────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsUpdatedTodo_WhenExists()
    {
        var created = await CreateTodoAsync("Old Title", null);

        var response = await _client.PutAsJsonAsync(
            $"/api/todos/{created.Id}",
            new UpdateTodoRequest("New Title", "Updated desc", true)
        );
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(todo);
        Assert.Equal("New Title", todo.Title);
        Assert.Equal("Updated desc", todo.Description);
        Assert.True(todo.IsCompleted);
    }

    [Fact]
    public async Task Update_Returns404_WhenNotFound()
    {
        var response = await _client.PutAsJsonAsync(
            $"/api/todos/{Guid.NewGuid()}",
            new UpdateTodoRequest("Title", null, false)
        );

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_Returns422_WhenTitleIsEmpty()
    {
        var created = await CreateTodoAsync("Valid", null);

        var response = await _client.PutAsJsonAsync(
            $"/api/todos/{created.Id}",
            new UpdateTodoRequest(string.Empty, null, false)
        );

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    // ── DELETE /api/todos/{id} ──────────────────────────────────────────────

    [Fact]
    public async Task Delete_Returns204_WhenExists()
    {
        var created = await CreateTodoAsync("To delete", null);

        var response = await _client.DeleteAsync($"/api/todos/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns404_WhenNotFound()
    {
        var response = await _client.DeleteAsync($"/api/todos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesTodo_FromList()
    {
        var created = await CreateTodoAsync("Delete me", null);
        await _client.DeleteAsync($"/api/todos/{created.Id}");

        var response = await _client.GetAsync($"/api/todos/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task<TodoResponse> CreateTodoAsync(string title, string? description)
    {
        var response = await _client.PostAsJsonAsync(
            "/api/todos",
            new CreateTodoRequest(title, description)
        );
        response.EnsureSuccessStatusCode();
        var todo = await response.Content.ReadFromJsonAsync<TodoResponse>();
        return todo!;
    }
}
