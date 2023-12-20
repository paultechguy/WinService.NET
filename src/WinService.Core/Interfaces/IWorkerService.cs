﻿// -------------------------------------------------------------------------
// <copyright file="IWorkerService.cs" company="CompanyName">
// Copyright (c) Paul Carver. All rights reserved.
// </copyright>
// Use of this source code is governed by an MIT-style license that can
// be found in the LICENSE file or at https://opensource.org/licenses/MIT.
// -------------------------------------------------------------------------

namespace WinService.Core.Interfaces;

using System.Threading.Tasks;

public interface IWorkerService
{
   Task ExecuteAsync(CancellationToken cancelToken);
}
