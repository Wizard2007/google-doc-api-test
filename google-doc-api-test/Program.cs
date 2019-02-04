using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Apis.Json;

namespace google_api_test
{
    class Program
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/calendar-dotnet-quickstart.json

        static string[] OAuthScopes = { CalendarService.Scope.Calendar, CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "Google Calendar API .NET Quickstart";

        private static ServiceAccountCredential getServiceAccountCredential(String pathToJsonFile, String emailToImpersonate)
        {
            // Load and deserialize credential parameters from the specified JSON file.
            JsonCredentialParameters parameters;

            using (Stream json = new FileStream(pathToJsonFile, FileMode.Open, FileAccess.Read))
            {
                parameters = NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(json);
            }

            // Create a credential initializer with the correct scopes.
            ServiceAccountCredential.Initializer initializer = new ServiceAccountCredential.Initializer(parameters.ClientEmail){ Scopes = OAuthScopes };

            // Configure impersonation (if applicable).
            if (!String.IsNullOrEmpty(emailToImpersonate))
            {
                initializer.User = emailToImpersonate;
            }

            // Create a service account credential object using the deserialized private key.
            ServiceAccountCredential credential = new ServiceAccountCredential(initializer.FromPrivateKey(parameters.PrivateKey));

            return credential;
        }

        static void Main(string[] args)
        {
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/calendar-dotnet-quickstart.json");
                
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    OAuthScopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
            /*
            ServiceAccountCredential credential1 =
                  getServiceAccountCredential("client_secret.json", "");
                  */
            // Create Google Calendar API service.
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            }); 

            IList<CalendarListEntry> l = service.CalendarList.List().Execute().Items;
            CalendarsResource l1 = service.Calendars;
            // Define parameters of request.     
       
            Acl acl = service.Acl.List("g9ievp23dnjtb3scugc6c6hosc@group.calendar.google.com").Execute();

            foreach (AclRule rule in acl.Items)
            {
                Console.WriteLine(rule.Id);
            }

            EventsResource.ListRequest request = service.Events.List("g9ievp23dnjtb3scugc6c6hosc @group.calendar.google.com");
            request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 100;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // List events.
            Events events = request.Execute();
            Console.WriteLine("Upcoming events:");

            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();

                    if (String.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }

                    Console.WriteLine("{0} ({1})", eventItem.Summary, when);
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }

            Console.Read();
        }        
    }
}