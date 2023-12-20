﻿// -------------------------------------------------------------------------
// <copyright file="WindowsBackgroundService.cs" company="CompanyName">
// Copyright (c) Paul Carver. All rights reserved.
// </copyright>
// Use of this source code is governed by an MIT-style license that can
// be found in the LICENSE file or at https://opensource.org/licenses/MIT.
// -------------------------------------------------------------------------

namespace WinService.Core;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WinService.Core.Interfaces;

public sealed class WindowsBackgroundService(
   IWorkerService appService,
   ILogger<WindowsBackgroundService> logger)
   : BackgroundService
{
   private readonly IWorkerService appService = appService;
   private readonly ILogger<WindowsBackgroundService> logger = logger;

   protected override async Task ExecuteAsync(CancellationToken cancelToken)
   {
      this.logger.LogInformation($"Starting {nameof(WindowsBackgroundService)}.{nameof(this.ExecuteAsync)}");

      try
      {
         await this.appService.ExecuteAsync(cancelToken);
      }
      catch (OperationCanceledException)
      {
         // When the stopping token is canceled, for example, a call made from services.msc,
         // we shouldn't exit with a non-zero exit code. In other words, this is expected...
      }
      catch (Exception ex)
      {
         this.logger.LogError($"Exception in {nameof(WindowsBackgroundService)}: {ex}");
      }

      this.logger.LogInformation($"Ending {nameof(WindowsBackgroundService)}.{nameof(this.ExecuteAsync)}");
   }
}