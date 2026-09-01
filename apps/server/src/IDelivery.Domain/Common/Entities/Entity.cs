namespace IDelivery.Domain.Common.Entities;

public abstract class Entity
{
    public Guid Id { get; private set; }

    protected Entity() => Id = Guid.NewGuid();

    protected Entity(Guid id) => Id = id == Guid.Empty ? throw new ArgumentException("O id da entidade não pode ser vazio", nameof(id)) : id;
    
    public override bool Equals(object? obj)
    {
        if (obj is not Entity other) return false;
        if (ReferenceEquals(this, other)) return true;
        if (Id == Guid.Empty || other.Id == Guid.Empty) return false;
        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        return a.Equals(b);
    }

    public static bool operator !=(Entity? a, Entity? b) => !(a == b);
}