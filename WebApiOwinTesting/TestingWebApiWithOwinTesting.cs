﻿namespace WebApiOwinTesting
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;
    using FluentAssertions;
    using Microsoft.Owin.Infrastructure;
    using Newtonsoft.Json;
    using Owin;
    using Owin.Testing;
    using Xunit;

    public class TestingWebApiWithOwinTesting
    {
        [Fact]
        public async Task Can_invoke_message_controller()
        {
            // OwinTestServer is pure-in memory and has no environmental dependencies
            OwinTestServer owinTestServer = OwinTestServer.Create(app => new Startup().Configuration(app));
            HttpClient client = owinTestServer.CreateHttpClient();
            HttpResponseMessage response = await client.GetAsync("http://localhost/api/message");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            string content = await response.Content.ReadAsStringAsync();
            var greeting = JsonConvert.DeserializeObject<Greeting>(content);

            greeting.Text.Should().Be("Hello, World!");
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // supports Microsoft.Owin.OwinMiddleWare. I'd prefer not to have this here but not sure I want
            // to depend on Microsoft.Owin either.
            SignatureConversions.AddConversions(app); 
            var config = new MyHttpConfiguration();
            app.UseWebApi(config);
        }
    }

    public class MyHttpConfiguration : HttpConfiguration
    {
        public MyHttpConfiguration()
        {
            ConfigureRoutes();
        }

        private void ConfigureRoutes()
        {
            Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }

    public class Greeting
    {
        public string Text { get; set; }
    }

    public class MessageController : ApiController
    {
        public Greeting Get()
        {
            return new Greeting
            {
                Text = "Hello, World!"
            };
        }
    }
}