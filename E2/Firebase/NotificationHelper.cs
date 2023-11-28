//using E2.Models;

//namespace E2.Firebase
//{
//    public class NotificationHelper
//    {
//        public static async Task<string> SendNotificationAsync(string deviceToken, string title, string body)
//        {
//            using (var client = new HttpClient())
//            {
//                var baseUrl = "https://localhost:500/"; // Replace this with your API base URL
//                var apiUrl = "https://localhost:443/api/notification/send"; // Replace this with the route to your SendNotification endpoint
//                client.BaseAddress = new Uri(baseUrl);

//                var notificationModel = new NotificationModel
//                {
//                    DeviceToken = deviceToken,
//                    Title = title,
//                    Body = body,
//                };

//                var response = await client.PostAsJsonAsync(apiUrl, notificationModel);
//                response.EnsureSuccessStatusCode();

//                return await response.Content.ReadAsStringAsync();
//            }
//        }
//    }
//}
