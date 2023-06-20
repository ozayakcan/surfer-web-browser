﻿using System.Windows.Forms;
using CefSharp;
using EasyTabs;
using Surfer.BrowserSettings;

namespace Surfer.Forms
{
    public partial class MyAppContainer : TitleBarTabs
    {
        public MyAppContainer()
        {
            InitializeComponent();
            AeroPeekEnabled = true;
            TabRenderer = new ChromeTabRenderer(this);
            TabSelected += MyAppContainer_TabSelected;
        }

        private void MyAppContainer_TabSelected(object sender, TitleBarTabEventArgs e)
        {
            foreach (var tab in Tabs)
            {
                Browser browser = (tab.Content as Browser);
                if (browser.searchPopupForm != null)
                    browser.searchPopupForm.Visible = tab.Active;
            }
        }

        public override TitleBarTab CreateTab()
        {
            TitleBarTab titlebarTab = new TitleBarTab(this);
            titlebarTab.Content = new Browser(this, titlebarTab)
            {
                StartUrl = MyBrowserSettings.HomePage,
            };
            return titlebarTab;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Browser myBrowser = (Browser)SelectedTab.Content;
            return myBrowser.KeyEvents(myBrowser.chBrowser, CefEventFlags.None, keyData, base.ProcessCmdKey(ref msg, keyData));
        }
    }
}
