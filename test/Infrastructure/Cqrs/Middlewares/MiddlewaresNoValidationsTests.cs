﻿using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Application.Cqrs.Commands;
using SharedKernel.Application.Cqrs.Middlewares;
using SharedKernel.Infrastructure;
using SharedKernel.Infrastructure.Cqrs.Commands;
using SharedKernel.Infrastructure.Cqrs.Middlewares;
using SharedKernel.Integration.Tests.Cqrs.Commands;
using SharedKernel.Integration.Tests.Shared;
using Xunit;

namespace SharedKernel.Integration.Tests.Cqrs.Middlewares
{
    public class MiddlewaresNoValidationsTests : InfrastructureTestCase
    {
        protected override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            return services
                .AddSharedKernel()
                .AddCommandsHandlers(typeof(SampleCommandHandler))
                .AddScoped(typeof(IMiddleware<,>), typeof(TimerMiddleware<,>))
                .AddTransient(typeof(IMiddleware<,>), typeof(ValidationMiddleware<,>))
                .AddInMemoryCommandBus();
        }

        [Fact]
        public async Task TestCommandHandlerNoValidationNoResponse()
        {
            var request = new SampleCommand(0);

            var result = await Record.ExceptionAsync(() => GetRequiredService<ICommandBus>().Dispatch(request, CancellationToken.None));

            result.Should().BeNull();
        }
    }
}