using System.Diagnostics.CodeAnalysis;
using Moneyman.Domain.Interfaces;

namespace Moneyman.Domain
{
    [ExcludeFromCodeCoverage]
    public class Entity : IEntity
    {
        public int Id { get; set; }
    }
}