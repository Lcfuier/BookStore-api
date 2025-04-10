using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BookStore.Api.Extensions
{
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if the action has [AllowAnonymous]
            var hasAllowAnonymous = context.MethodInfo
                .GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
                || context.MethodInfo.DeclaringType
                    .GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

            if (hasAllowAnonymous)
            {
                // Do not apply security requirements for [AllowAnonymous]
                return;
            }

            // Check if the action or controller has [Authorize]
            var hasAuthorize = context.MethodInfo
                .GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                || context.MethodInfo.DeclaringType
                    .GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            if (hasAuthorize)
            {
                // Apply security requirements for methods with [Authorize]
                operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                }
            };
            }
        }
    }
}
