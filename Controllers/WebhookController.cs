using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace RetriveLeads.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {

        private readonly string _verifyToken = "TheTokenForThewebhook"; // Replace with your actual verification token
        private readonly string _accessToken = "your_page_access_token";
        private static readonly string ApiUrl = "https://api-mywaitlist.azurewebsites.net/v1/leads"; // Replace with the actual API endpoint
        private static readonly string ApiKey = "afec886a-12b9-46a9-a0b7-cbf8e5545a64";
        private static readonly string LeadRetrievalUrl = "https://facebookapiendpoint.com/leads"; // Replace with actual URL
        // Facebook webhook verification (GET)

        [HttpGet]
        public async Task<IActionResult> Verify() 
        {

           // await CreateLeadAsync();
            // Get the x-original-uri header value
            var originalUri = Request.Headers["x-original-uri"].ToString();

            // Check if the header is present and not empty
            if (string.IsNullOrEmpty(originalUri))
            {
                return BadRequest("Missing x-original-uri header.");
            }

            // Prepend the scheme and host to make it a valid absolute URI
            var scheme = Request.Scheme; // e.g., "http" or "https"
            var host = Request.Host.Value; // e.g., "localhost:5000" or "example.com"

            var fullUri = $"{scheme}://{host}{originalUri}";

            // Parse the query string from the original URI
            var uri = new Uri(fullUri);
            var query = HttpUtility.ParseQueryString(uri.Query);

            // Extract the parameters
            var hub_mode = query["hub.mode"];
            var hub_challenge = query["hub.challenge"];
            var hub_verify_token = query["hub.verify_token"];

            // Validate the incoming parameters
            if (string.IsNullOrEmpty(hub_mode) ||
                string.IsNullOrEmpty(hub_challenge) ||
                string.IsNullOrEmpty(hub_verify_token))
            {
                return BadRequest("Missing required parameters.");
            }

            // Check if the parameters are correct
            if (hub_mode == "subscribe" && hub_verify_token == _verifyToken)
            {
                // Return the exact hub_challenge sent by Facebook
                return Ok(hub_challenge);
            }

            return BadRequest("Invalid verification request.");
        }


        [HttpPost]
        public static async Task CreateLeadAsync()
        {

            // Create the lead object with all the required fields
            var lead = new
            {
                FirstName = "API Test Child FN9",
                LastName = "API Test Child LN9",
                DOB = "", 
                Gender = "", 
                Address = "",
                Suburb = "", 
                State = "", 
                Postcode = "",
                Phone = "", 
                Status = "",
                ServiceType = "LDC",
                CareRequiredDate = "", 
                DaysRequired = new[] { 1, 2, 3 }, 
                Carers = new[]
                {
            new
            {
                Salutation = "", 
                FirstName = "API Test Carer FN9",
                LastName = "API Test Carer LN9",
                Occupation = "",
                Employer = "", 
                HomePhone = "", 
                WorkPhone = "", 
                MobilePhone = "999999999",
                Email = "apitest_waitlist9@xplortechnologies.com"
            }
        },
                Notes = new string[] { }, 
                CreatedDateTime = "", 
                Tags = new string[] { }, 
                ReferralSource = 1
            };

            // Serialize the object to JSON
            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(lead);

            using (HttpClient client = new HttpClient())
            {
                // Set the authorization header
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Console.WriteLine("Sending request to URL: " + ApiUrl);
                Console.WriteLine("Request Body: " + jsonData);

                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

                string responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Lead created successfully!");
                    Console.WriteLine(responseBody);
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    Console.WriteLine(responseBody);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] JObject data)
        {
            await CreateLeadAsync();
            if (data != null)
            {
                var leadgenId = data["entry"]?[0]?["changes"]?[0]?["value"]?["leadgen_id"]?.ToString();

                if (!string.IsNullOrEmpty(leadgenId))
                {
                    // Fetch lead details from the Facebook Graph API using the leadgen_id
                    var leadDetails = await GetLeadDetails(leadgenId);

                    // Process the lead data and create a lead asynchronously
                    // await CreateLeadAsync(leadDetails);
                }
            }

            return Ok();
        }

        // Fetch lead details using the Facebook Graph API
        private async Task<string> GetLeadDetails(string leadgenId)
        {
            var graphApiUrl = $"https://graph.facebook.com/v16.0/{leadgenId}?access_token={_accessToken}";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(graphApiUrl);

            return response; // Return the lead details as a JSON string
        }

        // Modified CreateLeadAsync method to accept lead details

        //private static async Task CreateLeadAsync(string leadDetails)
        //{
        //    Replace this with your API key
        //    string apiKey = "afec886a-12b9-46a9-a0b7-cbf8e5545a64"; // Ensure this is your actual API key

        //    The API endpoint
        //    string url = "https://api-mywaitlist.azurewebsites.net/v1/leads"; // Ensure this is the correct URL

        //    using (HttpClient client = new HttpClient())
        //    {
        //        Set the authorization header
        //        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        //        Prepare the content to send
        //       var content = new StringContent(leadDetails, Encoding.UTF8, "application/json");
        //        HttpResponseMessage response = await client.PostAsync(url, content);

        //        Get the response
        //        string responseBody = await response.Content.ReadAsStringAsync();

        //        if (response.IsSuccessStatusCode)
        //        {
        //            Console.WriteLine("Lead created successfully!");
        //            Console.WriteLine(responseBody);
        //        }
        //        else
        //        {
        //            Console.WriteLine($"Error: {response.StatusCode}");
        //            Console.WriteLine(responseBody);
        //        }
        //    }
        //}
    }
}




    

