using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;

namespace Contoso.Extensions.Data
{
    public interface IEmailConnection
    {
        void SendEmail(string subject, Exception exception, Action<MailMessage> func = null, string toEmail = null, string fromEmail = null);
        void SendEmail(string subject, string message, Action<MailMessage> func = null, string toEmail = null, string fromEmail = null);
    }

    public class EmailConnection : IEmailConnection
    {
        public EmailConnection(string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;
            string serviceCredential = null, serviceLogin = null, servicePassword = null;
            foreach (var param in connectionString.Split(';'))
            {
                if (string.IsNullOrEmpty(param)) continue;
                var kv = param.Split(new[] { '=' }, 2);
                if (kv.Length > 1 && string.Equals(kv[0], "Credential", StringComparison.OrdinalIgnoreCase)) serviceCredential = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "User Id", StringComparison.OrdinalIgnoreCase)) serviceLogin = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "Password", StringComparison.OrdinalIgnoreCase)) servicePassword = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "Server", StringComparison.OrdinalIgnoreCase)) Server = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "PickupDirectory", StringComparison.OrdinalIgnoreCase)) PickupDirectory = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "ToEmail", StringComparison.OrdinalIgnoreCase)) ToEmail = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "FromEmail", StringComparison.OrdinalIgnoreCase)) FromEmail = kv[1];
                else if (kv.Length > 1 && string.Equals(kv[0], "Subject", StringComparison.OrdinalIgnoreCase)) Subject = kv[1];
                else Params.Add(kv[0].ToLowerInvariant(), kv.Length > 1 ? kv[1] : null);
            }
            if (string.IsNullOrEmpty(serviceCredential)) Credential = new NetworkCredential { UserName = serviceLogin, Password = servicePassword };
            else if (CredentialManager.TryRead(serviceCredential, CredentialManager.CredentialType.GENERIC, out var credential) != 0)
                throw new InvalidOperationException("Unable to read credential store");
            else Credential = new NetworkCredential { UserName = credential.UserName, Password = credential.CredentialBlob };
        }

        /// <summary>
        /// Gets from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        public string FromEmail { get; }

        /// <summary>
        /// Converts to email.
        /// </summary>
        /// <value>
        /// To email.
        /// </value>
        public string ToEmail { get; }

        /// <summary>
        /// Gets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; }

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public string Server { get; }

        /// <summary>
        /// Gets the pickup directory.
        /// </summary>
        /// <value>
        /// The pickup directory.
        /// </value>
        public string PickupDirectory { get; }

        /// <summary>
        /// Gets the credential.
        /// </summary>
        /// <value>
        /// The credential.
        /// </value>
        public NetworkCredential Credential { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public Dictionary<string, string> Params { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="subject">The title.</param>
        /// <param name="exception">The ex.</param>
        /// <param name="func">The function.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="fromEmail">From email.</param>
        public void SendEmail(string subject, Exception exception, Action<MailMessage> func = null, string toEmail = null, string fromEmail = null) => SendEmail(subject, BuildExceptionMessage(exception), func);

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="subject">The title.</param>
        /// <param name="message">The message.</param>
        /// <param name="func">The function.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="fromEmail">From email.</param>
        public void SendEmail(string subject, string message, Action<MailMessage> func = null, string toEmail = null, string fromEmail = null)
        {
            using (var client = new SmtpClient())
            {
                if (!string.IsNullOrEmpty(Server))
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Host = Server;
                    if (Params.TryGetValue("Host", out var z)) client.Port = int.Parse(z);
                    if (Params.TryGetValue("Ssl", out _)) client.EnableSsl = true;
                    if (Params.TryGetValue("DefaultCredentials", out _)) client.UseDefaultCredentials = true;
                    if (!string.IsNullOrEmpty(Credential.UserName) || !string.IsNullOrEmpty(Credential.Password))
                        client.Credentials = Credential;
                }
                else if (!string.IsNullOrEmpty(PickupDirectory))
                {
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = PickupDirectory;
                }
                else client.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;
                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail ?? FromEmail),
                    Subject = !string.IsNullOrEmpty(Subject) ? string.Format(Subject, subject) : subject,
                    Body = message,
                };
                func?.Invoke(mail);
                foreach (var to in (toEmail ?? ToEmail).Split(';'))
                    mail.To.Add(new MailAddress(to));
                client.Send(mail);
            }
        }

        /// <summary>
        /// Builds the exception message.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public string BuildExceptionMessage(Exception e)
        {
            var b = new StringBuilder();
            SerializeException(b, e);
            return b.ToString();
        }

        protected virtual void SerializeException(StringBuilder b, Exception e)
        {
            b.Append(@"
Exception
=========");
            if (e == null)
                b.Append(@"
NULL
");
            else
                do
                {
                    SerializeAnException(b, e);
                    e = e.InnerException;
                    if (e != null)
                        b.Append(@"
Inner Exception
===============");
                } while (e != null);
        }

        static void SerializeAnException(StringBuilder b, Exception baseException)
        {
            b.AppendFormat(@"
MESSAGE
[{0}]

SOURCE
[{1}]

STACK TRACE
{2}
", baseException.Message, baseException.Source, baseException.StackTrace);
        }
    }
}
