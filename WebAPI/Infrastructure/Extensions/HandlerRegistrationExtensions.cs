using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace WebAPI.Infrastructure.Extensions
{
    public static class HandlerRegistrationExtensions
    {
        public static void AddFeatureHandlers(this IServiceCollection services, Assembly assembly)
        {
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => t.Name == "Handler");

            foreach (var handlerType in handlerTypes)
            {
                services.AddScoped(handlerType);
            }
        }
    }
}
