using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskHub.Api.Models.Dtos.Lists;

namespace TaskHub.Api.Core.Interfaces
{
    public interface IListService
    {
        Task<IEnumerable<ListDto>> GetListsByBoardIdAsync(Guid boardId);
        Task<ListDto?> GetListByIdAsync(Guid id);
        Task<ListDto> CreateListAsync(Guid boardId, CreateListDto createListDto);
        Task UpdateListAsync(Guid id, UpdateListDto updateListDto);
        Task DeleteListAsync(Guid id);
    }
}
