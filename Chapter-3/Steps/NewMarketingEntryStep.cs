// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Chapter_3.Models;

namespace Chapter_3.Steps;

/// <summary>
/// Mock step that emulates the creation a new marketing user entry.
/// </summary>
public class NewMarketingEntryStep : KernelProcessStep
{
    public static class ProcessStepFunctions
    {
        public const string CreateNewMarketingEntry = nameof(CreateNewMarketingEntry);
    }

    [KernelFunction(ProcessStepFunctions.CreateNewMarketingEntry)]
    public async Task CreateNewMarketingEntryAsync(KernelProcessStepContext context, MarketingNewEntryDetails userDetails, Kernel _kernel)
    {
        Console.WriteLine($"[MARKETING ENTRY CREATION] New Account {userDetails.AccountId} created");

        // Placeholder for a call to API to create new entry of user for marketing purposes
        await context.EmitEventAsync(new() { Id = AccountOpeningEvents.NewMarketingEntryCreated, Data = true });
    }
}
