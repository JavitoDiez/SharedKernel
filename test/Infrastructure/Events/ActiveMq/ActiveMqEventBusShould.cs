using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure.ActiveMq.Events;

namespace SharedKernel.Integration.Tests.Events.ActiveMq;


public class ActiveMqEventBusShould : EventBusCommonTestCase
{
    protected override string GetJsonFile()
    {
        return "Events/ActiveMq/appsettings.ActiveMq.json";
    }

    protected override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        return base.ConfigureServices(services).AddSharedKernelActiveMqEventBus(Configuration);
    }

    [Fact]
    public async Task PublishDomainEventFromApacheMq()
    {
        await PublishDomainEvent();
    }
}
