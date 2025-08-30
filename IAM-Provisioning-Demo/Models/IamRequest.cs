namespace IAM_Provisioning_Demo.Models
{
    public record IamRequest(
        string Id,
        RequestType Type,
        string RequestedBy,     // user UPN or sAMAccountName
        string? TargetUser,     // for user/service accounts or membership
        string? TargetService,  // for service account requests
        string? GroupName,      // for group creation or membership
        Dictionary<string, string>? Attributes // password, OU, display, etc.
    );
}
