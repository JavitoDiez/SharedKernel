﻿using SharedKernel.Application.Auth.UnitOfWork;
using SharedKernel.Application.Auth.Users.Services;
using SharedKernel.Application.Cqrs.Commands.Handlers;
using SharedKernel.Application.Events;
using SharedKernel.Domain.Events;

namespace SharedKernel.Application.Auth.Users.Commands;

internal sealed class AddUserClaimHandler : ICommandRequestHandler<AddUserClaim>
{
    private readonly IAuthUnitOfWork _unitOfWork;
    private readonly IEventBus _eventBus;
    private readonly IUserManager _userManager;

    public AddUserClaimHandler(
        IAuthUnitOfWork unitOfWork,
        IEventBus eventBus,
        IUserManager userManager)
    {
        _unitOfWork = unitOfWork;
        _eventBus = eventBus;
        _userManager = userManager;
    }

    public async Task Handle(AddUserClaim request, CancellationToken cancellationToken)
    {
        await _userManager.AddClaimAsync(request.Id, request.Claim, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        // ReSharper disable once UseCollectionExpression
        await _eventBus.Publish(Enumerable.Empty<DomainEvent>(), cancellationToken);
    }
}