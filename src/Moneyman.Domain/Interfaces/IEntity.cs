using System;

namespace Moneyman.Domain.Interfaces
{
    public interface IEntity
    {
        public int Id {get; set;}
        public DateTime CreatedDate => DateTime.Now;
    }
}