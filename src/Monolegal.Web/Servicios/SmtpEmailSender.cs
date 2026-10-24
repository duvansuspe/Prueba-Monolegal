using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Monolegal.Web.Data;

namespace Monolegal.Web.Servicios;

public class SmtpEmailSender : IEmailSender
{
    private readonly OpcionesSmtp _opciones;

    public SmtpEmailSender(IOptions<OpcionesSmtp> opciones)
    {
        _opciones = opciones.Value;
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken)
    {
        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress(_opciones.FromName, _opciones.FromAddress));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = asunto;
        mensaje.Body = new BodyBuilder { HtmlBody = cuerpoHtml }.ToMessageBody();

        using var cliente = new SmtpClient();
        cliente.Timeout = 15000;
        await cliente.ConnectAsync(_opciones.Host, _opciones.Port, Seguridad(), cancellationToken);

        if (!string.IsNullOrWhiteSpace(_opciones.Username))
        {
            await cliente.AuthenticateAsync(_opciones.Username, _opciones.Password, cancellationToken);
        }

        await cliente.SendAsync(mensaje, cancellationToken);
        await cliente.DisconnectAsync(true, cancellationToken);
    }

    private SecureSocketOptions Seguridad()
    {
        if (!_opciones.UseSsl)
        {
            return SecureSocketOptions.None;
        }

        return _opciones.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTls;
    }
}
