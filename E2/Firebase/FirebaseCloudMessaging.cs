using FirebaseAdmin.Messaging;

namespace E2.Firebase
{
    public class FirebaseCloudMessaging
    {
        public static void SendNotification(string deviceToken, string title, string body)
        {
            var message = new Message()
            {
                Notification = new Notification
                {
                    Title = title,
                    Body = body,
                },
                Token = deviceToken,
            };

            var messaging = FirebaseMessaging.DefaultInstance;
            messaging.SendAsync(message);
        }
    }
}
