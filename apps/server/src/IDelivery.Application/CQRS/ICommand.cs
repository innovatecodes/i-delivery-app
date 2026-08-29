namespace IDelivery.Application.CQRS;

public interface ICommand { }

public interface ICommand<out TResult> { }

public interface IQuery<out TResult> { }