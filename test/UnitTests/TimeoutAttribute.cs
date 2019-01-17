using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace UnitTests
{
    // https://github.com/abbotware/nunit-timeout/blob/master/Abbotware.Interop.NUnit/TimeoutAttribute.cs
    public class TimeoutAttribute : NUnitAttribute, IWrapTestMethod
    {
        private readonly TimeSpan timeout;

        public TimeoutAttribute(int milliseconds)
        {
            this.timeout = TimeSpan.FromMilliseconds(milliseconds);
        }

        public TestCommand Wrap(TestCommand command)
        {
            return new TimeoutCommand(command, this.timeout);
        }

        private class TimeoutCommand : DelegatingTestCommand
        {
            private readonly TimeSpan timeout;

            public TimeoutCommand(TestCommand innerCommand, TimeSpan timeout)
                : base(innerCommand)
            {
                this.timeout = timeout;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var t = Task.Run(() => this.innerCommand.Execute(context));

                try
                {
                    if (!Debugger.IsAttached)
                    {
                        t.TimeoutAfter(this.timeout).Wait();
                    }

                    return t.Result;
                }
                catch (AggregateException ae)
                {
                    throw ae.InnerException;
                }
            }
        }
    }

    internal static class TaskExtensions
    {
        /// <summary>
        /// Timeout a task after a timeout
        /// </summary>
        /// <remarks>https://blogs.msdn.microsoft.com/pfxteam/2011/11/10/crafting-a-task-timeoutafter-method/</remarks>
        /// <param name="task">wrapped task</param>
        /// <param name="timeout">timeout value</param>
        /// <returns>awaitable task</returns>
        public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)).ConfigureAwait(false))
            {
                await task.ConfigureAwait(false);
            }
            else
            {
                throw new TimeoutException();
            }
        }
    }
}
