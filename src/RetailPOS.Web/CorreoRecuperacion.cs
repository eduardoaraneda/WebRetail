using System.Net;
using System.Net.Mail;
namespace RetailPOS.Web;

public sealed class CorreoRecuperacion(IConfiguration config, IWebHostEnvironment env)
{
    public async Task EnviarAsync(string destinatario, string enlace)
    {
        using var mensaje = new MailMessage(config["Correo:Remitente"] ?? "no-reply@retail.invalid", destinatario)
        { Subject = "Recuperación de acceso · Punto de Venta Retail", Body = "Para cambiar tu contraseña abre este enlace de un solo uso:\n" + enlace + "\nSi no lo solicitaste, ignora este mensaje." };
        using var smtp = new SmtpClient();
        if (env.IsDevelopment() && config["Correo:Modo"] == "Archivo")
        {
            var directorio = Path.GetFullPath(config["Correo:Directorio"] ?? Path.Combine(env.ContentRootPath, "../../.local/correos"));
            Directory.CreateDirectory(directorio);
            smtp.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory; smtp.PickupDirectoryLocation = directorio;
        }
        else
        {
            if (config["Correo:Modo"] != "Smtp" || string.IsNullOrWhiteSpace(config["Correo:Servidor"])) throw new InvalidOperationException("Configura el envío SMTP.");
            smtp.Host = config["Correo:Servidor"]!; smtp.Port = config.GetValue("Correo:Puerto", 587); smtp.EnableSsl = true;
            smtp.Credentials = new NetworkCredential(config["Correo:Usuario"], config["Correo:Password"]);
        }
        await smtp.SendMailAsync(mensaje);
    }
}

