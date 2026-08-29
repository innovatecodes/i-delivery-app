using System.Collections.ObjectModel;

namespace IDelivery.Domain.Common.Exceptions
{
    /// <summary>
    /// Exception base para erros de domínio.
    /// Carrega uma coleção de mensagens de erro imutável.
    /// </summary>
    public abstract class BaseException : Exception
    {
        public IReadOnlyCollection<string> Errors { get; }

        protected BaseException(string message) : base(message)
        {
            Errors = new ReadOnlyCollection<string>(new List<string> { message });
        }

        protected BaseException(IEnumerable<string> errors) : base("Um ou mais erros de domínio ocorreram.")
        {
            ArgumentNullException.ThrowIfNull(errors, nameof(errors));
            Errors = new ReadOnlyCollection<string>(errors.ToList());
        }
    }
}
