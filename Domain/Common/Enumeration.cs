namespace Domain.Common;

public abstract class Enumeration : IComparable
{
    public int Id { get; private set; }
    
    public string Name { get; private set; }
    
    
    protected Enumeration(int id, string name) => 
        (Id, Name) = (id, name);
    
    
    public override string ToString() => Name;
    
    public int CompareTo(object? obj)
    {
        return Name.CompareTo(((Enumeration)obj).Name);
    }
}