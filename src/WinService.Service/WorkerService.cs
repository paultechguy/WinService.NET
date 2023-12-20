// -------------------------------------------------------------------------
// <copyright file="WorkerService.cs" company="CompanyName">
// Copyright (c) Paul Carver. All rights reserved.
// </copyright>
// Use of this source code is governed by an MIT-style license that can
// be found in the LICENSE file or at https://opensource.org/licenses/MIT.
// -------------------------------------------------------------------------

namespace WinService.Service;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WinService.Core.Interfaces;
using WinService.Core.Models;

public class WorkerService(
   ILogger<WorkerService> logger,
   IOptions<WorkerServiceSettings> workerServiceSettings,
   IEmailSender emailSender,
   CommandLineOptions commandLineOptions)
   : IWorkerService
{
   private readonly ILogger<WorkerService> logger = logger;
   private readonly WorkerServiceSettings workerServiceSettings = workerServiceSettings.Value;
   private readonly IEmailSender emailSender = emailSender;
   private readonly CommandLineOptions commandLineOptions = commandLineOptions;

   public async Task ExecuteAsync(CancellationToken cancelToken)
   {
      // When this method exits, the application will stop.  If this is a service, you typically will
      // stay in a "while" loop until a cancellation is requested.  If this is a schedule task or
      // command-line console application, you may loop a few times, but there is a good chance that
      // the method will exit fairly soon on its own (but you should still check for a cancellation
      // request since the Ctrl-C handler will initiate a cancellation request.

      try
      {
         this.logger.LogInformation($"Starting: {nameof(WorkerService)}.{nameof(this.ExecuteAsync)}");

         // demonstrate sending email as we start executing
         await this.SendEmailAsync(cancelToken);

         // loop forever until a cancellation request is seen
         int counter = 0;
         while (!cancelToken.IsCancellationRequested)
         {
            this.logger.LogInformation($"#{++counter}: Doing something every {this.commandLineOptions.ExecutionSleepMs} milliseconds: {nameof(WorkerService)}.{nameof(this.ExecuteAsync)}...");
            _ = cancelToken.WaitHandle.WaitOne(this.commandLineOptions.ExecutionSleepMs);
         }
      }
      catch (Exception)
      {
         // bubble up
         throw;
      }
      finally
      {
         this.logger.LogInformation($"Ending: {nameof(WorkerService)}.{nameof(this.ExecuteAsync)}");
      }
   }

   private async Task SendEmailAsync(CancellationToken cancelToken)
   {
      if (!this.workerServiceSettings.MessageIsEnabled)
      {
         this.logger.LogWarning("Email support disabled.  See appSettings.WorkerServiceSettings.MessageIsEnabled");

         return;
      }

      // if it appears we have email settings, send an email if we pass validation
      if (!string.IsNullOrWhiteSpace(this.workerServiceSettings.MessageFromEmailAddress)
         && !string.IsNullOrWhiteSpace(this.workerServiceSettings.MessageToEmailAddress))
      {
         string subject = $"Email from {nameof(WorkerService)}";
         string body = $"<html><head></head><body><h1>Hello World!</h1><p>I like it.</p></body></html>";

         this.logger.LogInformation("Sending email; to:{to}, subject:{subject}", this.workerServiceSettings.MessageToEmailAddress, subject);

         await this.emailSender.SendHtmlAsync(
            this.workerServiceSettings.MessageFromEmailAddress,
            this.workerServiceSettings.MessageToEmailAddress,
            subject,
            body);
      }
   }
}
