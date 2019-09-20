using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQRSSampleApp.Commands;
using CQRSSampleApp.Events;
using CQRSSampleApp.Models.Mongo;
using CQRSSampleApp.Models.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CQRSSampleApp
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddDbContext<CustomerSQLiteDatabaseContext>(options => options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<CustomerSQLiteRepository>();
            services.AddTransient<CustomerMongoRepository>();
            services.AddTransient<AMQPEventPublisher>();
            services.AddSingleton<CustomerMessageListener>();
            services.AddScoped<ICommandHandler<Command>, CustomerCommandHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<CustomerSQLiteDatabaseContext>();
                context.Database.EnsureCreated();
            }

            new Thread(() =>
            {
                app.ApplicationServices.GetService<CustomerMessageListener>().Start(env.ContentRootPath);
            }).Start();
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
