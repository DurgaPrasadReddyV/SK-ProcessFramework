using Microsoft.SemanticKernel;
using StarodubOleg.GPPG.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temp.Steps.Events;
using Temp.Steps.Functions;
using Temp.Steps.States;

namespace Temp.Steps
{
    internal class ProvisioningStep : KernelProcessStep<ProvisioningState>
    {
        public ProvisioningState? _state;

        [KernelFunction(ProvisioningFunctions.ProvisionEngineerReview)]
        public async Task ProvisionEngineerReviewAsync(KernelProcessStepContext context, Guid requestId)
        {
            if (_state is null) throw new InvalidOperationException("State is null.");
            _state.RequestId = requestId;

            Console.WriteLine($"[SIMULATION] Sending to provisioning team for review.");
            await Program.ProcessProvisionEngineerReviewAsync(requestId);
        }

        [KernelFunction(ProvisioningFunctions.ProcessProvisionEngineerReview)]
        public async Task ProcessProvisionEngineerReviewAsync(KernelProcessStepContext context, bool provisionEngineerReview)
        {
            if (_state is null) throw new InvalidOperationException("State is null.");

            if (provisionEngineerReview)
            {
                await Task.Delay(2000);
                Console.WriteLine($"Provisioning Team Approved");
                await context.EmitEventAsync(new() { Id = ProvisioningEvents.ProvisionEngineerReviewApproved, Data = _state.RequestId });
            }
            else
            {
                Console.WriteLine($"Provisioning Team Rejected");
                await context.EmitEventAsync(new() { Id = ProvisioningEvents.ProcessCompleted, Data = false });
            }
        }

        [KernelFunction(ProvisioningFunctions.ProvisionResource)]
        public async Task ProvisionResourceAsync(KernelProcessStepContext context, Guid requestId)
        {
            await Task.Delay(2000);
            Console.WriteLine($"[SIMULATION] Provisioning of Resource {requestId} Successful");
            await context.EmitEventAsync(new() { Id = ProvisioningEvents.ProcessCompleted, Data = true });
        }

        public override ValueTask ActivateAsync(KernelProcessStepState<ProvisioningState> state)
        {
            _state = state.State;
            return ValueTask.CompletedTask;
        }
    }
}
