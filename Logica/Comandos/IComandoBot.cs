using Entidades;

namespace Logica.Comandos
{
    /// <summary>
    /// Un comando que el bot sabe atender. Para agregar uno nuevo se crea una clase que
    /// implemente este contrato y se registra en RestauranteFabrica; nada más cambia (OCP).
    /// Contrato (LSP): Atiende no tiene efectos secundarios y Ejecutar siempre devuelve un texto.
    /// </summary>
    public interface IComandoBot
    {
        bool Atiende(string texto);
        string Ejecutar(MensajeEntrante mensaje);
    }
}
