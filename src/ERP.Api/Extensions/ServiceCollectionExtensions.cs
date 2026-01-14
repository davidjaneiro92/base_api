namespace ERP.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddModules(
           this IServiceCollection services, IConfiguration config
       )
        {
            //services
                //.AddModularControllers(config)
                //.AddAlmoxarifadoApi(config)
                //.AddRecebimentoDeCompraApi(config)
                //.AddEstoqueModule(config)
                ;

            return services;
        }
    }
}
