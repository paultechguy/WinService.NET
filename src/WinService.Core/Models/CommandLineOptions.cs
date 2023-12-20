// -------------------------------------------------------------------------
// <copyright file="CommandLineOptions.cs" company="CompanyName">
// Copyright (c) Paul Carver. All rights reserved.
// </copyright>
// Use of this source code is governed by an MIT-style license that can
// be found in the LICENSE file or at https://opensource.org/licenses/MIT.
// -------------------------------------------------------------------------

namespace WinService.Core.Models;

using CommandLine;

public class CommandLineOptions
{
   [Option('s', "sleep", Required = false, Default = 5000, HelpText = "Number of milliseconds to sleep in between task executions")]
   public int ExecutionSleepMs { get; set; }
}
