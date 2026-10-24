namespace Monolegal.Web.Servicios;

public interface IEmailSender
{
    Task EnviarAsync(string destinatario, string asunto, string cuerpoHtml, CancellationToken cancellationToken);
}
