using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;

namespace Contoso.Extensions.Data
{
    /// <summary>
    /// IEmailConnection
    /// </summary>
    public interface IEmailConnection
    {
        /// <summary>
        /// Gets the from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        string FromEmail { get; }

        /// <summary>
        /// Gets the to email.
        /// </summary>
        /// <value>
        /// To email.
        /// </value>
        string ToEmail { get; }

        /// <summary>
        /// Gets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        string Subject { get; }

        /// <summary>
        /// Gets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        string Server { get; }

        /// <summary>
        /// Gets the pickup directory.
        /// </summary>
        /// <value>
        /// The pickup directory.
        /// </value>
        string PickupDirectory { get; }

        /// <summary>
        /// Gets the credential.
        /// </summary>
        /// <value>
        /// The credential.
        /// </value>
        NetworkCredential Credential { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        IDictionary<string, string> Params { get; }

        /// <summary>
        /// Builds the message for an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        string BuildMessageForException(Exception exception);

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="func">The function.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="fromEmail">From email.</param>
        void SendEmail(string subject, Exception exception, Action<MailMessage> func = null, string toEmail = null, string fromEmail = null);
        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="func">The function.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="fromEmail">From email.</param>
        void SendEmail(string subject, string message, Action<MailMessage> func = null, string toEmail = null, string fromEmail = null);
    }

    /// <summary>
    /// EmailConnection
    /// </summary>
    /// <seealso cref="Contoso.Extensions.Data.IEmailConnection" />
    public class EmailConnection : IEmailConnection
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EmailConnection"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <exception cref="InvalidOperationException">Unable to read credential store</exception>
        public EmailConnection(string connectionString = null)
        {
            if (string.IsNullOrEmpty(connectionString))
                return;
            string serviceCredential = null, serviceLogin = null, servicePassword = null;
            foreach (var param in connectionString.Split(';'))
            {
                if (string.IsNullOrEmpty(param)) continue;
                var kv = param.Split(new[] { '=' }, 2);
                var key = kv[0]?.Replace(" ", "").ToLowerInvariant();
                if (kv.Length > 1 && key == "credential") serviceCredential = kv[1];
                else if (kv.Length > 1 && (key == "userid" || key == "uid")) serviceLogin = kv[1];
                else if (kv.Length > 1 && (key == "password" || key == "pwd")) servicePassword = kv[1];
                else if (kv.Length > 1 && (key == "server" || key == "datasource")) Server = kv[1];
                else if (kv.Length > 1 && key == "pickupdirectory") PickupDirectory = kv[1];
                else if (kv.Length > 1 && key == "toemail") ToEmail = kv[1];
                else if (kv.Length > 1 && key == "fromemail") FromEmail = kv[1];
                else if (kv.Length > 1 && key == "subject") Subject = kv[1];
                else Params.Add(key, kv.Length > 1 ? kv[1] : null);
            }
            Credential = string.IsNullOrEmpty(serviceCredential) ? new NetworkCredential { UserName = serviceLogin, Password = servicePassword } :
                CredentialManager.TryRead(serviceCredential, CredentialManager.CredentialType.GENERIC, out var cred) != 0 ? throw new InvalidOperationException("Unable to read credential store") :
                new NetworkCredential { UserName = cred.UserName, Password = cred.CredentialBlob };
        }

        /// <summary>
        /// Gets the from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        public string FromEmail { get; }

        /// <summary>
        /// Get the to email.
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
        public IDictionary<string, string> Params { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Builds the message for an exception.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public string BuildMessageForException(Exception exception)
        {
            var b = new StringBuilder();
            SerializeException(b, exception);
            return b.ToString();
        }

        /// <summary>
        /// Sends the email.
        /// </summary>
        /// <param name="subject">The title.</param>
        /// <param name="exception">The ex.</param>
        /// <param name="func">The function.</param>
        /// <param name="toEmail">To email.</param>
        /// <param name="fromEmail">From email.</param>
        public void SendEmail(string subject, Exception exception, Action<MailMessage> func = null, string toEmail = null, string fromEmail = null) => SendEmail(subject, BuildMessageForException(exception), func);

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
                    if (Params.TryGetValue("UseDefaultCredentials", out _)) client.UseDefaultCredentials = true;
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
