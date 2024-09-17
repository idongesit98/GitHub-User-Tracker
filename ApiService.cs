using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GitHub
{
    public class ApiService
    {

        //Validate the arguments
        public static bool ValidateArguments(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Error: Please provide a Github username");
                return false;
            }

            if (args.Length < 2 || (args[1].ToLower() != "events" && args[1].ToLower() != "repos" && args[1].ToLower() != "followers"))
            {
                Console.WriteLine("Error: Please provide a valid command (events, repos, or followers).");
                ShowAllAvailableCommand();
                return false;
            }
            return true;
        }

        /*
        *This code handle the Http request and paginates it, if it returns an error
        *The error is thrown and shown in the cli for attention
        */
        public async Task HttpRequestAsync(string url, string username, int page = 1,int perPage = 10)
        {
            HttpClient client = new HttpClient();
            //Github requires a User-agent header
            client.DefaultRequestHeaders.UserAgent.ParseAdd("MyGitHubActivityCLI");
            try
            {
                //Add pagination to the url
                string paginatedUrl = $"{url}?page={page}&per_page={perPage}";

                HttpResponseMessage response = await client.GetAsync(paginatedUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Error: Unable to fetch data for user '{username}'. Status code: {response.StatusCode}");
                    return;
                }
                await ProcessResponse(response,username,page,perPage);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Network error: {e.Message}");
            }
            catch (JsonException e)
            {
                Console.WriteLine($"Error parsing the response: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected error: {e.Message}");
            }

        }

        /*
        *This prints the response from the url in a json format if the events is not null 
        */

        private async Task ProcessResponse(HttpResponseMessage response, string username, int page, int perPage)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            var events = JsonSerializer.Deserialize<object[]>(responseBody);

            if(events == null || events.Length == 0)
            {
                Console.WriteLine($"No data found for user '{username}' on page {page}");
                return;
            }

            Console.WriteLine($"\nPage {page} - Recent activity for GitHub user '{username}':");
            Console.WriteLine(new string('-', 50));  // Divider for better readability

            foreach (var activity in events)
            {
                // Customize this output based on the GitHub API data structure
                Console.WriteLine(JsonSerializer.Serialize(activity, new JsonSerializerOptions { WriteIndented = true }));
            }

            Console.WriteLine(new string('-', 50));
        }

        /*
        *This handles pagination, prompting the user if they need a next page.
        */
         public static async Task HandlePagination(ApiService apiService, string url, string username)
        {
            int page = 1;
            int perPage = 5;
            bool continueFetching = true;

            while(continueFetching)
            {
                await apiService.HttpRequestAsync(url,username,page,perPage);

                //Prompt if the user needs the next page
                Console.WriteLine("\nDo you want to fetch the next page? (y/n)");
                string input = Console.ReadLine().ToLower();

                if (input == "y")
                {
                    page++;
                }
                else
                {
                    continueFetching = false;
                }
            }
        }

        /*
        *This shows all available commands
        */
        public static void ShowAllAvailableCommand()
        {
            Console.WriteLine("\nAvailable commands");
            Console.WriteLine("1. events     -  shows recent events of the user");
            Console.WriteLine("2. public      -  List of public events by the user");
            Console.WriteLine("3. received   -  List events received by authenticated user");
        }

    }
}