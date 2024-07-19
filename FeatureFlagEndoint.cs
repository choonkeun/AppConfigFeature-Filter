
using Microsoft.FeatureManagement;

namespace AppConfigFeature_Filter
{
    public class FeatureFlagEndoint : IEndpointFilter
    {
        public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
        {
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint is null) return await next(context);

            var endpointName = endpoint.Metadata.GetMetadata<EndpointNameMetadata>()?.EndpointName;
            if (endpointName is null) return await next(context);

            var featureManager = context.HttpContext.RequestServices
                                        .GetRequiredService<IFeatureManager>();

            //{endpointName}: match to KeyName
            //Endpoint_{endpointName}: match to KeyName
            //var enabled = await featureManager.IsEnabledAsync($"{endpointName}");
            //var enabled = await featureManager.IsEnabledAsync($"Endpoint_{endpointName}");
            var enabled = await featureManager.IsEnabledAsync(FeatureFlags.Debug);
            if (!enabled) return Results.NotFound();    //this will return 404 error response 

            return await next(context);
        }
    }
}
