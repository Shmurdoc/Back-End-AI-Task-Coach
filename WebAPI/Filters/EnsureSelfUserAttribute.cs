using Application.IRepositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

namespace WebAPI.Filters;

/// <summary>
/// Rejects any request that references a user other than the authenticated caller.
/// Covers the route parameter named "userId" and a "UserId" property on body arguments
/// (records, DTOs). Entity-id parameters are untouched - those are enforced by ownership
/// checks in the application services.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class EnsureSelfUserAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userContext = context.HttpContext.RequestServices.GetRequiredService<IUserContext>();
        var currentUserId = userContext.GetCurrentUserId();

        if (context.RouteData.Values.TryGetValue("userId", out var routeValue) &&
            Guid.TryParse(routeValue?.ToString(), out var routeUserId) &&
            routeUserId != currentUserId)
        {
            context.Result = Forbidden();
            return;
        }

        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var property = argument.GetType().GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance);
            if (property?.PropertyType == typeof(Guid) &&
                property.GetValue(argument) is Guid bodyUserId &&
                bodyUserId != currentUserId)
            {
                context.Result = Forbidden();
                return;
            }
        }

        await next();
    }

    private static ObjectResult Forbidden() => new(new { success = false, error = "You may only access your own user data." })
    {
        StatusCode = StatusCodes.Status403Forbidden
    };
}
