using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace EVChargingBookingAPI.Middleware
{
    /// <summary>
    /// Attribute for role-based authorization
    /// </summary>
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // For now, we'll implement a simple header-based authentication
            // In production, you should use proper JWT tokens
            var userRole = context.HttpContext.Request.Headers["User-Role"].FirstOrDefault();
            var userId = context.HttpContext.Request.Headers["User-Id"].FirstOrDefault();

            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
            {
                context.Result = new UnauthorizedObjectResult("Authentication required");
                return;
            }

            if (!_roles.Contains(userRole))
            {
                context.Result = new ForbiddenResult();
                return;
            }

            // Store user info for use in controllers
            context.HttpContext.Items["UserId"] = userId;
            context.HttpContext.Items["UserRole"] = userRole;
        }
    }

    /// <summary>
    /// Custom result for forbidden access
    /// </summary>
    public class ForbiddenResult : ActionResult
    {
        public override void ExecuteResult(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = 403;
        }
    }
}