namespace Monolegal.Web.Servicios;

public class ClienteNoEncontradoException : Exception
{
    public ClienteNoEncontradoException(string clienteId)
        : base($"No existe un cliente con el identificador {clienteId}.")
    {
        ClienteId = clienteId;
    }

    public string ClienteId { get; }
}
