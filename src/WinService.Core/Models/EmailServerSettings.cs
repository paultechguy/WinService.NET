// -------------------------------------------------------------------------
// <copyright file="EmailServerSettings.cs" company="CompanyName">
// Copyright (c) Paul Carver. All rights reserved.
// </copyright>
// Use of this source code is governed by an MIT-style license that can
// be found in the LICENSE file or at https://opensource.org/licenses/MIT.
// -------------------------------------------------------------------------

namespace WinService.Core.Models;

public class EmailServerSettings
{
   public const string ConfigurationName = "EmailServerSettings";

   // email server

   public string? ServerHost { get; set; }

   public int ServerPort { get; set; }

   public bool ServerEnableSsl { get; set; }

   public string? ServerUsername { get; set; }

   public string? ServerPassword { get; set; }
}
