// -------------------------------------------------------------------------
// <copyright file="Program_Configure.cs" company="CompanyName">
// Copyright (c) Paul Carver. All rights reserved.
// </copyright>
// Use of this source code is governed by an MIT-style license that can
// be found in the LICENSE file or at https://opensource.org/licenses/MIT.
// -------------------------------------------------------------------------

namespace WinService.Application;

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinService.Core;
using WinService.Core.Interfaces;
using WinService.Core.Models;
using WinService.Email;
using WinService.Service;

public partial class Program
{
   private CommandLineOptions commandLineOptions;

   // assume we are in a development environment; also note the "DOTNET_" rather than "DOTNETCORE_" or "ASPNETCORE_" environment
   private static readonly string EnvironmentName = (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "development").ToLower();

   private bool disposed = false;

   public Program()
   {
      // keep compiler happy, but we'll overwrite this in Run()
      this.commandLineOptions = new CommandLineOptions();
   }

   public void Dispose()
   {
      // Dispose of unmanaged resources.
      this.Dispose(true);

      // Suppress finalization.
      GC.SuppressFinalize(this);
   }

   protected virtual void Dispose(bool disposing)
   {
      if (this.disposed)
      {
         return;
      }

      if (disposing)
      {
         this.cancelTokenSource.Dispose();
      }

      // free unmanaged resources (unmanaged objects) and override a finalizer below;
      // set large fields to null.

      this.disposed = true;
   }

   private void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
   {
      // app settings DI using IOptions pattern
      _ = services.Configure<WorkerServiceSettings>(hostContext.Configuration.GetSection(WorkerServiceSettings.ConfigurationName));
      _ = services.Configure<EmailServerSettings>(hostContext.Configuration.GetSection(EmailServerSettings.ConfigurationName));

      // normal DI stuff
      _ = services.AddTransient<IWorkerService, WorkerService>();
      _ = services.AddTransient<IEmailSender, EmailSender>();
      _ = services.AddSingleton(this.commandLineOptions);
      _ = services.AddHostedService<WindowsBackgroundService>();

      // disable the default aspnet status messages that appear in console/log about startup
      _ = services.Configure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);
   }

   private IHostBuilder CreateHostBuilder()
   {
      // build configuration first so we can use it (e.g. default service name)
      IConfigurationRoot configuration = new ConfigurationBuilder()
         .SetBasePath(Directory.GetCurrentDirectory())
         .AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
         .AddJsonFile($"appsettings.user.json", optional: true, reloadOnChange: true)
         .AddJsonFile($"appsettings.{EnvironmentName}.json", optional: true, reloadOnChange: true)
         .AddJsonFile($"appsettings.{EnvironmentName}.user.json", optional: true, reloadOnChange: true)
         .AddEnvironmentVariables()
         .AddCommandLine(Environment.GetCommandLineArgs())
         .Build();

      // this value really doesn't do much since the service name is passed in
      // via the CLI, Windows sc.exe command, for the various commands like
      // create, start, stop, etc.
      string? defaultServiceName = configuration.GetValue<string>("generalSettings:defaultServiceName");

      IHostBuilder builder = Host.CreateDefaultBuilder()
         .ConfigureAppConfiguration(builder =>
         {
            builder.Sources.Clear();
            builder.AddConfiguration(configuration);
         })
         .ConfigureServices(this.ConfigureServices)
         .UseSerilog((hostingContext, services, loggerConfiguration) =>
         {
            loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration);

            // if we're in an interactive environment, add console output
            if (Environment.UserInteractive)
            {
               loggerConfiguration.WriteTo.Console();
            }
         })
         .UseWindowsService(options =>
         {
            options.ServiceName = defaultServiceName ?? "WinService Service";
         });

      return builder;
   }
}
