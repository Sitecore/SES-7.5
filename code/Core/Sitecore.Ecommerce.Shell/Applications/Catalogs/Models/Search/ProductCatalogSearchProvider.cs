// -------------------------------------------------------------------------------------------
// <copyright file="ProductCatalogSearchProvider.cs" company="Sitecore Corporation">
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

namespace Sitecore.Ecommerce.Shell.Applications.Catalogs.Models.Search
{
  using System;
  using System.Collections.Generic;
  using DomainModel.Products;
  using Ecommerce.Search;
  using Utils;

  /// <summary>
  /// Catalog Search provider
  /// </summary>
  [Obsolete]
  public class ProductCatalogSearchProvider : ISearchProvider
  {
    /// <summary>
    /// Provide search with the specified query.
    /// </summary>
    /// <param name="options">The search options.</param>
    /// <returns>The search result.</returns>
    public virtual GridData Search(SearchOptions options)
    {
      CatalogQueryBuilder builder = new CatalogQueryBuilder();
      Query query = builder.BuildQuery(options);

      IProductRepository productRepository = Context.Entity.Resolve<IProductRepository>();
      IEnumerable<ProductBaseData> products = productRepository.Get<ProductBaseData, Query>(query);
      return products.IsNullOrEmpty() ? new GridData() : new EntityResultDataConverter<ProductBaseData>().Convert(products, options.GridColumns);
    }
  }
}