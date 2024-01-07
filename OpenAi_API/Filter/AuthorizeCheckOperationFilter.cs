using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OpenAi_API.Filter
{
    /// <summary>
    ///在使用Swagger时，根据控制器的AuthorizeAttribute来决定是否需要在特定Api中添加Authorize
    /// </summary>
    public class AuthorizeCheckOperationFilter : IOperationFilter
    {
        private readonly EndpointDataSource _endpointDataSource;

        public AuthorizeCheckOperationFilter(EndpointDataSource endpointDataSource)
        {
            _endpointDataSource = endpointDataSource;
        }
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var descriptor = _endpointDataSource.Endpoints.FirstOrDefault(x =>
                x.Metadata.GetMetadata<ControllerActionDescriptor>() == context.ApiDescription.ActionDescriptor);
            var hasAuthorize = descriptor.Metadata.GetMetadata<AuthorizeAttribute>() != null;

            var allowAnon = descriptor.Metadata.GetMetadata<AllowAnonymousAttribute>() != null;
            if (!hasAuthorize || allowAnon) return;
            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecurityScheme {Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"}
                        }
                    ] = new[] { "chatcompletion_api" , "offline_access" }
                }
            };
        }
    }
}
