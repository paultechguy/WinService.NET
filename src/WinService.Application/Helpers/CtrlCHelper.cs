﻿// -------------------------------------------------------------------------
// <copyright file="CtrlCHelper.cs" company="CompanyName">
// Copyright (c) Paul Carver. All rights reserved.
// </copyright>
// Use of this source code is governed by an MIT-style license that can
// be found in the LICENSE file or at https://opensource.org/licenses/MIT.
// -------------------------------------------------------------------------

namespace WinService.Application.Helpers;

public static class CtrlCHelper
{
   public static void ConfigureCtrlCHandler(ConsoleCancelEventHandler handler)
   {
      Console.CancelKeyPress += handler;
   }
}
