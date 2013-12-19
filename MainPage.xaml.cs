using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using System.Net;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HelpVicinity.Resources;
using System.Device.Location;
using Windows.Devices.Geolocation;
using Nokia.Phone.HereLaunchers;


namespace HelpVicinity
{
    public partial class MainPage : PhoneApplicationPage
    {
        string[] stations = new string[5];
        string[] icons = new string[5];
        string[] distance = new string[5];
        string[] near = new string[5];
        string[] href = new string[5];
        string[] position = new string[10];
        string drivelatitude = "";
        string drivelongitude = "";
        string drivename = "";
        public static string lat = "28.650825";
        public static string lon = "77.215805";
        public static string calluri = "";
        int count = 0;
        int countp = 0;
        int fail = 0;
        int imgcount = 0;
        // Constructor
        
        public MainPage()
        {
            InitializeComponent();

            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "cee244be-e863-4d17-8a3b-2a7a0982250e";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "sFo6tgoROBc9UZrH5gWzWg";

            ShellTile appTile = ShellTile.ActiveTiles.First();
            if (appTile != null)
            {
                StandardTileData newTile = new StandardTileData
                {
                    Title = "Vicinity Help",
                    BackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative),
                    //Count = 12,
                    BackTitle = "Reach Help",
                    BackBackgroundImage = new Uri("/Assets/Tiles/FlipCycleTileMediumBack.png", UriKind.Relative),
                    BackContent = "Our App now supports wallpaper."
                };
                appTile.Update(newTile);
            }
            
            
            
