// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using Microsoft.AzureHealth.DataServices.Filters;
using Microsoft.AzureHealth.DataServices.Pipelines;
using Newtonsoft.Json;

namespace Quickstart.Extensions
{
    /// <summary>
    /// Extensions to help with filter errors.
    /// </summary>
    public static class FilterExtensions
    {
        /// <summary>
        /// Sets the context body to the error message.
        /// </summary>
        /// <param name="context">Pipeline context object.</param>
        /// <param name="args">Filter error arguements.</param>
        /// <param name="debug">Show error to called.</param>
        /// <returns>Modified context object.</returns>
        public static OperationContext SetContextErrorBody(this OperationContext context, FilterErrorEventArgs args, bool debug = false)
        {
            if (!debug)
            {
                args = new FilterErrorEventArgs(args.Name, args.Id, args.IsFatal, code: args.Code);
            }

            // System.Text.Json does not support int pointers (like HTTP status codes).
            context.ContentString = JsonConvert.SerializeObject(args);
            return context;
        }
    }
}
