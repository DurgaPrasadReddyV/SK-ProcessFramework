using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Temp.Steps.States;

namespace Temp.Steps
{
    public class ApprovalsGenerationStep : KernelProcessStep<ApprovalsGenerationState>
    {
        public ApprovalsGenerationState? _state;


    }
}
