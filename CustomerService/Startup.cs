using System;
using System.Net.Http;
using CustomerService.Controllers;
using CustomerService.Data;
using CustomerService.RabbitMq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Refit;

namespace CustomerService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("helloApi", c => { c.BaseAddress = new Uri("http://localhost:3000"); })
                .AddTransientHttpErrorPolicy(p => GetRetryPolicy());
            services.AddRefitClient<IBookApi>()
                .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:7000"))
                .AddTransientHttpErrorPolicy(p => p.CircuitBreakerAsync(2, TimeSpan.FromSeconds(5)));

            services.AddDbContext<CustomerContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DbConnectionString")));
            services.AddHostedService<BookReservedByUserEventListener>();
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CustomerService", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CustomerService v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(4, retryAttempt => TimeSpan.FromSeconds(3));
        }
    }
}
