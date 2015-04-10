// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Item.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// <summary>
//   Defines the Item class.
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

namespace Sitecore.Ecommerce.OrderManagement.Orders
{
  public class Item
  {
    /// <summary>
    /// Description of the item
    /// </summary>
    public virtual string Description { get; set; }

    /// <summary>
    /// Quantity of package
    /// </summary>
    public virtual decimal PackQuantity { get; set; }

    /// <summary>
    /// Number of items in package unit
    /// </summary>
    public virtual decimal PackSizeNumeric { get; set; }

    /// <summary>
    /// Code of the item.
    /// </summary>
    /// <value>The code.</value>
    public virtual string Code { get; set; }
    
    /// <summary>
    /// Name of the item
    /// </summary>
    public virtual string Name { get; set; }

    /// <summary>
    /// Additional info
    /// </summary>
    public virtual string AdditionalInformation { get; set; }

    /// <summary>
    /// Keywords for search
    /// </summary>
    public virtual string Keyword { get; set; }

    /// <summary>
    /// Brand name
    /// </summary>
    public virtual string BrandName { get; set; }

    /// <summary>
    /// Model Name
    /// </summary>
    public virtual string ModelName { get; set; }
  }
}
