using Azure.Storage.Blobs;
using AzureBlob.Api.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;

namespace AzureBlob.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "AzureBlobWithAutoTagging.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "AzureBlobWithAutoTagging.Api", Version = "v1" });
            });
            services.AddScoped(_ =>
            {
                return new BlobServiceClient(Configuration["AzureBlobStorage:ConnectionString"]);
            });
            services.AddScoped(_ =>
            {
                var visionClient = new ComputerVisionClient(
                    new ApiKeyServiceClientCredentials(Configuration["CognitiveService:Key"]),
                    new System.Net.Http.DelegatingHandler[] { });
                visionClient.Endpoint = Configuration["CognitiveService:Endpoint"];
                return visionClient;
            });

            services.AddScoped<IFileService, FileService>();
        }
    }
}