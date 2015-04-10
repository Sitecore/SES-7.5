// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstallationPostStep.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2015
// </copyright>
// <summary>
//   The UPT Post Action. It is possible to register only one post action class for UPT to call. So this class is needed to invoke several post actions.
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

namespace Sitecore.Ecommerce.Install
{
  using System.Collections.Specialized;
  using Sitecore.Install.Framework;

  /// <summary>
  /// The UPT Post Action. It is possible to register only one post action class for UPT to call. So this class is needed to invoke several post actions.
  /// </summary>
  public class InstallationPostStep : IPostStep
  {
    /// <summary>
    /// Runs this post step.
    /// </summary>
    /// <param name="output">The output.</param><param name="metaData">The meta data.</param>
    public void Run(ITaskOutput output, NameValueCollection metaData)
    {
      new RebuildLinkDatabasePostStep().Run(output, metaData);
      new PublishIndexItemsPostStep().Run(output, metaData);
      new RegisterPageEventsPostStep().Run(output, metaData);
    }
  }
}