// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Held.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// <summary>
//   Represents Held order status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// Copyright 2015 Sitecore Corporation A/S
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
// except in compliance with the License. You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the 
// License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
// either express or implied. See the License for the specific language governing permissions 
// and limitations under the License.
// -------------------------------------------------------------------------------------------

namespace Sitecore.Ecommerce.Orders.Statuses
{
  /// <summary>
  /// Represents Held order status.
  /// </summary>
  public class Held : OrderStatusBase
  {
    /// <summary>
    /// Processes the specified order.
    /// </summary>
    /// <typeparam name="T">The order type.</typeparam>
    /// <param name="order">The order instance.</param>
    protected override void Process<T>(T order)
    {
    }
  }
}