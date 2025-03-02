namespace Watson.Abstractions;

public interface ICli
{
   Task<int> Run(string[] args);
}