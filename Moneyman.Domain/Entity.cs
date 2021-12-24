using System.Diagnostics.CodeAnalysis;

namespace Moneyman.Domain
{
    [ExcludeFromCodeCoverage]
    public class Entity : IEntity
    {
        public int Id { get; set; }
    }
}