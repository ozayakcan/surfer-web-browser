﻿using CefSharp;
using CefSharp.WinForms;
using EasyTabs;
using Surfer.BrowserSettings;
using Surfer.Utils;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace Surfer
{
    public partial class Browser : Form
    {
        public string StartUrl {get; set;}
        public Icon OriginalIcon = Properties.Resources.tab_icon;
        public Icon SiteIcon;
        public MyAppContainer AppContainer { get; set; }
        public TitleBarTab Tab { get; set; }
        public Browser(MyAppContainer appContainer, TitleBarTab titlebarTab)
        {
            CefSettings cefSettings = new CefSettings();
            cefSettings.CachePath = Paths.AppData("Cache");
            if (!Cef.IsInitialized)
                Cef.Initialize(cefSettings);
            AppContainer = appContainer;
            Tab = titlebarTab;
            InitializeComponent();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            LoadUrl(tbUrl.Text);
        }

        public void LoadUrl(string url)
        {
            chBrowser.Load(url);
        }

        ChromiumWebBrowser chBrowser;
        private void Browser_Load(object sender, EventArgs e)
        {
            // Browser
            chBrowser = new ChromiumWebBrowser(tbUrl.Text);
            chBrowser.Dock = DockStyle.Fill;
            chBrowser.LoadingStateChanged += ChBrowser_LoadingStateChanged;
            chBrowser.DisplayHandler = new MyDisplayHandler(this);
            chBrowser.RequestHandler = new MyRequestHandler(this);
            pnlBrowser.Controls.Add(chBrowser);
            SetGoBackButtonStatus(chBrowser.CanGoBack);
            SetGoForwardButtonStatus(chBrowser.CanGoForward);
            if (!string.IsNullOrEmpty(StartUrl) && !string.IsNullOrWhiteSpace(StartUrl))
                LoadUrl(StartUrl);
        }

        internal void OpenInNewTab(string targetUrl)
        {
            InvokeAction(() =>
            {
                TitleBarTab titleBarTab = new TitleBarTab(AppContainer);
                titleBarTab.Content = new Browser(AppContainer, titleBarTab)
                {
                    StartUrl = targetUrl
                };
                int index = AppContainer.SelectedTabIndex;
                AppContainer.Tabs.Insert(index + 1, titleBarTab);
                AppContainer.SelectedTabIndex = index + 1;
                AppContainer.SelectedTabIndex = index;
            });
        }

        public void OnAddressChanged(AddressChangedEventArgs addressChangedArgs)
        {
            InvokeAction(() => {
                tbUrl.Text = addressChangedArgs.Address;
            });
        }
        
        public void SetIcon(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {

                    WebClient wc = new WebClient();
                    byte[] originalData = wc.DownloadData(url);
                    MemoryStream stream = new MemoryStream(originalData);
                    Bitmap bmp = new Bitmap(stream);
                    var thumb = (Bitmap)bmp.GetThumbnailImage(32, 32, null, IntPtr.Zero);
                    thumb.MakeTransparent();
                    Icon icon = Icon.FromHandle(thumb.GetHicon());
                    _setIcon(icon);
                }
            }
            catch
            {
                _setIcon(OriginalIcon);
            }
        }
        private void _setIcon(Icon icon)
        {
            InvokeAction(() => {
                Icon = Tab.Icon = SiteIcon = icon;
                Tab.Parent.UpdateThumbnailPreviewIcon(Tab, icon);
            });
        }

        internal void TitleChanged(TitleChangedEventArgs titleChangedArgs)
        {
            InvokeAction(() => {
                Text = titleChangedArgs.Title;
            });
        }
        public void ShowLoading(int progress)
        {
            InvokeAction(() =>
            {
                pnlProgress.Visible = true;
                pbLoading.Value = progress;
                SetRefreshButtonStatus(false);
            });
        }

        public void HideLoading()
        {
            InvokeAction(() =>
            {
                SetRefreshButtonStatus(true);
                pbLoading.Value = 0;
                pnlProgress.Visible = true;
            });
        }
        private void ChBrowser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
        {
            InvokeAction(() =>
            {
                chBrowser.Focus();
            });
            if (!e.IsLoading)
            {
                SetGoBackButtonStatus(e.CanGoBack);
                SetGoForwardButtonStatus(e.CanGoForward);
            }
            SetRefreshButtonStatus(e.IsLoading);
        }

        public void InvokeAction(Action action)
        {
            Invoke(new Action(() => {
                action();
                this.Invalidate();
                this.Update();
                this.Refresh();
                Application.DoEvents();
            }));
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (chBrowser.CanGoBack)
            {
                SetGoBackButtonStatus(false);
                chBrowser.Back();
            }
        }
        private void SetGoBackButtonStatus(bool status)
        {
            InvokeAction(() =>
            {
                btnBack.Enabled = status;
            });
        }
        private void btnForward_Click(object sender, EventArgs e)
        {

            if (chBrowser.CanGoForward)
            {
                SetGoForwardButtonStatus(false);
                chBrowser.Forward();
            }
        }

        private void SetGoForwardButtonStatus(bool status)
        {
            InvokeAction(() =>
            {
                btnForward.Visible = btnForward.Enabled = status;
            });
        }
        private void btnHome_Click(object sender, EventArgs e)
        {
            LoadUrl(MyBrowserSettings.HomePage);
        }
   
        private void SetGoHomeButtonStatus(bool status)
        {
            InvokeAction(() =>
            {
                btnHome.Enabled = status;
            });
        }
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            chBrowser.Reload();
        }

        private void SetRefreshButtonStatus(bool status)
        {
            InvokeAction(() =>
            {
                if (status)
                {
                    btnRefresh.IconChar = FontAwesome.Sharp.IconChar.Close;
                }
                else
                {
                    btnRefresh.IconChar = FontAwesome.Sharp.IconChar.Refresh;
                }
            });
        }

        private bool tbUrlEntered = false;

        private void tbUrl_Enter(object sender, EventArgs e)
        {
            if (!tbUrlEntered)
            {
                tbUrl.SelectAll();
                tbUrlEntered = true;
            }

            // Url Auto Complete
            AutoCompleteStringCollection autoCompleteString = new AutoCompleteStringCollection();
            autoCompleteString.Add("www.twitter.com");
            autoCompleteString.Add("www.youtube.com");
            autoCompleteString.Add("www.facebook.com");
            autoCompleteString.Add("www.instagram.com");
            tbUrl.AutoCompleteCustomSource = autoCompleteString;
        }

        private void tbUrl_Click(object sender, EventArgs e)
        {
            if (tbUrlEntered)
            {
                tbUrl.SelectAll();
            }

            tbUrlEntered = false;
        }
        private void tbUrl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string url = tbUrl.Text;
                Uri uriResult;
                bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult);
                if(uriResult == null)
                {
                    LoadUrl("https://www.google.com/search?q=" + tbUrl.Text);
                }
                else
                {
                    LoadUrl(tbUrl.Text);
                }
            }
        }
    }
}
