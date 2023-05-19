using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AzureHealth.DataServices.Tests.Assets
{
    public class HttpEchoListener
    {
        private static int? s_numRetry;
        private static int s_max;
        private static int s_attempt;
        private HttpListener _listener;

        public async Task StartAsync(int port, int? num409 = null, int? maxAttempts = null)
        {
            _listener = new HttpListener
            {
                Realm = "localhost",
            };

            s_numRetry = num409;
            s_max = maxAttempts ?? 0;

            _listener.Prefixes.Add($"http://localhost:{port}/");

            try
            {
                _listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            while (true)
            {
                HttpListenerContext context = await _listener.GetContextAsync();
                await ProcessMessage(context);
            }
        }

        public async Task StopAsync()
        {
            s_numRetry = null;
            s_max = 0;
            _listener.Stop();
            await Task.CompletedTask;
        }

        private static async Task ProcessMessage(HttpListenerContext context)
        {
            switch (context.Request.HttpMethod.ToLowerInvariant())
            {
                case "post":
                    await ProcessPost(context);
                    break;
                case "get":
                    await ProcessGet(context);
                    break;
                case "put":
                    await ProcessPut(context);
                    break;
                case "delete":
                    await ProcessDelete(context);
                    break;
                case "patch":
                    await ProcessPatch(context);
                    break;
                default:
                    throw new Exception("Http method unavailable.");
            }
        }

        private static async Task ProcessPost(HttpListenerContext context)
        {
            if (s_numRetry.HasValue)
            {
                if (s_attempt < s_numRetry.Value && s_numRetry.Value < s_max)
                {
                    s_attempt++;
                    context.Response.StatusCode = 409;
                    context.Response.Close();
                    return;
                }
            }

            // echo the body
            byte[] buffer = new byte[100];
            int bytesRead = await context.Request.InputStream.ReadAsync(buffer.AsMemory(0, 100));
            byte[] message = new byte[bytesRead];
            Buffer.BlockCopy(buffer, 0, message, 0, bytesRead);
            await context.Response.OutputStream.WriteAsync(message.AsMemory(0, message.Length));
            foreach (var requestHeaderKey in context.Request.Headers.AllKeys)
            {
                context.Response.Headers.Add(requestHeaderKey, context.Request.Headers[requestHeaderKey]);
            }

            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        private static async Task ProcessGet(HttpListenerContext context)
        {
            // echo the query string key:value as a json body property
            var keys = context.Request.QueryString.AllKeys;
            StringBuilder builder = new();
            builder.Append("{ ");
            for (int i = 0; i < keys.Length; i++)
            {
                string value = context.Request.QueryString.GetValues(i).First();
                if (keys.Length == 1 || keys.Length - 1 == i)
                {
                    builder.Append($"\"{keys[i]}\": \"{value}\"");
                }
                else
                {
                    builder.Append($"\"{keys[i]}\": \"{value}\",");
                }
            }

            builder.Append(" }");
            string body = builder.ToString();
            byte[] buffer = Encoding.UTF8.GetBytes(body);
            foreach (var requestHeaderKey in context.Request.Headers.AllKeys)
            {
                context.Response.AppendHeader(requestHeaderKey, context.Request.Headers[requestHeaderKey]);
            }

            await context.Response.OutputStream.WriteAsync(buffer.AsMemory(0, buffer.Length));
            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        private static async Task ProcessPut(HttpListenerContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.Close();
            await Task.CompletedTask;
        }

        private static async Task ProcessDelete(HttpListenerContext context)
        {
            context.Response.StatusCode = 204;
            context.Response.Close();
            await Task.CompletedTask;
        }

        private static async Task ProcessPatch(HttpListenerContext context)
        {
            context.Response.StatusCode = 204;
            context.Response.Close();
            await Task.CompletedTask;
        }
    }
}
