using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace WebAPI.Infrastructure.Extensions
{
    public static class EndpointMappingExtensions
    {
        public static void MapFeatureEndpoints(this IEndpointRouteBuilder app, Assembly assembly)
        {
            var endpointTypes = assembly.GetTypes()
                .Where(t => t.IsClass && t.IsAbstract && t.IsSealed) // "static class" pattern
                .Where(t => t.Name == "Endpoint")
                .Where(t => t.GetMethod("Map", BindingFlags.Public | BindingFlags.Static) is not null);

            foreach (var endpointType in endpointTypes)
            {
                var mapMethod = endpointType.GetMethod("Map", BindingFlags.Public | BindingFlags.Static)!;
                mapMethod.Invoke(null, new object[] { app });
            }
        }
    }
}
