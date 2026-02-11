using ElectronicInvoicing.Domain.Contracts.ServicesContracts;
using ElectronicInvoicing.Domain.DTOs.Auth;
using Microsoft.AspNetCore.Mvc;

namespace ElectronicInvoicing.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentication")
            .WithOpenApi();

        // POST: /api/auth/register
        group.MapPost("/register", Register)
            .WithName("Register")
            .WithSummary("Register a new user")
            .WithDescription("Creates a new user account for a company")
            .Produces<AuthResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .AllowAnonymous();

        // POST: /api/auth/login
        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Login user")
            .WithDescription("Authenticates user and returns JWT token")
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterDto registerDto,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            var result = await serviceManager.AuthService.RegisterAsync(registerDto);
            return Results.Created($"/api/users/{result.UserId}", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred during registration: {ex.Message}");
        }
    }

    private static async Task<IResult> Login(
        [FromBody] LoginDto loginDto,
        [FromServices] IServiceManager serviceManager)
    {
        try
        {
            var result = await serviceManager.AuthService.LoginAsync(loginDto);
            return Results.Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"An error occurred during login: {ex.Message}");
        }
    }
}
