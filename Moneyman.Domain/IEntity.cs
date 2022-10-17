using System;

namespace Moneyman.Domain
{
    public interface IEntity
    {
        public int Id {get; set;}
        public DateTime CreatedDate => DateTime.Now;
    }
}