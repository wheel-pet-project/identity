using Domain.Common;

namespace Domain.Role;

public class Role(int id, string name) : Enumeration(id, name)
{
    public static Role Customer = new(1, "customer");
    
    public static Role Admininstrator = new(2, "admin");
    
    public static Role Support = new(3, "support");
    
    public static Role Maintenance = new(4, "maintenance");
    
    public static Role HR = new(5, "hr");
}