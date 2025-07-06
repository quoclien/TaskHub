using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ListEntity = TaskHub.Api.Models.Entities.Lists.List;

namespace TaskHub.Api.Core.Interfaces.Repositories
{
    public interface IListRepository : IRepository<ListEntity>
    {
        Task<ListEntity> GetByIdOrThrowAsync(Guid id);
        Task<IEnumerable<ListEntity>> GetListsByBoardIdAsync(Guid boardId);
        Task<bool> BoardExistsAsync(Guid boardId);
        Task<int> GetMaxPositionAsync(Guid boardId);
        Task ReorderListsAsync(Guid boardId, int fromPosition, int toPosition);
                Task ReorderListsAfterDeletion(Guid boardId, int deletedPosition);
        Task ReorderListsAfterInsertion(Guid boardId, int insertedPosition);
    }
}
