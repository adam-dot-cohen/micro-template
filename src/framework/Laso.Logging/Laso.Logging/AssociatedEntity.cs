namespace Laso.Logging
{
    public class AssociatedEntity
    {
        public string Entity { get; set; }
        public string Id { get; set; }

        protected bool Equals(AssociatedEntity other)
        {
            return string.Equals(Entity, other.Entity) && string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AssociatedEntity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Entity != null ? Entity.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}