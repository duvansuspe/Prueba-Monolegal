namespace Monolegal.Web.Data;

public class OpcionesSmtp
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 1025;
    public bool UseSsl { get; set; }
    public string FromName { get; set; } = "Monolegal";
    public string FromAddress { get; set; } = "recordatorios@monolegal.test";
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
