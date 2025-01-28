using Quartz;
using Quartz.Spi;

namespace VivesBankApi.Rest.Movimientos.Jobs;

public class ScopedJobFactory : IJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    
    public ScopedJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {
        using var scope = _serviceProvider.CreateScope();
        return (IJob)scope.ServiceProvider.GetRequiredService(bundle.JobDetail.JobType);
    }

    public void ReturnJob(IJob job)
    {
    }
}