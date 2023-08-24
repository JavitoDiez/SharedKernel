﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedKernel.Application.Cqrs.Commands;
using SharedKernel.Application.Cqrs.Commands.Handlers;
using SharedKernel.Application.System;
using SharedKernel.Application.System.Threading;
using SharedKernel.Infrastructure.Requests.Middlewares;
using System.Collections;
using System.Collections.Concurrent;

namespace SharedKernel.Infrastructure.Cqrs.Commands.InMemory
{
    /// <summary>
    /// 
    /// </summary>
    public class InMemoryCommandBus : ICommandBus
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IExecuteMiddlewaresService _executeMiddlewaresService;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IParallel _parallel;

        private static readonly ConcurrentDictionary<Type, object> CommandHandlers = new();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="serviceScopeFactory"></param>
        /// <param name="taskQueue"></param>
        /// <param name="executeMiddlewaresService"></param>
        /// <param name="applicationLifetime"></param>
        /// <param name="parallel"></param>
        public InMemoryCommandBus(
            IServiceProvider serviceProvider,
            IServiceScopeFactory serviceScopeFactory,
            IBackgroundTaskQueue taskQueue,
            IExecuteMiddlewaresService executeMiddlewaresService,
            IHostApplicationLifetime applicationLifetime,
            IParallel parallel)
        {
            _serviceProvider = serviceProvider;
            _serviceScopeFactory = serviceScopeFactory;
            _taskQueue = taskQueue;
            _executeMiddlewaresService = executeMiddlewaresService;
            _applicationLifetime = applicationLifetime;
            _parallel = parallel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="command"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task<TResponse> Dispatch<TResponse>(ICommandRequest<TResponse> command, CancellationToken cancellationToken)
        {
            return _executeMiddlewaresService.ExecuteAsync(command, cancellationToken, (req, c) =>
            {
                var handler = GetWrappedHandlers(req);

                if (handler == null)
                    throw new CommandNotRegisteredException(req.ToString());

                return handler.Handle(req, _serviceProvider, c);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task Dispatch(ICommandRequest command, CancellationToken cancellationToken)
        {
            return _executeMiddlewaresService.ExecuteAsync(command, cancellationToken, (req, c) =>
            {
                var handler = GetWrappedHandlers(req);

                if (handler == null)
                    throw new CommandNotRegisteredException(req.ToString());

                return handler.Handle(req, _serviceProvider, c);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Dispatch(IEnumerable<ICommandRequest> commands, CancellationToken cancellationToken)
        {
            return _parallel.ForEachAsync(commands, cancellationToken, Dispatch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<TResponse[]> Dispatch<TResponse>(IEnumerable<ICommandRequest<TResponse>> commands, CancellationToken cancellationToken)
        {
            return Task.WhenAll(commands.Select(command => Dispatch(command, cancellationToken)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="commands"></param>
        /// <param name="queueName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task DispatchOnQueue(IEnumerable<ICommandRequest> commands, string queueName, CancellationToken cancellationToken)
        {
            return _parallel.ForEachAsync(commands, cancellationToken, (command, ct) => DispatchOnQueue(command, queueName, ct));
        }

        /// <summary>
        /// Dispatch a command request on a queue.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="queueName">Queue name</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task DispatchOnQueue(ICommandRequest command, string queueName, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mutexManager = scope.ServiceProvider.GetService<IMutexManager>();

            if (mutexManager == default)
                throw new InvalidOperationException("IMutexManager not registered");

            return mutexManager.RunOneAtATimeFromGivenKeyAsync(queueName, async () =>
            {
                await Dispatch(command, cancellationToken);
                return TaskHelper.CompletedTask;
            }, cancellationToken);
        }

        /// <summary>
        /// Dispatch a command request on a queue.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="queueName">Queue name</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns></returns>
        public Task<TResponse> DispatchOnQueue<TResponse>(ICommandRequest<TResponse> command, string queueName, CancellationToken cancellationToken)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var mutexManager = scope.ServiceProvider.GetService<IMutexManager>();

            if (mutexManager == default)
                throw new InvalidOperationException("IMutexManager not registered");

            return mutexManager.RunOneAtATimeFromGivenKeyAsync(queueName, () => Dispatch(command, cancellationToken),
                cancellationToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Task QueueInBackground(ICommandRequest command)
        {
            _taskQueue.QueueBackground(async _ =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var commandBus = scope.ServiceProvider.GetRequiredService<ICommandBus>();
                await commandBus.Dispatch(command, _applicationLifetime.ApplicationStopping);
            });

            return Task.CompletedTask;
        }

        private CommandHandlerWrapper GetWrappedHandlers(ICommandRequest command)
        {
            var handlerType = typeof(ICommandRequestHandler<>).MakeGenericType(command.GetType());
            var wrapperType = typeof(CommandHandlerWrapper<>).MakeGenericType(command.GetType());

            var handlers =
                (IEnumerable)_serviceProvider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(handlerType));

            var wrappedHandlers = (CommandHandlerWrapper)CommandHandlers.GetOrAdd(command.GetType(), handlers.Cast<object>()
                .Select(_ => (CommandHandlerWrapper)Activator.CreateInstance(wrapperType)).FirstOrDefault());

            return wrappedHandlers;
        }

        private CommandHandlerWrapperResponse<TResponse> GetWrappedHandlers<TResponse>(ICommandRequest<TResponse> command)
        {
            var handlerType = typeof(ICommandRequestHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));
            var wrapperType = typeof(CommandHandlerWrapperResponse<,>).MakeGenericType(command.GetType(), typeof(TResponse));

            var handlers =
                (IEnumerable)_serviceProvider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(handlerType));

            var wrappedHandlers = (CommandHandlerWrapperResponse<TResponse>)CommandHandlers.GetOrAdd(command.GetType(), handlers.Cast<object>()
                .Select(_ => (CommandHandlerWrapperResponse<TResponse>)Activator.CreateInstance(wrapperType)).FirstOrDefault());

            return wrappedHandlers;
        }
    }
}
