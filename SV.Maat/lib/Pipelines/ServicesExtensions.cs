using Microsoft.Extensions.DependencyInjection;

namespace SV.Maat.lib.Pipelines
{
    public static class ServicesExtensions
    {
        public static void AddPipelines(this IServiceCollection services)
        {
            Pipeline pipeline = new Pipeline();
            services.AddSingleton(typeof(Pipeline), pipeline);
            services.AddTransient(typeof(PipelineBuilder));
        }
    }

}
