using ExdTech.ImageProcessing.Standard;
using ExdTech.ImageServer.Common;
using ExdTech.ImageServer.ImageInfoPersistence;
using ExdTech.ImageServer.Persistence.AzBlobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.AzureAppServices;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;
using System;

namespace ExdTech.ImageServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            IdentityModelEventSource.ShowPII = true;
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.Configure<AzureFileLoggerOptions>(Configuration.GetSection("AzureLogging"));

            // Adds Microsoft Identity platform (AAD v2.0) support to protect this Api
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(options =>
                    {
                        Configuration.Bind("AzureAdB2C", options);

                        options.TokenValidationParameters.NameClaimType = "name";
                    },
            options => { Configuration.Bind("AzureAdB2C", options); });

            services.AddControllers();
            services.AddAuthorization(options =>
            {
                // Create policy to check for the scope 'read'
                options.AddPolicy("access",
                    policy => policy.Requirements.Add(new ScopesRequirement("access")));
            });

            var imageProcessingOptions = new ImageProcessingOptions
            {
                CompressionQualityPercentage = int.Parse(Configuration["ImageProcessingConfig:CompressionQualityPercentage"]),
                MaxFileSizeAcceptedInBytes = uint.Parse(Configuration["ImageProcessingConfig:MaxFileSizeAcceptedInBytes"]),
                MaxHeightInPixels = uint.Parse(Configuration["ImageProcessingConfig:MaxHeightInPixels"]),
                MaxWidthInPixels = uint.Parse(Configuration["ImageProcessingConfig:MaxWidthInPixels"]),
            };

            // This was a more elegant way of doing it, but environment variables were not being applied when I did it this way:
            // Configuration.Bind (ImageProcessingOptions.ImageProcessingConfig, imageProcessingOptions);

            services.AddScoped<IImageStorageService>(c => new BlobAccess(Configuration["ImageStoreConnectionString"], Configuration["ContainerClient"]));
            services.AddScoped<IInfoStorageService>(c => new InfoStorageService());
            services.AddScoped<IImageProcessingService> (c => new ImageProcessingService (imageProcessingOptions));


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "ExdTech.ImageServer",
                    Description = "Image server for Exd Pic.",
                    Contact = new OpenApiContact
                    {
                        Name = "ExdPic.com",
                        Email = string.Empty,
                        Url = new Uri("http://exdpic.com"),
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExdTech.ImageServer V1");
                c.RoutePrefix = string.Empty;
            });


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
