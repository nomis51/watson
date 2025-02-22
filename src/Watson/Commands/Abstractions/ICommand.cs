namespace Watson.Commands.Abstractions;

public interface ICommand<in TOptions>
{
    Task<int> Run(TOptions options);
}