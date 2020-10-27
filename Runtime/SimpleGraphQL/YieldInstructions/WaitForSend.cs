using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace SimpleGraphQL.YieldInstructions
{
    [PublicAPI]
    public class WaitForSend : CustomYieldInstruction
    {
        private bool _taskDone;

        public override bool keepWaiting => !_taskDone;

        /// <summary>
        /// Create a new WaitForSend Yield Instruction.
        /// </summary>
        /// <param name="sendFunc">The graphQL send function.</param>
        /// <param name="onComplete">The callback that will be invoked after the task is complete.</param>
        public WaitForSend(Func<Task<string>> sendFunc, Action<string> onComplete)
        {
            Task.Run(() => RunSendAsync(sendFunc, onComplete));
        }

        private async Task<string> RunSendAsync(Func<Task<string>> func, Action<string> onComplete)
        {
            string result = await func.Invoke();
            _taskDone = true;

            onComplete.Invoke(result);

            return result;
        }
    }
}