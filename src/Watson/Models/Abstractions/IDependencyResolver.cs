using Watson.Core.Repositories.Abstractions;

namespace Watson.Models.Abstractions;

public interface IDependencyResolver
{
    IProjectRepository ProjectRepository { get; }
    ITagRepository TagRepository { get; }
    IFrameRepository FrameRepository { get; }
}