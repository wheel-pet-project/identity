using Domain.Common;

namespace Domain.Status;

public class Status(int id, string name) : Enumeration(id, name)
{
    public static Status Unconfirmed = new Status(1, "Unconfirmed");
    
    public static Status Confirmed = new Status(2, "Confirmed");
    
    public static Status Active = new Status(3, "Active");
    
    public static Status DeActive = new Status(4, "DeActive");
    
    public static Status Deleted = new Status(5, "Deleted");
}