using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var users = new List<User>
{
    new User { Id = 1, Name = "Alice", Email = "alice@example.com", Password = "password123" },
    new User { Id = 2, Name = "Bob", Email = "bob@example.com", Password = "password456" }
};

var authenticatedUsers = new Dictionary<string, int>();


// Get all users (only accessible if authenticated)
app.MapGet("/users", (HttpContext httpContext) =>
{
    if (!IsAuthenticated(httpContext)) return Results.Unauthorized();
    return Results.Ok(users);
});

// Get User by id
app.MapGet("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user is not null ? Results.Ok(user) : Results.NotFound();
});

// Add a user
app.MapPost("/users", (User user) =>
{
    var validationContext = new ValidationContext(user);
    var validationResults = new List<ValidationResult>();

    if (!Validator.TryValidateObject(user, validationContext, validationResults, true))
    {
        return Results.ValidationProblem(validationResults.ToDictionary(
            v => v.MemberNames.FirstOrDefault() ?? "Error", 
            v => new[] { v.ErrorMessage! }
        ));
    }

    user.Id = users.Count > 0 ? users.Max(u => u.Id) + 1 : 1;
    users.Add(user);
    return Results.Created($"/users/{user.Id}", user);
});

// Update a user

app.MapPut("/users/{id}", (int id, User updatedUser) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();

    user.Name = updatedUser.Name;
    user.Email = updatedUser.Email;
    return Results.Ok(user);
});

// Delete a user

app.MapDelete("/users/{id}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    if (user is null) return Results.NotFound();

    users.Remove(user);
    return Results.NoContent();
});


// Login endpoint (authenticate user)
app.MapPost("/login", (LoginRequest request) =>
{
    var user = users.FirstOrDefault(u => u.Email == request.Email && u.Password == request.Password);

    if (user == null)
    {
        return Results.Unauthorized();
    }

    // Store userId in the session for future requests
    var sessionKey = Guid.NewGuid().ToString();  // Generate a unique session key
    authenticatedUsers[sessionKey] = user.Id;

    return Results.Ok(new { Message = "Login successful", SessionKey = sessionKey });
});

// Simple Logout (invalidate session)
app.MapPost("/logout", (HttpContext httpContext) =>
{
    var sessionKey = httpContext.Request.Headers["SessionKey"].ToString();
    if (authenticatedUsers.ContainsKey(sessionKey))
    {
        authenticatedUsers.Remove(sessionKey);
        return Results.Ok(new { Message = "Logged out successfully." });
    }

    return Results.Unauthorized();
});

app.Run();

// Check if the user is authenticated by the session key
bool IsAuthenticated(HttpContext httpContext)
{
    var sessionKey = httpContext.Request.Headers["SessionKey"].ToString();
    return authenticatedUsers.ContainsKey(sessionKey);
}


public class User
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Name is required.")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
    [MaxLength(50, ErrorMessage = "Name cannot be longer than 50 characters.")]
    public string Name { get; set; }
    
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } 
}

// Login request model
public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}