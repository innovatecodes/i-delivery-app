namespace IDelivery.Application.Abstractions.CQRS;

public interface ICommand { }

public interface ICommand<TResult> { }

public interface IQuery<TResult> { }