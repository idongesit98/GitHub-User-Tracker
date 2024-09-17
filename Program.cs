// See https://aka.ms/new-console-template for more information
using System.Text.Json;
using GitHub;


if (args.Length == 0)
{
    Console.WriteLine("Please provide a Github username");
    return;
}

ApiService apiService = new ApiService();

if (!ApiService.ValidateArguments(args))
{
    //ApiService.ShowAllAvailableCommand();
    return;
};


string username = args[0];
string command = args.Length > 1 ? args[1].ToLower() : "events";
string url = "";

switch (command)
{
    case "events":
        url = $"https://api.github.com/users/{username}/events";
        break;

    case "repos":
        url = $"https://api.github.com/users/{username}/received_events/public";
        break;

    case "followers":
        url = $"https://api.github.com/users/{username}/followers";
        break;   

    case "help":
        ApiService.ShowAllAvailableCommand();
        break;

    default:
      break;       
}

await ApiService.HandlePagination(apiService,url,username);