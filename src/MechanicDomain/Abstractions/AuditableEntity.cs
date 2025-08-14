namespace MechanicDomain.Abstractions
{

    public abstract class AuditableEntity : Entity
    {
        protected AuditableEntity()
        { }

        protected AuditableEntity(Guid id = default(Guid))
            : base(id)
        {
        }

        public DateTimeOffset CreatedAtUtc { get; set; }

        public string? CreatedBy { get; set; }

        public DateTimeOffset LastModifiedUtc { get; set; }

        public string? LastModifiedBy { get; set; }
    }
}
