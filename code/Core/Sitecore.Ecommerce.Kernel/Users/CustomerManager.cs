// -------------------------------------------------------------------------------------------
// <copyright file="CustomerManager.cs" company="Sitecore Corporation">
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

namespace Sitecore.Ecommerce.Users
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web.Security;
  using CheckOuts;
  using Configuration;
  using Diagnostics;
  using DomainModel.Addresses;
  using DomainModel.Carts;
  using DomainModel.CheckOuts;
  using DomainModel.Data;
  using DomainModel.Mails;
  using DomainModel.Users;
  using Security;
  using SecurityModel;
  using Sitecore.Data;
  using Sitecore.Data.Items;
  using Sitecore.Data.Managers;
  using Sitecore.Data.Templates;
  using Sitecore.Pipelines;
  using Sitecore.Security;
  using Sitecore.Security.Accounts;
  using Sitecore.Security.Authentication;
  using Utils;

  /// <summary>
  /// The customer info class.
  /// </summary>
  /// <typeparam name="T">The CustomerInfo type.</typeparam>
  public class CustomerManager<T> : ICustomerManager<T> where T : DomainModel.Users.CustomerInfo
  {
    #region Fields

    /// <summary>
    /// Customer profile item id.
    /// </summary>
    private static readonly string customerProfileItemId = "{AC45E370-869B-4AEA-BC51-C4AFB248D498}";

    /// <summary>
    /// The user profile name.
    /// </summary>
    private static readonly string userProfileName = "Name";

    /// <summary>
    /// The user profile e-mail.
    /// </summary>
    private static readonly string userProfileEmail = "E-mail";

    /// <summary>
    /// The user profile customer id.
    /// </summary>
    private static readonly string userProfileCustomerId = "Customer ID";

    /// <summary>
    /// The billing address name.
    /// </summary>
    private static readonly string billingAddressName = "Billing Address Name";

    /// <summary>
    /// The billing address name 2.
    /// </summary>
    private static readonly string billingAddressName2 = "Billing Address Name 2";

    /// <summary>
    /// The billing address.
    /// </summary>
    private static readonly string billingAddress = "Billing Address";

    /// <summary>
    /// The billing address 2.
    /// </summary>
    private static readonly string billingAddress2 = "Billing Address 2";

    /// <summary>
    /// The billing address country code.
    /// </summary>
    private static readonly string billingAddressCountryCode = "Billing Address Country Code";

    /// <summary>
    /// The billing address zip.
    /// </summary>
    private static readonly string billingAddresZip = "Billing Address Zip";

    /// <summary>
    /// The billing address city.
    /// </summary>
    private static readonly string billingAddresCity = "Billing Address City";

    /// <summary>
    /// The billing address state.
    /// </summary>
    private static readonly string billingAddresState = "Billing Address State";

    /// <summary>
    /// The shipping address name.
    /// </summary>
    private static readonly string shippingAddressName = "Shipping Address Name";

    /// <summary>
    /// The shipping address name 2.
    /// </summary>
    private static readonly string shippingAddressName2 = "Shipping Address Name 2";

    /// <summary>
    /// The shipping address.
    /// </summary>
    private static readonly string shippingAddress = "Shipping Address";

    /// <summary>
    /// The shipping address 2.
    /// </summary>
    private static readonly string shippingAddress2 = "Shipping Address 2";

    /// <summary>
    /// The shipping address country code.
    /// </summary>
    private static readonly string shippingAddressCountryCode = "Shipping Address Country Code";

    /// <summary>
    /// The shipping address zip.
    /// </summary>
    private static readonly string shippingAddresZip = "Shipping Address Zip";

    /// <summary>
    /// The shipping address city.
    /// </summary>
    private static readonly string shippingAddresCity = "Shipping Address City";

    /// <summary>
    /// The shipping address state.
    /// </summary>
    private static readonly string shippingAddresState = "Shipping Address State";

    /// <summary>
    /// Defines the database that contains profile item.
    /// </summary>
    private static readonly string profileDatabase = "core";

    #endregion

    /// <summary>
    /// Gets or sets the membership.
    /// </summary>
    /// <value>
    /// The membership.
    /// </value>
    [CanBeNull]
    public CustomerMembership CustomerMembership { get; set; }

    /// <summary>
    /// Gets or sets the current user.
    /// </summary>
    /// <value>The current user.</value>
    public virtual T CurrentUser
    {
      get
      {
        T customerInfo = Context.Entity.GetInstance<T>();

        if (!string.IsNullOrEmpty(customerInfo.NickName) && !string.IsNullOrEmpty(customerInfo.CustomerId))
        {
          return customerInfo;
        }

        MembershipUser membershipUser = Membership.GetUser();
        if (membershipUser != null)
        {
          string name = Sitecore.Context.Domain.GetShortName(membershipUser.UserName);
          T savedCustomerInfo = this.GetCustomerInfo(name);
          if (savedCustomerInfo != null)
          {
            customerInfo = savedCustomerInfo;
          }
        }

        Context.Entity.SetInstance(customerInfo);

        return customerInfo;
      }

      set
      {
        Context.Entity.SetInstance(value);
      }
    }

    /// <summary>
    /// Resets the current user.
    /// </summary>
    public void ResetCurrentUser()
    {
      this.CurrentUser = Context.Entity.Resolve<T>();

      ShoppingCart shoppingCart = Context.Entity.GetInstance<ShoppingCart>();
      shoppingCart.CustomerInfo = this.CurrentUser;
      Context.Entity.SetInstance(shoppingCart);

      ICheckOut checkOut = Context.Entity.GetInstance<ICheckOut>();
      if (checkOut is CheckOut)
      {
        ((CheckOut)checkOut).HasOtherShippingAddressBeenChecked = false;
      }

      Context.Entity.SetInstance(checkOut);
    }

    /// <summary>
    /// Gets the customer info.
    /// </summary>
    /// <param name="nickName">Name of the nick.</param>
    /// <returns>
    /// If user account exists, returns the user info, else returns null.
    /// </returns>
    public virtual T GetCustomerInfo(string nickName)
    {
      Assert.IsNotNullOrEmpty(nickName, "User nick name is null or empty");

      T customerInfo = Context.Entity.Resolve<T>();

      string fullNickName = Sitecore.Context.Domain.GetFullName(nickName);
      User user = User.FromName(fullNickName, true);

      if (user == null)
      {
        Log.Warn(string.Format("User: '{0}' cannot be logged in", fullNickName), this);
        return customerInfo;
      }

      UserProfile userProfile = user.Profile;
      Assert.IsNotNull(userProfile, "User profile is null");

      customerInfo.NickName = nickName;

      IList<TemplateField> profileFields = this.GetProfileFields(userProfile).Where(pg => (new[] { "billing ", "shipping " }).All(t => !pg.Name.ToLower().StartsWith(t))).ToList();
      foreach (TemplateField field in profileFields)
      {
        customerInfo[field.Name] = this.GetCustomProperty(userProfile, field.Name);
      }

      string customerId = this.GetCustomProperty(userProfile, userProfileCustomerId);
      if (string.IsNullOrEmpty(customerId) || !ID.IsID(customerId))
      {
        customerInfo.CustomerId = ID.NewID.ToString();
      }
      else
      {
        customerInfo.CustomerId = customerId;
      }

      customerInfo.BillingAddress = customerInfo.BillingAddress ?? Context.Entity.Resolve<AddressInfo>();
      customerInfo.BillingAddress.Name = this.GetCustomProperty(userProfile, billingAddressName);
      customerInfo.BillingAddress.Name2 = this.GetCustomProperty(userProfile, billingAddressName2);
      customerInfo.BillingAddress.Address = this.GetCustomProperty(userProfile, billingAddress);
      customerInfo.BillingAddress.Address2 = this.GetCustomProperty(userProfile, billingAddress2);
      customerInfo.BillingAddress.Zip = this.GetCustomProperty(userProfile, billingAddresZip);
      customerInfo.BillingAddress.City = this.GetCustomProperty(userProfile, billingAddresCity);
      customerInfo.BillingAddress.State = this.GetCustomProperty(userProfile, billingAddresState);

      string billingCountryCode = this.GetCustomProperty(userProfile, billingAddressCountryCode);
      IEntityProvider<Country> countryProvider = Context.Entity.Resolve<IEntityProvider<Country>>();
      customerInfo.BillingAddress.Country = !string.IsNullOrEmpty(billingCountryCode) ? countryProvider.Get(billingCountryCode) : this.GetDefaultCountry();

      customerInfo.ShippingAddress = customerInfo.ShippingAddress ?? Context.Entity.Resolve<AddressInfo>();
      customerInfo.ShippingAddress.Name = this.GetCustomProperty(userProfile, shippingAddressName);
      customerInfo.ShippingAddress.Name2 = this.GetCustomProperty(userProfile, shippingAddressName2);
      customerInfo.ShippingAddress.Address = this.GetCustomProperty(userProfile, shippingAddress);
      customerInfo.ShippingAddress.Address2 = this.GetCustomProperty(userProfile, shippingAddress2);
      customerInfo.ShippingAddress.Zip = this.GetCustomProperty(userProfile, shippingAddresZip);
      customerInfo.ShippingAddress.City = this.GetCustomProperty(userProfile, shippingAddresCity);
      customerInfo.ShippingAddress.State = this.GetCustomProperty(userProfile, shippingAddresState);

      string shippingCountryCode = this.GetCustomProperty(userProfile, shippingAddressCountryCode);
      customerInfo.ShippingAddress.Country = !string.IsNullOrEmpty(shippingCountryCode) ? countryProvider.Get(shippingCountryCode) : customerInfo.BillingAddress.Country;

      return customerInfo;
    }

    /// <summary>
    /// Creates the customer with customer info.
    /// </summary>
    /// <param name="nickName">Name of the nick.</param>
    /// <param name="password">The password.</param>
    /// <param name="email">The email.</param>
    /// <returns>
    /// If customer is created, returns customer, else return null.
    /// </returns>
    public virtual T CreateCustomerAccount(string nickName, string password, string email)
    {
      Assert.IsNotNullOrEmpty(nickName, "User nick name is null or empty");
      Assert.IsNotNullOrEmpty(email, "User e-mail is null or empty");

      password = string.IsNullOrEmpty(password) ? Membership.GeneratePassword(8, 2) : password;

      string fullNickName = Sitecore.Context.Domain.GetFullName(nickName);

      if (User.Exists(fullNickName))
      {
        return this.GetCustomerInfo(fullNickName);
      }

      User user = User.Create(fullNickName, password);

      ID customerId = ID.NewID;

      UserProfile userProfile = user.Profile;
      Assert.IsNotNull(userProfile, "User profile is null");

      using (new SecurityDisabler())
      {
        MembershipUser membershipUser = Membership.GetUser(fullNickName);
        Assert.IsNotNull(membershipUser, "Membership user is null");

        membershipUser.Email = email;
        Membership.UpdateUser(membershipUser);

        userProfile.FullName = fullNickName;
        userProfile.Name = Sitecore.Context.Domain.GetShortName(nickName);
        userProfile.Email = email;
        userProfile.ProfileItemId = customerProfileItemId;
        userProfile.SetCustomProperty(userProfileCustomerId, customerId.ToString());
        userProfile.SetCustomProperty(userProfileEmail, email);
        userProfile.Save();
      }

      T customerInfo = Context.Entity.Resolve<T>();
      customerInfo.NickName = fullNickName;
      customerInfo.Email = email;
      customerInfo.CustomerId = customerId.ToString();
      customerInfo.BillingAddress = Context.Entity.Resolve<AddressInfo>();
      customerInfo.BillingAddress.Country = Context.Entity.Resolve<IEntityProvider<Country>>().GetDefault();
      customerInfo.ShippingAddress = Context.Entity.Resolve<AddressInfo>();
      customerInfo.ShippingAddress.Country = customerInfo.BillingAddress.Country;


      PipelineArgs args = new PipelineArgs();
      args.CustomData["user"] = user;
      args.CustomData["customerInfo"] = customerInfo;
      args.CustomData["userName"] = fullNickName;
      args.CustomData["password"] = password;
      args.CustomData["email"] = email;

      CorePipeline.Run("customerCreated", args);

      return args.CustomData["customerInfo"] as T;
    }

    /// <summary>
    /// Updates the customer info.
    /// </summary>
    /// <param name="customerInfo">The customer info.</param>
    public virtual void UpdateCustomerProfile(T customerInfo)
    {
      Assert.IsNotNull(customerInfo, "Customer info is null");

      string fullNickName = Sitecore.Context.Domain.GetFullName(customerInfo.NickName);
      User user = User.FromName(fullNickName, true);
      Assert.IsNotNull(user, "User is null");

      UserProfile userProfile = user.Profile;
      Assert.IsNotNull(userProfile, "User profile is null");

      using (new SecurityDisabler())
      {
        string userName = Sitecore.Context.Domain.GetFullName(customerInfo.NickName);

        MembershipUser membershipUser = Membership.GetUser(userName);
        Assert.IsNotNull(membershipUser, "Membership user is null");

        membershipUser.Email = customerInfo.Email;
        Membership.UpdateUser(membershipUser);

        userProfile.FullName = userName;
        userProfile.Name = Sitecore.Context.Domain.GetShortName(customerInfo.NickName);
        userProfile.Email = customerInfo.Email;

        this.SetCustomProperty(userProfile, userProfileName, userProfile.Name);

        this.SetCustomProperty(userProfile, billingAddressName, customerInfo.BillingAddress.Name);
        this.SetCustomProperty(userProfile, billingAddressName2, customerInfo.BillingAddress.Name2);
        this.SetCustomProperty(userProfile, billingAddress, customerInfo.BillingAddress.Address);
        this.SetCustomProperty(userProfile, billingAddress2, customerInfo.BillingAddress.Address2);
        this.SetCustomProperty(userProfile, billingAddressCountryCode, customerInfo.BillingAddress.Country.Code);
        this.SetCustomProperty(userProfile, billingAddresZip, customerInfo.BillingAddress.Zip);
        this.SetCustomProperty(userProfile, billingAddresCity, customerInfo.BillingAddress.City);
        this.SetCustomProperty(userProfile, billingAddresState, customerInfo.BillingAddress.State);

        this.SetCustomProperty(userProfile, shippingAddressName, customerInfo.ShippingAddress.Name);
        this.SetCustomProperty(userProfile, shippingAddressName2, customerInfo.ShippingAddress.Name2);
        this.SetCustomProperty(userProfile, shippingAddress, customerInfo.ShippingAddress.Address);
        this.SetCustomProperty(userProfile, shippingAddress2, customerInfo.ShippingAddress.Address2);
        this.SetCustomProperty(userProfile, shippingAddressCountryCode, customerInfo.ShippingAddress.Country.Code);
        this.SetCustomProperty(userProfile, shippingAddresZip, customerInfo.ShippingAddress.Zip);
        this.SetCustomProperty(userProfile, shippingAddresCity, customerInfo.ShippingAddress.City);
        this.SetCustomProperty(userProfile, shippingAddresState, customerInfo.ShippingAddress.State);

        IList<TemplateField> profileFields = this.GetProfileFields(userProfile).Where(pg => (new[] { "billing ", "shipping " }).All(t => !pg.Name.ToLower().StartsWith(t))).ToList();

        foreach (TemplateField field in profileFields)
        {
          this.SetCustomProperty(userProfile, field.Name, customerInfo[field.Name]);
        }

        userProfile.Save();
      }
    }

    /// <summary>
    /// Log in the customer.
    /// </summary>
    /// <param name="nickName">Name of the nick.</param>
    /// <param name="password">The customer password.</param>
    /// <returns>The login result.</returns>
    public virtual bool LogInCustomer(string nickName, string password)
    {
      string fullNickName = Sitecore.Context.Domain.GetFullName(nickName);
      bool loginSucces = AuthenticationManager.Login(fullNickName, password);

      if (loginSucces)
      {
        T customerInfo = this.GetCustomerInfo(fullNickName);
        if (customerInfo != null)
        {
          this.CurrentUser = customerInfo;

          ShoppingCart shoppingCart = Context.Entity.GetInstance<ShoppingCart>();
          shoppingCart.CustomerInfo = customerInfo;
          Context.Entity.SetInstance(shoppingCart);

          return true;
        }

        Sitecore.Context.Logout();
      }

      return false;
    }

    public virtual bool LogInCustomer(T customerInfo, string password)
    {
      string fullNickName = Sitecore.Context.Domain.GetFullName(customerInfo.NickName);
      return this.LogInCustomer(fullNickName, password);
    }

    public virtual void NotifyUser(string mailTemplate, T customerInfo)
    {
      var usersParams = new { UserName = customerInfo.NickName, CustomerEmail = customerInfo.Email };
      this.NotifyUser(mailTemplate, usersParams);
    }

    public virtual void NotifyUser(string mailTemplate, object userParams)
    {
      IMail mailProvider = Context.Entity.Resolve<IMail>();
      mailProvider.SendMail(mailTemplate, userParams, string.Empty);
    }

    /// <summary>
    /// Deletes the customer.
    /// </summary>
    /// <param name="nickName">Name of the nick.</param>
    public virtual void DeleteCustomer(string nickName)
    {
      string fullNickName = Sitecore.Context.Domain.GetFullName(nickName);
      if (User.Exists(fullNickName))
      {
        User user = User.FromName(fullNickName, false);
        if (this.IsCustomer(user))
        {
          user.Delete();
        }
      }
    }

    /// <summary>
    /// Deletes the customer.
    /// </summary>
    /// <param name="customerInfo">The customer info.</param>
    public virtual void DeleteCustomer(T customerInfo)
    {
      Assert.ArgumentNotNull(customerInfo, "customerInfo");
      Assert.ArgumentNotNull(customerInfo.NickName, "CustomerInfo.NickName");

      this.DeleteCustomer(customerInfo.NickName);
    }

    /// <summary>
    /// Deletes all customers.
    /// </summary>
    public virtual void DeleteAllCustomers()
    {
      foreach (Account acount in Sitecore.Context.Domain.GetAccounts())
      {
        this.DeleteCustomer(acount.LocalName);
      }
    }

    /// <summary>
    /// Determines whether the specified user is customer.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>
    ///   <c>true</c> if the specified user is customer; otherwise, <c>false</c>.
    /// </returns>
    [Obsolete("Use Sitecore.Ecommerce.Security.CustomerMembership.IsCustomer instead.")]
    protected virtual bool IsCustomer([NotNull] User user)
    {
      Assert.ArgumentNotNull(user, "user");
      Assert.IsNotNull(this.CustomerMembership, "Customer Membership cannot be null.");

      return CustomerMembership.IsCustomer(user);
    }

    /// <summary>
    /// Gets the default country.
    /// </summary>
    /// <returns>The default country.</returns>
    /// <exception cref="ArgumentException">List of countries is empty.</exception>
    protected virtual Country GetDefaultCountry()
    {
      IEntityProvider<VatRegion> vatRegionprovider = Context.Entity.Resolve<IEntityProvider<VatRegion>>();
      IEntityProvider<Country> countryProvider = Context.Entity.Resolve<IEntityProvider<Country>>();

      VatRegion defaultVatRegion = vatRegionprovider.GetDefault();

      IEnumerable<Country> countries = countryProvider.GetAll();
      if (countries.IsNullOrEmpty())
      {
        throw new ArgumentException("List of countries is empty.");
      }

      foreach (Country country in countries)
      {
        if (country.VatRegion == null || string.IsNullOrEmpty(country.VatRegion.Name))
        {
          continue;
        }

        if (country.VatRegion.Name.Equals(defaultVatRegion.Name, StringComparison.OrdinalIgnoreCase))
        {
          return country;
        }
      }

      return countryProvider.GetDefault();
    }

    /// <summary>
    /// Sets the custom properties.
    /// </summary>
    /// <param name="userProfile">The user profile.</param>
    /// <param name="name">The property name.</param>
    /// <param name="value">The value.</param>
    protected virtual void SetCustomProperty(UserProfile userProfile, string name, string value)
    {
      if (!string.IsNullOrEmpty(value))
      {
        userProfile.SetCustomProperty(name, value);
      }
    }

    /// <summary>
    /// Gets the custom property.
    /// </summary>
    /// <param name="userProfile">The user profile.</param>
    /// <param name="name">The property name.</param>
    /// <returns>The value.</returns>
    protected virtual string GetCustomProperty(UserProfile userProfile, string name)
    {
      string value = userProfile.GetCustomProperty(name);

      return string.IsNullOrEmpty(value) ? string.Empty : value;
    }

    /// <summary>
    /// Gets the list of the template fields.
    /// </summary>
    /// <param name="userProfile">The user profile.</param>
    /// <returns>The list of the fields.</returns>
    protected IEnumerable<TemplateField> GetProfileFields(UserProfile userProfile)
    {
      IList<TemplateField> profileFields = new List<TemplateField>();
      using (new SecurityDisabler())
      {
        Database database = Factory.GetDatabase(profileDatabase);
        if (userProfile.ProfileItemId != null)
        {
          Item item = database.GetItem(userProfile.ProfileItemId);
          if (item != null)
          {
            Template template = TemplateManager.GetTemplate(item.TemplateID, database);
            if (template != null)
            {
              profileFields = template.GetFields().Where(tf => tf.Template == template).ToList();
            }
          }
        }
      }

      return profileFields;
    }
  }
}