            Geolocator locator = new Geolocator();
            ContentPanel.Visibility = Visibility.Collapsed;
            if (locator.LocationStatus == PositionStatus.Disabled)
            {
                PBHelper.Text = "Please Turn On your Location";
                PB.Visibility = Visibility.Collapsed;
                LocationBtn.Visibility = Visibility.Visible;
                getLocImg.Visibility = Visibility.Collapsed;
                getHelpImg.Visibility = Visibility.Collapsed;
            }
            else
            {
                LocationBtn.Visibility = Visibility.Collapsed;
                GetCoordinate();
            }

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void GetCoordinate()
        {
            var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High)
            {
                MovementThreshold = 1
            };
            watcher.PositionChanged += this.watcher_PositionChanged;
            watcher.Start();
        }

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            var pos = e.Position.Location;
            lat = pos.Latitude.ToString("0.000");
            lon=pos.Longitude.ToString("0.000");
            PBHelper.Text = "Location Recieved!! Getting List of Helps";
            getLocImg.Visibility = Visibility.Collapsed;            
            GetFeed(handleFeed,lat,lon);
            
        } 

        // this method downloads the feed without blocking the UI;
        // when finished it calls the given action
        public void GetFeed(Action<string> doSomethingWithFeed, string latitude, string longitude)
        {
            if (imgcount < 1)
            {
                getHelpImg.Visibility = Visibility.Visible; 
                imgcount++;
            }
            

            try
            {
                HttpWebRequest request = HttpWebRequest.CreateHttp("http://vicinityhelp.fawkesindia.com/xml.php?lat=" + latitude + "&lon=" + longitude);
                request.BeginGetResponse(
                    asyncCallback =>
                    {
                        string data = null;
                        try
                        {
                            using (WebResponse response = request.EndGetResponse(asyncCallback))
                            {
                                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                {
                                    data = reader.ReadToEnd();
                                }
                            }
                        }
                        catch (Exception netExp)
                        {
                            fail++;
                            
                        }
                        
                        Deployment.Current.Dispatcher.BeginInvoke(() => doSomethingWithFeed(data));
                    }
                    , null);
            }
            catch(Exception netEx){
                            fail++;
                            
            }
        }

        // this method will be called by GetFeed once the feed has been downloaded
        private void handleFeed(string feedString)
        {
            if (fail<1)
            {

                
                // build XML DOM from feed string
                XDocument doc = XDocument.Parse(feedString);

                // show title of feed in TextBlock
                station.Text = doc.Element("stations").Element("results").Element("items").Element("category").Element("title").Value;
                myloc.Text = doc.Element("stations").Element("search").Element("context").Element("location").Element("address").Element("text").Value;
                // add each feed item to a ListBox
                foreach (var item in doc.Descendants("items"))
                {
                    
                    if (count > 4)
                    {
                        break;
                    }

                    stations[count] = item.Element("title").Value;
                    distance[count] = item.Element("distance").Value;
                    href[count] = item.Element("href").Value;
                    near[count] = item.Element("vicinity").Value;
                    icons[count] = item.Element("icon").Value;
                    position[countp] = item.Element("position").Value;
                    countp++;
                    position[countp] = (item.Element("position").NextNode.ToString().Replace("<position>", "")).Replace("</position>", "");   
                    
                    //feedListBox.Items.Add(item.Element("title").Value);
                    count++;
                    countp++;
                }

                try
                {
                    
                    Station1.Text = stations[0];
                    Station2.Text = stations[1];
                    Station3.Text = stations[2];
                    Station4.Text = stations[3];
                    Station5.Text = stations[4];

                    Near1.Text = near[0];
                    Near2.Text = near[1];
                    Near3.Text = near[2];
                    Near4.Text = near[3];
                    Near5.Text = near[4];

                    Dis1.Text = Convert.ToSingle(distance[0]) / 1000 + " KM";
                    Dis2.Text = Convert.ToSingle(distance[1]) / 1000 + " KM";
                    Dis3.Text = Convert.ToSingle(distance[2]) / 1000 + " KM";
                    Dis4.Text = Convert.ToSingle(distance[3]) / 1000 + " KM";
                    Dis5.Text = Convert.ToSingle(distance[4]) / 1000 + " KM";
                    //Dis5.Text = position[0] + " or " + position[1];

                    Uri uri0 = new Uri(icons[0], UriKind.Absolute);
                    Listicon1.Source = new BitmapImage(uri0);
                    Uri uri1 = new Uri(icons[1], UriKind.Absolute);
                    Listicon2.Source = new BitmapImage(uri1);
                    Uri uri2 = new Uri(icons[2], UriKind.Absolute);
                    Listicon3.Source = new BitmapImage(uri2);
                    Uri uri3 = new Uri(icons[3], UriKind.Absolute);
                    Listicon4.Source = new BitmapImage(uri3);
                    Uri uri4 = new Uri(icons[4], UriKind.Absolute);
                    Listicon5.Source = new BitmapImage(uri4);
                    PB.Visibility = Visibility.Collapsed;
                    PBHelper.Text = "VICINITY HELP APP";
                    getHelpImg.Visibility = Visibility.Collapsed;
                    ContentPanel.Visibility = Visibility.Visible;

                }
                catch (Exception ex)
                {
                    
                }
                // continue here...
            }
            else
            {
                PBHelper.Text = "Internet Connection Unavailable";
                PB.Visibility = Visibility.Collapsed;
                getHelpImg.Visibility = Visibility.Collapsed;
                NetBtn.Visibility = Visibility.Visible;
                
            }
        }

        // user clicks a button -> start feed download
        
        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            ContentPanel.Visibility = Visibility.Collapsed;
            PB.Visibility = Visibility.Visible;
            PBHelper.Visibility = Visibility.Visible;
            getHelpImg.Visibility = Visibility.Visible;
            Random random = new Random();
            PBHelper.Text = "Please wait while help list is reloaded";
            GetFeed(handleFeed, lat, lon + "&reload=" + random);
            imgcount = 0;

        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            MessageBox.Show("Changing your LockScreen!! Press Ok");
            LockHelper("DefaultLockScreen.jpg", true);
            
            //LaunchLockSettings();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            LocationBtn.Visibility = Visibility.Collapsed;
            LaunchLocationSettingsApp();
            
        }

        private async System.Threading.Tasks.Task LaunchLockSettings()
        {

            var op = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-lock:"));

        }


        private async System.Threading.Tasks.Task LaunchLocationSettingsApp()
        {

            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-location:"));

        }

        private void NetBtn_Click_1(object sender, RoutedEventArgs e)
        {
            NetBtn.Visibility = Visibility.Collapsed;
            LaunchNetSettingsApp();          

        }

        private async System.Threading.Tasks.Task LaunchNetSettingsApp()
        {

            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-cellular:"));
           // MessageBox.Show("Focused");
        }

        private void GPSHome_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            GeoCoordinate toGeo = new GeoCoordinate(Double.Parse(lat), Double.Parse(lon));
                
            ExploremapsExplorePlacesTask searchMap = new ExploremapsExplorePlacesTask();

            searchMap.Location = toGeo;
            searchMap.Category.Add("police-emergency");

            searchMap.Show();
        }

        private void grid5_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            calluri = href[4];
            NavigationService.Navigate(new Uri("/CallPage.xaml", UriKind.Relative));
        }

        private void grid4_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            calluri = href[3];
            NavigationService.Navigate(new Uri("/CallPage.xaml", UriKind.Relative));
        }
        private void grid3_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            calluri = href[2];
            NavigationService.Navigate(new Uri("/CallPage.xaml", UriKind.Relative));
        }

        private void grid2_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            calluri = href[1];
            NavigationService.Navigate(new Uri("/CallPage.xaml", UriKind.Relative));
        }

        private void grid1_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
        {
            calluri = href[0];
            NavigationService.Navigate(new Uri("/CallPage.xaml", UriKind.Relative));
        }

        private void grid1_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid1.Background = new SolidColorBrush(Colors.LightGray);
            grid2.Background = new SolidColorBrush(Colors.Transparent);
            grid3.Background = new SolidColorBrush(Colors.Transparent);
            grid4.Background = new SolidColorBrush(Colors.Transparent);
            grid5.Background = new SolidColorBrush(Colors.Transparent);
            drivename = stations[0];
            drivelatitude = position[0];
            drivelongitude = position[1];
        }

        private void grid2_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid2.Background = new SolidColorBrush(Colors.LightGray);
            grid1.Background = new SolidColorBrush(Colors.Transparent);
            grid3.Background = new SolidColorBrush(Colors.Transparent);
            grid4.Background = new SolidColorBrush(Colors.Transparent);
            grid5.Background = new SolidColorBrush(Colors.Transparent);
            drivename = stations[1];
            drivelatitude = position[2];
            drivelongitude = position[3];
        }

        private void grid3_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid3.Background = new SolidColorBrush(Colors.LightGray);
            grid2.Background = new SolidColorBrush(Colors.Transparent);
            grid1.Background = new SolidColorBrush(Colors.Transparent);
            grid4.Background = new SolidColorBrush(Colors.Transparent);
            grid5.Background = new SolidColorBrush(Colors.Transparent);
            drivename = stations[2];
            drivelatitude = position[4];
            drivelongitude = position[5];
        }
        private void grid4_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid4.Background = new SolidColorBrush(Colors.LightGray);
            grid2.Background = new SolidColorBrush(Colors.Transparent);
            grid3.Background = new SolidColorBrush(Colors.Transparent);
            grid1.Background = new SolidColorBrush(Colors.Transparent);
            grid5.Background = new SolidColorBrush(Colors.Transparent);
            drivename = stations[3];
            drivelatitude = position[6];
            drivelongitude = position[7];
        }

        private void grid5_MouseEnter_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            grid5.Background = new SolidColorBrush(Colors.LightGray);
            grid2.Background = new SolidColorBrush(Colors.Transparent);
            grid3.Background = new SolidColorBrush(Colors.Transparent);
            grid4.Background = new SolidColorBrush(Colors.Transparent);
            grid1.Background = new SolidColorBrush(Colors.Transparent);
            drivename = stations[4];
            drivelatitude = position[8];
            drivelongitude = position[9];
        }


        private async void LockHelper(string filePathOfTheImage, bool isAppResource)
        {
            try
            {
                var isProvider = Windows.Phone.System.UserProfile.LockScreenManager.IsProvidedByCurrentApplication;
                if (!isProvider)
                {
                    // If you're not the provider, this call will prompt the user for permission.
                    // Calling RequestAccessAsync from a background agent is not allowed.
                    var op = await Windows.Phone.System.UserProfile.LockScreenManager.RequestAccessAsync();                    
                    // Only do further work if the access was granted.
                    isProvider = op == Windows.Phone.System.UserProfile.LockScreenRequestResult.Granted;                    
                }

                if (isProvider)
                {
                    // At this stage, the app is the active lock screen background provider.

                    // The following code example shows the new URI schema.
                    // ms-appdata points to the root of the local app data folder.
                    // ms-appx points to the Local app install folder, to reference resources bundled in the XAP package.
                    var schema = isAppResource ? "ms-appx:///" : "ms-appdata:///Local/";
                    var uri = new Uri(schema + filePathOfTheImage, UriKind.Absolute);                    
                    // Set the lock screen background image.
                    Windows.Phone.System.UserProfile.LockScreen.SetImageUri(uri);                    
                    // Get the URI of the lock screen background image.
                    var currentImage = Windows.Phone.System.UserProfile.LockScreen.GetImageUri();
                    System.Diagnostics.Debug.WriteLine("The new lock screen background image is set to {0}", currentImage.ToString());
                }
                else
                {
                    MessageBox.Show("You said no, so I can't update your background.");
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }

        private void ApplicationBarIconButton_Click_3(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click_4(object sender, EventArgs e)
        {
            if (drivelatitude == "")
            {
                MessageBox.Show("Please select a place from the list first");
            }
            else
            {
                RequestDirections();
            }
        }

        async void RequestDirections()
        {
            // Get the values required to specify the destination.
            

            // Assemble the Uri to launch.
            Uri uri = new Uri("ms-drive-to:?destination.latitude=" + drivelatitude +
                "&destination.longitude=" + drivelongitude + "&destination.name=" + drivename);
            // The resulting Uri is: "ms-drive-to:?destination.latitude=47.6451413797194
            //  &destination.longitude=-122.141964733601&destination.name=Redmond, WA")

            // Launch the Uri.
            var success = await Windows.System.Launcher.LaunchUriAsync(uri);

            if (success)
            {
                // Uri launched.
            }
            else
            {
                // Uri failed to launch.
            }
        }

    }
   
}