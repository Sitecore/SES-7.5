﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyOrderMappingRule.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// <summary>
//   Defines the MyOrderMappingRule class.
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

namespace Sitecore.Ecommerce.Examples.Orders
{
  using Data;

  public class MyOrderMappingRule : OrderMappingRule
  {
    /// <summary>
    /// Gets or sets the hobby.
    /// </summary>
    /// <value>
    /// The hobby.
    /// </value>
    [Entity(FieldName = "Hobby")]
    public virtual string Hobby
    {
      get { return ((MyOrder)this.MappingObject).MyCustomerInfo.Hobby; }
      set { ((MyOrder)this.MappingObject).MyCustomerInfo.Hobby = value; }
    }
  }
}