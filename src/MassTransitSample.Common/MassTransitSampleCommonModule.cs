using Volo.Abp.Modularity;
using Volo.Abp.Threading;
using Volo.Abp.Timing;

namespace MassTransitSample.Common
{
    [DependsOn(
        typeof(AbpThreadingModule),
        typeof(AbpTimingModule)
        )]
    public class MassTransitSampleCommonModule : AbpModule
    {
    }
}
