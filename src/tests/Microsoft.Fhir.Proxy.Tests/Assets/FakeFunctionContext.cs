using Microsoft.Azure.Functions.Worker;
using System;
using System.Collections.Generic;

namespace Microsoft.Fhir.Proxy.Tests.Assets
{

    public class FakeFunctionContext : FunctionContext, IDisposable
    {
        private readonly FunctionInvocation invocation;

        public FakeFunctionContext()
            : this(new FakeFunctionDefinition(), new FakeFunctionInvocation())
        {
        }

        public FakeFunctionContext(FunctionDefinition functionDefinition, FunctionInvocation invocation)
        {
            FunctionDefinition = functionDefinition;
            this.invocation = invocation;

            //Features.Set<IFunctionBindingsFeature>(new TestFunctionBindingsFeature
            //{
            //    OutputBindingsInfo = new DefaultOutputBindingsInfoProvider().GetBindingsInfo(FunctionDefinition)
            //});

            //BindingContext = new DefaultBindingContext(this);
        }

        public bool IsDisposed { get; private set; }

        public override IServiceProvider InstanceServices { get; set; }

        public override FunctionDefinition FunctionDefinition { get; }

        public override IDictionary<object, object> Items { get; set; }

        public override IInvocationFeatures Features { get; } = null;

        public override string InvocationId => invocation.Id;

        public override string FunctionId => invocation.FunctionId;

        public override TraceContext TraceContext => invocation.TraceContext;

        public override BindingContext BindingContext { get; }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
