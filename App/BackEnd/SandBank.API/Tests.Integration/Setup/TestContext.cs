using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Core.Jwt;
using Docker.DotNet;
using Docker.DotNet.Models;
using Api;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TechDebtTags;
using Xunit;

namespace Tests.Integration.Setup
{
    public class TestContext : IAsyncLifetime
    {
        public IConfiguration Configuration { get; private set; }
        private TestServer _testServer;
        public HttpClient Client { get; set; }
        private ServiceCollection ServiceCollection { get; set; }
        public ServiceProvider ServiceProvider { get; private set; }
        private readonly DockerClient _dockerClient;
        private const string DynamoDBImage = "amazon/dynamodb-local";
        private string _containerId;

        public TestContext()
        {
            SetupConfiguration();
            SetupServices();
            SetupClient();
            _dockerClient = new DockerClientConfiguration(new Uri("unix:///var/run/docker.sock")).CreateClient();
        }
        
        public async Task InitializeAsync()
        {
            await PullImage();
            await StartContainer();
            await TestDataSetup.CreateTable();
        }

        public async Task DisposeAsync()
        {
            if (_containerId != null)
            {
                await _dockerClient.Containers.KillContainerAsync(_containerId, new ContainerKillParameters());
            }
        }

        private async Task PullImage()
        {
            await _dockerClient.Images.CreateImageAsync(new ImagesCreateParameters()
            {
                FromImage = DynamoDBImage,
                Tag = "latest",
            }, new AuthConfig(), new Progress<JSONMessage>());
        }

        private async Task StartContainer()
        {
            var response = await _dockerClient.Containers.CreateContainerAsync(new CreateContainerParameters
            {
                Image = DynamoDBImage,
                ExposedPorts = new Dictionary<string, EmptyStruct>
                {
                    {"8000", default}  
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        {"8000", new List<PortBinding> {new PortBinding { HostPort = "8000" }}}  
                    },
                    PublishAllPorts = true,
                }
            });
            _containerId = response.ID;

            await _dockerClient.Containers.StartContainerAsync(_containerId, null);
        }

        private void SetupClient()
        {
            _testServer = new TestServer(new WebHostBuilder()
                .UseEnvironment("Test")
                .ConfigureAppConfiguration((context,conf) =>
                    {
                        conf.AddConfiguration(Configuration);
                    })
                .UseStartup<Startup>()
                .UseSerilog());
            
            Client = _testServer.CreateClient();
            var jwtToken = ServiceProvider.GetService<IJwtTokenService>().GenerateToken(1);
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        [TechnicalDebt("appsettings.Test.json has been copied into the Tests.Integration project and loaded from there.",
            "If the configurations diverge, they'll need to manually be kept in sync.",
            "It should load appsettings.Development.json from the main project.")]
        private void SetupConfiguration()
        {
            var projectDir = Directory.GetCurrentDirectory();
            var configPath = Path.Combine(projectDir, "appsettings.Test.json");
            
            Configuration = new ConfigurationBuilder()
                .AddJsonFile(configPath)
                .Build();
        }
        
        private void SetupServices()
        {
            ServiceCollection = new ServiceCollection();
            var jwtConfigSection = Configuration.GetSection(nameof(JwtTokenConfiguration));
            ServiceCollection.Configure<JwtTokenConfiguration>(jwtConfigSection);
            ServiceCollection.AddTransient<IJwtTokenService, JwtTokenService>();
            ServiceProvider = ServiceCollection.BuildServiceProvider();
        }
    }
}