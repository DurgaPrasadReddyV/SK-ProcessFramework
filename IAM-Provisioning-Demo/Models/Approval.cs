namespace IAM_Provisioning_Demo.Models
{
    public record Approval(
        string Approver,
        string Role,            // Manager, ResourceOwner, PlatformOwner, GroupOwner
        bool Approved,
        DateTimeOffset Timestamp
    );
}
