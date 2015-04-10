<%--=======================================================================================
Copyright 2015 Sitecore Corporation A/S
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
except in compliance with the License. You may obtain a copy of the License at
    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the 
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, 
either express or implied. See the License for the specific language governing permissions 
and limitations under the License.
======================================================================================--%>
<%@ Page Language="C#" Debug="true" %>
<%@ Import Namespace="System" %>
<%@ Import Namespace="System.Threading" %>
<script runat="server">
  protected override void OnLoad(EventArgs args)
  {
     Delegate cb = Thread.GetData(Thread.GetNamedDataSlot("WebTestRunner")) as Delegate;
     Thread.SetData(Thread.GetNamedDataSlot("WebTestRunner"), cb.DynamicInvoke(new object[0]));
  }
</script>