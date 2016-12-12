﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Agon.Models;
using AspNet.Security.OAuth.Spotify;
using MongoUtils;
using SpotifyUtils;
using Microsoft.AspNetCore.Identity.MongoDB;

namespace Agon
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            builder.AddEnvironmentVariables();

            Configuration = builder.Build();
            MongoManager.SetupMongoClient(Configuration["MongoConnection"]);
            SpotifyManager.SetVariables(Configuration["SpotifyClientId"], Configuration["SpotifyClientSecret"]);
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register identity framework services and also Mongo storage.
            services.AddIdentity<IdentityUser, IdentityRole>(o => o.Cookies.ApplicationCookie.LoginPath = "/Account/ExternalLogin");
            services.AddIdentityWithMongoStores(MongoManager.MongoConnection)
                .AddDefaultTokenProviders();

            services.AddMvc();
            services.AddSession();
            services.AddMemoryCache();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSession(new SessionOptions { CookieHttpOnly = false });
            //if (env.IsDevelopment())
            app.UseDeveloperExceptionPage();
            //else
            //    app.UseExceptionHandler("/Home/Error");

            app.UseStaticFiles();
            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715
            app.UseSpotifyAuthentication(new SpotifyAuthenticationOptions()
            {
                ClientId = SpotifyManager.SpotifyClientId,
                ClientSecret = SpotifyManager.SpotifyClientSecret,
                SaveTokens = true,
                Scope = { "playlist-read-private playlist-modify-private" }                
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            //app.Run(async (context) =>
            //{
            //    var output = Configuration["MongoConnection"];
            //    await context.Response.WriteAsync("Output:" + output);
            //});
        }
    }
}
