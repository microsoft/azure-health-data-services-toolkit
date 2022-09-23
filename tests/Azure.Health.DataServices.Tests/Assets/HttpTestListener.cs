using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Azure.Health.DataServices.Tests.Assets
{
    public class HttpTestListener
    {

        private HttpListener listener;
        private static List<Tuple<string, string>> responseHeaders;

        public async Task StartAsync(int port, List<Tuple<string, string>> responseEchoHeaders = null)
        {
            listener = new HttpListener
            {
                Realm = "localhost"
            };

            responseHeaders = responseEchoHeaders;
            responseHeaders ??= new();

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
                await ProcessMessage(context);
                context.Response.OutputStream.Close();
                context.Response.Close();
            }
        }

        public async Task StopAsync()
        {
            listener.Stop();
            await Task.CompletedTask;
        }

        private static async Task ProcessMessage(HttpListenerContext context)
        {
            switch (context.Request.HttpMethod.ToLowerInvariant())
            {
                case "get":
                    await ProcessGet(context);
                    break;
                default:
                    throw new Exception("Http method unavailable.");
            }
        }


        private static async Task ProcessGet(HttpListenerContext context)
        {
            try
            {
                await WriteResponseHeadersAsync(context);
                context.Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            await Task.CompletedTask;
        }


        private static async Task WriteResponseHeadersAsync(HttpListenerContext context)
        {
            foreach (var item in responseHeaders)
            {
                string value = context.Request.Headers[item.Item1];
                string content = $"{{ \"{item.Item2}\": \"{value}\" }}";
                byte[] buffer = Encoding.UTF8.GetBytes(content);
                context.Response.ContentLength64 = buffer.Length;
                context.Response.ContentType = "application/json";
                await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            }
        }
    }
}
