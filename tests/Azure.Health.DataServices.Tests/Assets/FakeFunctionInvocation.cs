using System;
using Microsoft.Azure.Functions.Worker;

namespace Azure.Health.DataServices.Tests.Assets
{
    public class FakeFunctionInvocation : FunctionInvocation
    {
        public FakeFunctionInvocation(string id = null, string functionId = null)
        {
            if (id is not null)
            {
                Id = id;
            }

            if (functionId is not null)
            {
                FunctionId = functionId;
            }
        }

        public override string Id { get; } = Guid.NewGuid().ToString();

        public override string FunctionId { get; } = Guid.NewGuid().ToString();

        public override TraceContext TraceContext { get; } = null;
    }
}
