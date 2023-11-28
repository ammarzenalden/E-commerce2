using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

namespace E2.Firebase
{
    public class MobileMessagingClient
    {
        private readonly FirebaseMessaging messaging;

        public MobileMessagingClient()
        {
            // Check if a Firebase app already exists
            if (FirebaseApp.DefaultInstance == null)
            {
                var app = FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile("C:\\Users\\ammar\\Desktop\\E-commerce\\E2\\Firebase\\srr.json")
                });
            }

            messaging = FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance);
        }

        private Message CreateNotification(string title, string notificationBody, string token)
        {
            return new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Body = notificationBody,
                    Title = title
                }
            };
        }

        public async Task SendNotification(string token, string title, string body)
        {
            await messaging.SendAsync(CreateNotification(title, body, token));
            //do something with result
        }
    }


}
