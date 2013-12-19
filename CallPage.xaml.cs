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
using Microsoft.Phone.Maps.Controls;
using Microsoft.Phone.Maps;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using Nokia.Phone.HereLaunchers;
namespace HelpVicinity
{
    public partial class CallPage : PhoneApplicationPage
    {

        string[] position = new string[10];
        string getlat = MainPage.lat;
        string getlon = MainPage.lon;
        string callurinew = MainPage.calluri;
        string phoneNumber = "";
        double latitudeget=0;
        double longitudeget = 0;

        int fail = 0;
        // Constructor

        public CallPage()
        {
            InitializeComponent();            
            ContentPanel.Visibility = Visibility.Collapsed;
            GetFeed(handleFeed);

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        

        // this method downloads the feed without blocking the UI;
        // when finished it calls the given action
        public void GetFeed(Action<string> doSomethingWithFeed)
        {
            try
            {

                HttpWebRequest request = HttpWebRequest.CreateHttp("http://vicinityhelp.fawkesindia.com/callxml.php?uri=" + callurinew);
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
            catch (Exception netEx)
            {
                fail++;

            }
        }

        private void myMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.ApplicationId = "cee244be-e863-4d17-8a3b-2a7a0982250e";
            Microsoft.Phone.Maps.MapsSettings.ApplicationContext.AuthenticationToken = "sFo6tgoROBc9UZrH5gWzWg";
        }

        // this method will be called by GetFeed once the feed has been downloaded
        private void handleFeed(string feedString)
        {
            if (fail < 1)
            {

                // build XML DOM from feed string
                XDocument doc = XDocument.Parse(feedString);

                // show title of feed in TextBlock
                //station.Text = doc.Element("stations").Element("results").Element("items").Element("category").Element("title").Value;

                
                
                try
                {
                    Title.Text = doc.Element("call").Element("name").Value.Replace("Police Station", "").Replace("-", "");
                    T2.Text = doc.Element("call").Element("location").Element("address").Element("city").Value;
                    T3.Text = doc.Element("call").Element("location").Element("address").Element("state").Value + ", India ";
                    phoneNumber = doc.Element("call").Element("contacts").Element("phone").Element("value").Value;
                    T4.Text = phoneNumber;
                    position[0] = doc.Element("call").Element("location").Element("position").Value;
                    position[1] = (doc.Element("call").Element("location").Element("position").NextNode.ToString().Replace("<position>", "")).Replace("</position>", "");

                    PB.Visibility = Visibility.Collapsed;
                    PBHelper.Text = "VICINITY HELP APP";
                    ContentPanel.Visibility = Visibility.Visible;

                    latitudeget = Convert.ToDouble(position[0]);
                    longitudeget = Convert.ToDouble(position[1]);
                    myMap.Center = new GeoCoordinate(latitudeget, longitudeget);
                    myMap.ZoomLevel = 14;
                    myMap.LandmarksEnabled = true;
                    myMap.PedestrianFeaturesEnabled = true;
                    DrawMapMarkers(latitudeget, longitudeget);
                    

                }
                catch (Exception ex)
                {

                }
                // continue here...
            }
            else
            {
                PBHelper.Text = "There is some problem with your Internet Connection";
                PB.Visibility = Visibility.Collapsed;

            }
        }

        // user clicks a button -> start feed download

        private void DrawMapMarkers(double latt, double longg)
        {
        myMap.Layers.Clear();
        MapLayer mapLayer = new MapLayer();
        GeoCoordinate MyCoordinate = new GeoCoordinate(latt, longg);
        // Draw marker for current position
        if (MyCoordinate != null)
        {
            
            DrawMapMarker(MyCoordinate, Colors.Red, mapLayer);
        }
                
         
       myMap.Layers.Add(mapLayer);
        }
        
        
        private void DrawMapMarker(GeoCoordinate coordinate, Color color, MapLayer mapLayer)
        {
        // Create a map marker
        Polygon polygon = new Polygon();
        polygon.Points.Add(new Point(0, 0));
        polygon.Points.Add(new Point(0, 75));
        polygon.Points.Add(new Point(25, 0));
        polygon.Fill = new SolidColorBrush(color);
            

        // Create a MapOverlay and add marker
        MapOverlay overlay = new MapOverlay();
        overlay.Content = polygon;
        overlay.GeoCoordinate = new GeoCoordinate(coordinate.Latitude, coordinate.Longitude);
        overlay.PositionOrigin = new Point(0.0, 1.0);
        mapLayer.Add(overlay);
    }
        
        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            ContentPanel.Visibility = Visibility.Collapsed;
            PB.Visibility = Visibility.Visible;
            PBHelper.Text = "Reloading the page with Details";
            GetFeed(handleFeed);

        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void ApplicationBarIconButton_Click_3(object sender, EventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void CallBTN_Click(object sender, RoutedEventArgs e)
        {
            if (phoneNumber == "")
            {
                phoneNumber = "100";  
            }
            
            PhoneCallTask phoneCallTask = new PhoneCallTask();
            phoneCallTask.PhoneNumber = phoneNumber;
            phoneCallTask.DisplayName = Title.Text +" - Police";
            phoneCallTask.Show();
        }

        private void DriveBTN_Click(object sender, RoutedEventArgs e)
        {
            GuidanceDriveTask driveTo = new GuidanceDriveTask();

            driveTo.Destination = new GeoCoordinate(latitudeget, longitudeget);
            driveTo.Title = "Drive to " + Title.Text;
            driveTo.Show();
        }

        private void WalkBTN_Click(object sender, RoutedEventArgs e)
        {
            GuidanceWalkTask walkTo = new GuidanceWalkTask();

            walkTo.Destination = new GeoCoordinate(latitudeget,longitudeget);
            walkTo.Title = "Walk to "+Title.Text;
            walkTo.Show();
        }

        private void ShareBTN_Click(object sender, RoutedEventArgs e)
        {
            ShareStatusTask shareStatusTask = new ShareStatusTask();

            shareStatusTask.Status = "SOS!! I Need Help!!! Location: "+getlat+","+getlon;

            shareStatusTask.Show();
        }

    }
}