using HtmlAgilityPack;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using RabbitHole.Classes;
using RabbitHole.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Web.Http;

namespace RabbitHole
{
    public sealed partial class MainWindow : Window
    {
        RecentViewModel recents;
        bool NeedsErase;

        public MainWindow()
        {
            recents = new RecentViewModel();
            NeedsErase = false;
            this.InitializeComponent();
        }

        private string RandomString(int length)
        {
            while (true)
            {
                Random rand = new Random();
                const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
                var chars = Enumerable.Range(0, length)
                    .Select(x => pool[rand.Next(0, pool.Length)]);
                string str = new string(chars.ToArray());

                if (str[0] != '0')
                {
                    return str;
                }
            }
        }

        private void LoadRandomButton_Click(object sender, RoutedEventArgs e)
        {
            NeedsErase = false;
            MainImage.Visibility = Visibility.Collapsed;
            MainImage.Source = new BitmapImage(new Uri("https://i.imgur.com/xH2ojWU.png"));
            LoadRandomButton.IsEnabled = false;
            Random rand = new Random();
            string RandStr = RandomString(rand.Next(8) + 4);
            string Url = $"https://prnt.sc/{RandStr}";

            UrlTextBox.Text = Url;
            ImageLoadProgress.IsIndeterminate = true;
            Thread t = new Thread(LoadContent);
            t.Start(Url);
        }

        private async void LoadContent(object urlObj)
        {

            string Url = (string)urlObj;
            HttpResponseMessage httpResponse = new HttpResponseMessage();
            var filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter()
            {
                CookieUsageBehavior = Windows.Web.Http.Filters.HttpCookieUsageBehavior.Default
            };
            using (HttpClient httpClient = new HttpClient(filter))
            {
                var headers = httpClient.DefaultRequestHeaders;

                string header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
                if (!headers.UserAgent.TryParseAdd(header))
                {
                    return;
                }

                Uri requestUri = new Uri(Url);

                try
                {
                    httpResponse = await httpClient.GetAsync(requestUri);
                    httpResponse.EnsureSuccessStatusCode();
                }
                catch (Exception)
                {
                    return;
                }
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(httpResponse.Content.ToString());

            var imageNodes = htmlDoc.DocumentNode.SelectNodes("//img");

            foreach (var node in imageNodes)
            {
                if (node.GetAttributeValue("class", "NULL") == "no-click screenshot-image")
                {
                    try
                    {
                        string s = node.GetAttributeValue("src", "NULL");
                        this.DispatcherQueue.TryEnqueue(() =>
                        {
                            try
                            {
                                NeedsErase = true;
                                MainImage.Source = new BitmapImage(new Uri(s));
                                MainImage.Visibility = Visibility.Visible;
                                string[] str = s.Split("/");
                                ImgTextBox.Text = str[str.Length - 1];
                                recents.AddRecent(new Recent() { ImageURL = s, URL = Url });
                            }
                            catch (Exception)
                            {
                                Random rand = new Random();
                                string RandStr = RandomString(rand.Next(10) + 2);
                                string NewUrl = $"https://prnt.sc/{RandStr}";

                                this.DispatcherQueue.TryEnqueue(() =>
                                {
                                    UrlTextBox.Text = NewUrl;
                                });
                                Thread t = new Thread(LoadContent);
                                t.Start(NewUrl);
                            }
                        });
                    }
                    catch (Exception)
                    {
                        Random rand = new Random();
                        string RandStr = RandomString(rand.Next(10) + 2);
                        string NewUrl = $"https://prnt.sc/{RandStr}";

                        this.DispatcherQueue.TryEnqueue(() =>
                        {
                            UrlTextBox.Text = NewUrl;
                        });
                        Thread t = new Thread(LoadContent);
                        t.Start(NewUrl);
                    }
                    return;
                }
            }
            return;
        }

        private void MainImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            if (NeedsErase)
            {
                ImageLoadProgress.IsIndeterminate = false;
                LoadRandomButton.IsEnabled = true;
            }
        }

        private void MainImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ImageLoadProgress.IsIndeterminate = false;
            LoadRandomButton.IsEnabled = true;
        }
    }
}
