// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityResultDataConverter.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// <summary>
//   Converts item collection to the result data grid.
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

namespace Sitecore.Ecommerce.Shell.Applications.Catalogs.Models.Search
{
  using System;
  using System.Collections.Generic;
  using System.Reflection;
  using Data;
  using Data.Mapping;
  using Diagnostics;
  using DomainModel.Data;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Utils;

  /// <summary>
  /// Converts entity collection to the result data grid.
  /// </summary>
  /// <typeparam name="T">The entity type</typeparam>
  public class EntityResultDataConverter<T> : IResultDataConverter<IEnumerable<T>>
  {
    /// <summary>
    /// Converts the specified initial result.
    /// </summary>
    /// <param name="initialResult">The initial result.</param>
    /// <param name="columns">The columns.</param>
    /// <returns>
    /// The data converted to the GridData format.
    /// </returns>
    public virtual GridData Convert(IEnumerable<T> initialResult, List<GridColumn> columns)
    {
      Assert.ArgumentNotNull(initialResult, "initialResult");
      Assert.ArgumentNotNull(columns, "columns");

      GridData data = new GridData();

      foreach (T item in initialResult)
      {
        List<string> row = new List<string>(columns.Count);
        foreach (GridColumn column in columns)
        {
          if (column.RowID)
          {
            object mappingObject = this.GetMappingObject(item);
            object dataObject = mappingObject ?? item;

            if (dataObject is IEntity && ID.IsID(((IEntity)dataObject).Alias))
            {
              row.Add(ID.Parse(((IEntity)dataObject).Alias).ToString());
              continue;
            }
          }

          if (string.IsNullOrEmpty(column.FieldName))
          {
            row.Add(string.Empty);
            continue;
          }

          string value = this.GetProductFiledValue(item, column);
          if (DateUtil.IsIsoDate(value) && !string.IsNullOrEmpty(column.FormatString))
          {
            value = DateUtil.FormatIsoDate(value, column.FormatString);
          }

          row.Add(value);
        }

        data.Rows.Add(row);
      }

      return data;
    }

    /// <summary>
    /// Gets the product filed value.
    /// </summary>
    /// <param name="container">The container.</param>
    /// <param name="column">The column.</param>
    /// <returns>The value of the field.</returns>
    protected virtual string GetProductFiledValue(T container, GridColumn column)
    {
      Assert.ArgumentNotNull(container, "container");
      Assert.ArgumentNotNull(column, "column");

      string value = null;

      MappingItemMatcher matcher = Context.Entity.Resolve<MappingItemMatcher>();

      foreach (PropertyInfo info in container.GetType().GetProperties())
      {
        if (!string.IsNullOrEmpty(value))
        {
          break;
        }

        if (!info.CanRead)
        {
          continue;
        }

        IEnumerable<string> names = matcher.GetFieldNames(info);
        foreach (string name in names)
        {
          if (!string.IsNullOrEmpty(value))
          {
            break;
          }

          if (column.FieldName != name)
          {
            continue;
          }

          object instanceValue = info.GetValue(container, null);
          value = TypeUtil.TryParse(instanceValue, string.Empty);

          if (!ID.IsID(value))
          {
            continue;
          }

          Item item = Sitecore.Context.Database.GetItem(value) ?? Sitecore.Context.ContentDatabase.GetItem(value);
          if (item != null)
          {
            value = item.DisplayName;
          }
        }
      }

      if (value == null)
      {
        PropertyInfo propertyInfo = container.GetType().GetProperty("Specifications");
        if (propertyInfo != null)
        {
          object specification = propertyInfo.GetValue(container, null);

          if (specification != null)
          {
            MethodInfo methodInfo = specification.GetType().GetMethod("ContainsKey", new[] { typeof(string) });

            if (true.Equals(methodInfo.Invoke(specification, new object[] { column.FieldName })))
            {
              propertyInfo = specification.GetType().GetProperty("Item", new[] { typeof(string) });

              if (propertyInfo != null)
              {
                object rawValue = propertyInfo.GetValue(specification, new object[] { column.FieldName });

                value = TypeUtil.TryParse(rawValue, string.Empty);

                if (ID.IsID(value))
                {
                  Item item = Sitecore.Context.Database.GetItem(value) ?? Sitecore.Context.ContentDatabase.GetItem(value);
                  if (item != null)
                  {
                    value = item.DisplayName;
                  }
                }
              }
            }
          }
        }
      }

      if (value == null && container is IEntity)
      {
        string id = ((IEntity)container).Alias;
        if (!string.IsNullOrEmpty(id))
        {
          Item item = Sitecore.Context.Database.GetItem(id);
          if (item != null)
          {
            value = item[column.FieldName];
            if (ID.IsID(value))
            {
              Item linkedItem = item.Database.GetItem(value);
              if (linkedItem != null)
              {
                value = linkedItem.Name;
              }
            }
          }
        }
      }

      return value ?? string.Empty;
    }

    /// <summary>
    /// Determines whether [is napping rule] [the specified item].
    /// </summary>
    /// <typeparam name="T">The object type.</typeparam>
    /// <param name="item">The object.</param>
    /// <returns>
    /// <c>true</c> if [is napping rule] [the specified item]; otherwise, <c>false</c>.
    /// </returns>
    protected virtual object GetMappingObject(T item)
    {
      Assert.ArgumentNotNull(item, "item");

      foreach (var i in item.GetType().GetInterfaces())
      {
        if (!i.IsGenericType || i.GetGenericTypeDefinition() != typeof(IMappingRule<object>).GetGenericTypeDefinition())
        {
          continue;
        }

        Type mappingObjectType = i.GetGenericArguments()[0];
        foreach (PropertyInfo propertyInfo in item.GetType().GetProperties())
        {
          if (propertyInfo.PropertyType.IsAssignableFrom(mappingObjectType))
          {
            return propertyInfo.GetValue(item, null);
          }
        }
      }

      return null;
    }
  }
}