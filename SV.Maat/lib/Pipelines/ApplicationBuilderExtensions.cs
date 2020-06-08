using System;
using Microsoft.AspNetCore.Builder;

namespace SV.Maat.lib.Pipelines
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UsePipelines(this IApplicationBuilder builder, Action<PipelineBuilder> configure)
        {

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            PipelineBuilder pipelineBuilder = builder.ApplicationServices.GetService(typeof(PipelineBuilder)) as PipelineBuilder;

            if (pipelineBuilder == null)
            {
                throw new ArgumentNullException(nameof(pipelineBuilder));
            }

            configure(pipelineBuilder);

            return builder;
        }
    }

}
