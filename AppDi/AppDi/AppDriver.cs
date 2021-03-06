﻿using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Dynamic;

namespace AppDi
{
    public class AppDriver : DynamicObject
    {
        public Uri BaseUrl { get; }
        public Lazy<IWebDriver> WebDriver { get; }

        private Dictionary<string, Type> _pageObjects;

        /// <summary>
        /// The AppDriverFactory is responsible for creating an instance of an AppDriver
        /// </summary>
        /// <returns></returns>
        public static AppDriverFactory Factory()
        {
            return new AppDriverFactory((Uri baseUrl, Lazy<IWebDriver> webDriver, Dictionary<string, Type> PageObjectmembers) =>
            {
                return new AppDriver(baseUrl, webDriver, PageObjectmembers);
            });
        }

        /// <summary>
        /// AppDriver should only be instantiated through its factory
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="webDriver"></param>
        /// <param name="PageObjectmembers"></param>
        private AppDriver(Uri baseUrl, Lazy<IWebDriver> webDriver, Dictionary<string, Type> PageObjectmembers)
        {
            this.BaseUrl = baseUrl;
            this.WebDriver = webDriver;
            _pageObjects = new Dictionary<string, Type>(PageObjectmembers);
        }

        /// <summary>
        /// This method is in charge of dynamic binding of properties/methods in this Dynamic Object
        /// In specific for this class, it is in charge of creating instances of Page Objects that are dinamically
        /// registered with the AppDriver during the initial driver creation (using AppDriver.Factory().Register<> method)
        /// </summary>
        /// <param name="binder">Information about the member we are trying to bing</param>
        /// <param name="result">Newly created PageObject</param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            Type pageType;
            bool isPageRegistered = _pageObjects.TryGetValue(binder.Name, out pageType);

            if (isPageRegistered)
            {
                //this needs to be extracted out into it's own method that can be replaced by users if desired
                var createdPage = (PageObject)Activator.CreateInstance(pageType);
                createdPage.WebDriver = this.WebDriver.Value;
                createdPage.Url = this.BaseUrl;
                //TO DO: Make wait timeout configurable
                createdPage.Wait = new WebDriverWait(this.WebDriver.Value, new TimeSpan(0,0,10));
                PageFactory.InitElements(this.WebDriver.Value, createdPage);
                result = createdPage;
                return isPageRegistered;
            }
            else
            {
                return base.TryGetMember(binder, out result);
            }

        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {

            return base.TrySetMember(binder, value);
        }

        /// <summary>
        /// Returns the count of PageObject types that have been DYNAMICALLY registed with the current AppDriver object
        /// </summary>
        public int PageObjectsCount
        {
            get {
                return _pageObjects.Count;
            }
        }
    }
}