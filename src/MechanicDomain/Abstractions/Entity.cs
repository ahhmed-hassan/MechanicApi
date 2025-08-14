namespace MechanicDomain.Abstractions
{
    public abstract class Entity
    {
        public Guid Id { get; init; }

        private readonly List<DomainEvent> _domainEvents = new();

        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        protected Entity(Guid id = default(Guid)) => Id = id == Guid.Empty ? Guid.NewGuid() : id;
        
        public void AddDomainEvent(DomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void RemoveDomainEvent(DomainEvent even) => _domainEvents.Remove(even);

        public void ClearDomainEvents() => _domainEvents.Clear();   
    }
}
