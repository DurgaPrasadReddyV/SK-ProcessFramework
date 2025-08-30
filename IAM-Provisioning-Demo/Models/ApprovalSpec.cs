namespace IAM_Provisioning_Demo.Models
{
    public record ApprovalSpec(
        string RequestId,
        List<(string Approver, string Role)> RequiredApprovers,
        bool AutoApproved);
}
