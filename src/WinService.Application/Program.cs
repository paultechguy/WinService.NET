// -------------------------------------------------------------------------
// <copyright file="Program.cs" company="CompanyName">
// Copyright (c) Paul Carver. All rights reserved.
// </copyright>
// Use of this source code is governed by an MIT-style license that can
// be found in the LICENSE file or at https://opensource.org/licenses/MIT.
// -------------------------------------------------------------------------

namespace WinService.Application;

using System;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinService.Application.Helpers;
using WinService.Core.Models;

public partial class Program : IDisposable
{
   private readonly CancellationTokenSource cancelTokenSource = new();

   /// <summary>
   /// Main program.
   /// </summary>
   /// <param name="args">The command-line arguments.</param>
   private static void Main(string[] args)
   {
      // first things first...need to set content root
      Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

      new Program().Run(args);
   }

   private void Run(string[] args)
   {
      ParserResult<CommandLineOptions> result = Parser.Default.ParseArguments<CommandLineOptions>(args)
      .WithParsed(cmdLineOptions =>
      {
         // so we can use for dependency injections
         this.commandLineOptions = cmdLineOptions;

         // the initial bootstrap logger is able to log errors during start-up;
         // it's fully replaced by the logger configured in `UseSerilog()`
         Log.Logger = new LoggerConfiguration()
             .WriteTo.Console()
             .CreateBootstrapLogger();

         try
         {
            LogStarting();

            // build host first so we can hope to have a logger if issues come up
            IHost host = this.CreateHostBuilder().Build();

            // intialize and notify user of use
            this.InitializeEnvironment();

            //
            // run the service!
            //
            host.RunAsync(this.cancelTokenSource.Token).Wait();
         }
         catch (Exception ex)
         {
            string message = $"Top-level application exception caught: {ex}";

            // logger never initialized successfully
            if (Log.Logger.GetType().Name == "SilentLogger")
            {
               Console.ForegroundColor = ConsoleColor.Red;
               Console.Error.WriteLine(message);
               Console.ResetColor();
            }
            else
            {
               Log.Information(message);
            }
         }
      })
      .WithNotParsed(errors => // errors is a sequence of type IEnumerable<Error>
      {
      });

      // final user notifications
      LogStopping();

      // all done...close logger
      CloseLogger();

      // Terminate this process and return an exit code to the operating system.
      // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
      // performs one of two scenarios:
      // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
      // 2. When set to "StopHost": will cleanly stop the host, and log errors.
      //
      // In order for the Windows Service Management system to leverage configured
      // recovery options, we need to terminate the process with a non-zero exit code.

      Environment.Exit(1);
   }

   private static void LogStarting()
   {
      Log.Information($"Starting WinService.{nameof(Main)}");

      if (Environment.UserInteractive)
      {
         Log.Information("{msg}", $"Press Ctrl-C to cancel");
      }
   }

   private static void LogStopping()
   {
      Log.Information("Stopping WinService.Main");
   }

   private static void CloseLogger()
   {
      // serilog flush
      Log.CloseAndFlush();

      // just in case...
      if (Environment.UserInteractive)
      {
         Console.Out.Flush();
      }
   }

   private void InitializeEnvironment()
   {
      Log.Information("{environment}", $"{EnvironmentName.ToUpper()} environment detected");

      // allow ctrl-c in case running in console mode
      this.ConfigureCtrlCHandler();
   }

   private void ConfigureCtrlCHandler()
   {
      // allow cancellable Ctrl-C if interactive
      if (Environment.UserInteractive)
      {
         CtrlCHelper.ConfigureCtrlCHandler((sender, e) =>
         {
            // if ctrl-c pressed
            if (!e.Cancel && e.SpecialKey == ConsoleSpecialKey.ControlC)
            {
               this.cancelTokenSource.Cancel();
               e.Cancel = true;
            }
         });
      }
   }
}
