﻿using AppEngine.DomainEvents;
using AppEngine.ServiceBus;

using MediatR;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.DataAccess;

public class CommitUnitOfWorkDecorator<TRequest, TResponse>(DbContext dbContext,
                                                            CommandQueue commandQueue,
                                                            EventBus eventBus)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request,
                                        RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        try
        {
            var response = await next(cancellationToken);
            dbContext.ChangeTracker.DetectChanges();

            if (dbContext.ChangeTracker.HasChanges())
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            // "transaction": only release messages to event bus if db commit succeeds
            await commandQueue.Release(true);
            eventBus.Release(true);

            return response;
        }
        catch
        {
            await commandQueue.Release(false);
            eventBus.Release(false);

            throw;
        }
    }
}