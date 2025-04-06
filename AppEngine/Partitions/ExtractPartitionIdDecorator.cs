using AppEngine.Authorization;

using MediatR;

namespace AppEngine.Partitions;

public class ExtractPartitionIdDecorator<TRequest, TResponse>(PartitionContext partitionContext) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request,
                                        RequestHandlerDelegate<TResponse> next,
                                        CancellationToken cancellationToken)
    {
        if (request is IPartitionBoundRequest partitionBoundRequest)
        {
            partitionContext.PartitionId = partitionBoundRequest.PartitionId;
        }

        return await next();
    }
}