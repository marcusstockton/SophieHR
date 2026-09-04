using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace SophieHR.Api.Extensions
{
    public static class OpenApiExtensions
    {
        public static IServiceCollection AddCustomOpenApi(this IServiceCollection services)
        {
            services.AddOpenApi(options =>
            {
                // Required for when generating the client.ts file using nswagstudio.
                options.AddOperationTransformer((operation, context, cancellationToken) =>
                {
                    if (context.Description.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor cad)
                    {
                        operation.OperationId = cad.ActionName;
                    }

                    return Task.CompletedTask;
                });

                // Required for the missing FileParameter on the client.ts that worked in swagger but not in openapi. This is because the OpenAPI spec does not have a file type, so we need to transform the schema to be a string with format binary.
                options.AddOperationTransformer((operation, context, cancellationToken) =>
                {
                    if (operation.RequestBody?.Content.TryGetValue("multipart/form-data", out var mediaType) == true
                        && mediaType.Schema?.Properties is not null)
                    {
                        var formFileParamNames = context.Description.ParameterDescriptions
                            .Where(p => p.Type == typeof(IFormFile) || p.Type == typeof(IFormFileCollection))
                            .Select(p => p.Name);

                        foreach (var paramName in formFileParamNames)
                        {
                            if (mediaType.Schema.Properties.ContainsKey(paramName))
                            {
                                mediaType.Schema.Properties[paramName] = new OpenApiSchema
                                {
                                    Type = JsonSchemaType.String,
                                    Format = "binary"
                                };
                            }
                        }
                    }

                    return Task.CompletedTask;
                });

                // Enum transformer - include enum values so NSwag generates TypeScript enums instead of number types
                options.AddSchemaTransformer((schema, context, cancellationToken) =>
                {
                    var type = context.JsonTypeInfo.Type;

                    if (!type.IsEnum)
                    {
                        return Task.CompletedTask;
                    }

                    var enumIntValues = Enum.GetValues(type)
                        .Cast<object>()
                        .Select(value => Convert.ToInt32(value))
                        .ToList();

                    var enumNames = Enum.GetNames(type);

                    // Ensure vendor extensions collection exists
                    schema.Extensions ??= new Dictionary<string, IOpenApiExtension>();

                    // Add the enum names (used by NSwag to create named enums)
                    schema.Extensions["x-enumNames"] =
                        new JsonNodeExtension(
                            new JsonArray(
                                enumNames
                                    .Select(name => (JsonNode?)JsonValue.Create(name))
                                    .ToArray()));

                    schema.Extensions["x-enum-varnames"] =
                        new JsonNodeExtension(
                            new JsonArray(
                                enumNames
                                    .Select(name => (JsonNode?)JsonValue.Create(name))
                                    .ToArray()));

                    // Populate the standard OpenAPI 'enum' property with numeric values so TS client generator recognizes it as an enum
                    // Add as an OpenAPI extension named "enum" so it is serialized into the document. This avoids needing the Microsoft.OpenApi.Any types.
                    schema.Extensions["enum"] =
                        new JsonNodeExtension(
                            new JsonArray(
                                enumIntValues
                                    .Select(v => (JsonNode?)JsonValue.Create(v))
                                    .ToArray()));

                    return Task.CompletedTask;
                });
            });

            return services;
        }
    }
}
