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

namespace Agon
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            MongoUtils.MongoManager.GetConnectionString;
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Register identity framework services and also Mongo storage. 
            services.AddIdentityWithMongoStores(Configuration["MongoConnection"])
                
                .AddDefaultTokenProviders();

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();


            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715
            app.UseSpotifyAuthentication(new SpotifyAuthenticationOptions()
            {
                ClientId = Configuration["SpotifyClientId"],
                ClientSecret = Configuration["SpotifyClientSecret"],
                SaveTokens = true,
                Scope = { "playlist-read-private" }
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
