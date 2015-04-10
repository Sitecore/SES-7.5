// -------------------------------------------------------------------------------------------
// <copyright file="Canceled.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// -------------------------------------------------------------------------------------------
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
  using System;
  using DomainModel.Orders;
  using DomainModel.Payments;
  using Sitecore.Exceptions;

  /// <summary>
  /// The Canceled state.
  /// </summary>
  public class Canceled : OrderStatusBase
  {
    /// <summary>
    /// Processes the specified order.
    /// </summary>
    /// <typeparam name="T">The order type.</typeparam>
    /// <param name="order">The order instance.</param>
    /// <exception cref="InvalidTypeException">The provider doesn't implement IReservable interface.</exception>
    protected override void Process<T>(T order)
    {
      bool cancelSuccess = false;
      ReservationTicket reservationTicket = new ReservationTicket(order);
      PaymentArgs paymentArgs = new PaymentArgs();
    
      PaymentProvider paymentProvider = Context.Entity.Resolve<PaymentProvider>(order.PaymentSystem.Code);
      IReservable reservableProvider = paymentProvider as IReservable;

      if (reservableProvider != null)
      {
        reservableProvider.CancelReservation(order.PaymentSystem, paymentArgs, reservationTicket);

        ITransactionData transactionDataProvider = Context.Entity.Resolve<ITransactionData>();
        string result = transactionDataProvider.GetPersistentValue(order.OrderNumber) as string;

        if (string.Compare(result, "Canceled", StringComparison.OrdinalIgnoreCase) == 0)
        {
          cancelSuccess = true;
        }
      }

      if (!cancelSuccess)
      {
        order.Status = Context.Entity.Resolve<OrderStatus>("Processing");
        IOrderManager<Order> orderManager = Context.Entity.Resolve<IOrderManager<Order>>();
        orderManager.SaveOrder(order);
      }

      if (reservableProvider == null)
      {
        throw new InvalidTypeException("The provider doesn't implement IReservable interface.");
      }
    }
  }
}