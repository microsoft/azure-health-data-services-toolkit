using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Fhir.Proxy.Tests.Assets
{
    public class HttpEchoListener
    {

        private HttpListener listener;

        public async Task StartAsync(int port)
        {
            listener = new HttpListener();
            listener.Realm = "localhost";
            listener.Prefixes.Add($"http://localhost:{port}/");

            try
            {
                listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            while (true)
            {
                HttpListenerContext context = await listener.GetContextAsync();
                //var cert = await context.Request.GetClientCertificateAsync();
                //if (!context.Request.Headers.AllKeys.ToArray().Contains("Authorization") &&
                //    cert == null)
                //{
                //    context.Response.StatusCode = 503;
                //    context.Response.Close();
                //    return;
                //}

                await ProcessMessage(context);
            }
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
            //echo the body
            byte[] buffer = new byte[100];
            int bytesRead = await context.Request.InputStream.ReadAsync(buffer.AsMemory(0, 100));
            byte[] message = new byte[bytesRead];
            Buffer.BlockCopy(buffer, 0, message, 0, bytesRead);
            await context.Response.OutputStream.WriteAsync(message.AsMemory(0, message.Length));
            context.Response.StatusCode = 200;
            context.Response.Close();
        }

        private static async Task ProcessGet(HttpListenerContext context)
        {
            //echo the query string key:value as a json body property
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


        public async Task StopAsync()
        {
            listener.Stop();
            await Task.CompletedTask;
        }
    }
}
