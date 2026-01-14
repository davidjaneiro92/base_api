using System;
using System.Collections.Generic;
using System.Text;

namespace ERP.BuildingBlocks.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModularControllers(
            this IServiceCollection services, IConfiguration config
        )
        {
            services


                .AddControllers(opts =>
                    opts.Conventions.Add(new RouteTokenTransformerConvention(new KebabCaseTransformer()))
                )
                .ConfigureApplicationPartManager(manager => {
                    manager.FeatureProviders.Add(new SharedControllerFeatureProvider());
                })
                .AddMvcOptions(options => {
                    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, propertyName) =>
                        throw new CustomException($"{propertyName}: value '{value}' is invalid.", statusCode: HttpStatusCode.BadRequest));
                });
            ;

            services.AddRouting(options => options.LowercaseUrls = true);

            return services;
        }

        public static IServiceCollection AddDocApi(
            this IServiceCollection services
        )
        {
            services
                //.AddOpenApi()
                .AddEndpointsApiExplorer()
                .AddSwaggerGen(o => {
                    //var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

                    //o.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename), true);
                    o.SchemaFilter<DescriptionSchemaFilter>();
                    //o.ExampleFilters();

                    o.DescribeAllParametersInCamelCase();

                    o.UseInlineDefinitionsForEnums();

                    o.OperationFilter<CustomHeaderFilter>();
                    o.DocumentFilter<OrderTagsDocumentFilter>();
                })
                .AddApiVersioning(opt => {
                    opt.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                    opt.AssumeDefaultVersionWhenUnspecified = true;
                    opt.ReportApiVersions = true;
                    opt.ApiVersionReader = ApiVersionReader.Combine(
                        new UrlSegmentApiVersionReader(),
                        new HeaderApiVersionReader("x-api-version"),
                        new MediaTypeApiVersionReader("x-api-version")
                    );
                })
                .AddVersionedApiExplorer(setup => {
                    setup.GroupNameFormat = "'v'VVV";
                    setup.SubstituteApiVersionInUrl = true;
                });
            ;

            return services;
        }

        public static void UseDocApi(this IApplicationBuilder app)
        {

            //if (app.Environment.IsDevelopment()) {
            var apiVersionDescriptionProvider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

            app.UseSwagger();
            app.UseSwaggerUI(opt => {
                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions.Reverse())
                {
                    var isDeprecated = description.IsDeprecated ? " (DEPRECATED)" : string.Empty;

                    opt.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"{description.GroupName.ToUpperInvariant()}{isDeprecated}"
                    );
                }
            });

            //}
        }

        public static void ConfigurarApi(this WebApplication app)
        {
            app.UseHttpsRedirection();

            //app.UseCors(x => x
            //    .SetIsOriginAllowed(origin => true)
            //    .AllowCredentials()
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //);

            app.UseAuthorization();

            //app.MapOpenApi();
            app.MapControllers();

            app.UseDocApi();

        }

        public static void UseScalarDocApi(this IEndpointRouteBuilder app)
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }
    }
}
