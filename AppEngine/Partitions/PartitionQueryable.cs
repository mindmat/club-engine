using AppEngine.DataAccess;

using Microsoft.EntityFrameworkCore;

namespace AppEngine.Partitions
{
    public class PartitionQueryable<TPartition>(DbContext dbContext) : Queryable<TPartition>(dbContext) , IQueryable<IPartition>
        where TPartition : Entity, IPartition
    {
        public IEnumerator<IPartition> GetEnumerator()
        {
            return base.GetEnumerator();
        }
    }
}