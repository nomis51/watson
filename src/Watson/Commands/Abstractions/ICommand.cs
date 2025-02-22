namespace Watson.Commands.Abstractions;

public interface ICommand<TOptions>
{
   Task<int> Run(TOptions options);
}