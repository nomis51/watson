using Watson.Core.Repositories.Abstractions;

namespace Watson.Models;

public class DependencyResolver
{
    public IProjectRepository ProjectRepository { get; }
    public IFrameRepository FrameRepository { get; }

    public DependencyResolver(IProjectRepository projectRepository, IFrameRepository frameRepository)
    {
        ProjectRepository = projectRepository;
        FrameRepository = frameRepository;
    }
}