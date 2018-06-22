using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetSharp;
using Newtonsoft;
using System.Web.Script.Serialization;
using System.Threading;
using Newtonsoft.Json;

namespace TwitterBot
{

    class Program
    {

        // link to the excel spread sheet https://docs.google.com/spreadsheets/d/1mk66HSAO86pKciep4urbczyJA0oNFoNgY59AqqBE4lo/edit

        // properties for twitter
        public static string _ConsumerKey = "5Yf62h6RdXurOYhHd6O71vzcf";
        public static string _ConsumerSecret = "AfaLUbpS5wHZ6ayz4LZuh6XDTO2OLeBtAz1QLKfxvMHvAXDW9P";
        public static string _AccessToken = "1496800286-U1B03zCbjFi0ANHKPEmFsyMJo7zfWmgf9GLzHdO";
        public static string _AccessTokenSecret = "NbrC8v6xX99vUSt5rVoMzLmv1If0u02h7oUJaeC9ifNDn";
        public static string txtHashTag = "";

        //properties for google API
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        static string ApplicationName = "Google Sheets API";
        static string SheetId = "1mk66HSAO86pKciep4urbczyJA0oNFoNgY59AqqBE4lo";

        


        static void Main(string[] args)
        {
            
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\t\tWelcome to Twitter Bot...");
            Console.ResetColor();

            

            while(true)
            {
                Console.BackgroundColor = ConsoleColor.White;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine("\nEnter a twitter hashtag: \n");

                string input = Console.ReadLine();
                Console.ResetColor();
                IList<IList<Object>> objNeRecords = null;
                List<User> twitterObj = fetchTwitterDetails(input);
                var service = authenticateGoogleApp();
                string newRange = getRange(service);

                foreach (User u in twitterObj)
                {
                    objNeRecords = generateData(u);
                }

                // update the google sheet
                updatGoogleSheetinBatch(objNeRecords, SheetId, newRange, service);

                Console.WriteLine("\n\nGoogle Spread sheet has been updated. ");
            }

            

            
            Console.ReadLine();
        }

        public static List<User> fetchTwitterDetails(string myHashTag)
        {
            txtHashTag = myHashTag;

            //List<IList<Object>> twitterResult = new List<IList<object>>();
            //List<Object> twitterObject = new List<object>();
            var userResult = new List<User>();

            TweetSharp.TwitterService twService = new TweetSharp.TwitterService(_ConsumerKey, _ConsumerSecret);
            twService.AuthenticateWith(_AccessToken, _AccessTokenSecret);

            var twSearch = twService.Search(new TweetSharp.SearchOptions
            {
                Q = txtHashTag.Trim(),
                Resulttype = TweetSharp.TwitterSearchResultType.Recent,
                Count = 1
            });

            if (twSearch == null)
            {
                Console.WriteLine("Something is wrong...could not fetch details!");
                
            }

            else
            {

                List<TwitterStatus> tweetList = new List<TwitterStatus>(twSearch.Statuses);
                var user = new User();

                //twitterStats = new TwitterStatus();
                //var list = twSearch.Statuses.ToList();

                foreach (TwitterStatus t in tweetList)
                {
                    Console.WriteLine("Profile name: " + t.Author.ScreenName + ", Number of followers: " + t.User.FollowersCount);
                    //add details to object

                    user.screenName = t.Author.ScreenName;
                    user.followersCount = t.User.FollowersCount;
                }

                userResult.Add(user);
            }
            return userResult;
        }

        private static SheetsService authenticateGoogleApp()
        {
            UserCredential credential;


            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None).Result; //,
                    //new FileDataStore(credPath, true)).Result;
               // Console.WriteLine("Credential file saved to: " + credPath);
                Console.WriteLine("");

            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            return service;

        }

        protected static string getRange(SheetsService service)
        {
            // Define request parameters.
            String spreadsheetId = SheetId;
            String range = "A:A";

            SpreadsheetsResource.ValuesResource.GetRequest getRequest =
                       service.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange getResponse = getRequest.Execute();
            IList<IList<Object>> getValues = getResponse.Values;

            int currentCount =  getValues.Count() + 1;

            
            String newRange = "A" + currentCount + ":A";

            return newRange;
        }

        private static IList<IList<Object>> generateData(User user)
        {
            List<IList<Object>> objNewRecords = new List<IList<Object>>();

            IList<Object> obj = new List<Object>();

            obj.Add(user.screenName);
            obj.Add(user.followersCount);

            objNewRecords.Add(obj);

            return objNewRecords;
        }

        private static void updatGoogleSheetinBatch(IList<IList<Object>> values, string spreadsheetId, string newRange, SheetsService service)
        {
          
            SpreadsheetsResource.ValuesResource.AppendRequest request =
            service.Spreadsheets.Values.Append(new ValueRange() { Values = values }, spreadsheetId, newRange);
            request.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var response = request.Execute();
         
        }

        
    }

    class User
    {
        public string screenName { get; set; }
        public int followersCount { get; set; }
    }

   
}
