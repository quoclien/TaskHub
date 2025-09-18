using System;
using System.Collections.Generic;
using TaskHub.Api.Models.Entities;
using TaskHub.Api.Models.Entities.Users;
using ListEntity = TaskHub.Api.Models.Entities.Lists.List;

namespace TaskHub.Api.Models.Entities.Boards
{
    public class Board : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsPublic { get; set; }
        public bool IsArchived { get; set; }
        public Guid OwnerId { get; set; }
        public virtual User Owner { get; set; } = null!;
        public virtual ICollection<ListEntity> Lists { get; set; } = new List<ListEntity>();
        public virtual ICollection<BoardMember> BoardMembers { get; set; } = new List<BoardMember>();
    }
}
