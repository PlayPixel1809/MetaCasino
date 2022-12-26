using System.Net;
using System.Net.Mail;
using UnityEngine;

public class Mail : MonoBehaviour
{
    public static string smtpAddress = "smtp.gmail.com";
    public static int portNumber = 587;
    public static bool enableSSL = true;
    public static string emailFromAddress = "admin@playpixelinteractive.com"; //Sender Email Address  
    public static string password = "admiN@12345"; //Sender Password  
    
    public static void SendEmail(string to, string subject, string body)
    {
        using (MailMessage mail = new MailMessage())
        {
            mail.From = new MailAddress(emailFromAddress);
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;
            //mail.Attachments.Add(new Attachment("D:\\TestFile.txt"));//--Uncomment this to send any attachment  
            using (SmtpClient smtp = new SmtpClient(smtpAddress, portNumber))
            {
                smtp.Credentials = new NetworkCredential(emailFromAddress, password);
                smtp.EnableSsl = enableSSL;
                smtp.Send(mail);
            }
        }
    }
}

