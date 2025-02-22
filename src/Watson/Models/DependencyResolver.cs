using Watson.Core.Repositories.Abstractions;

namespace Watson.Models;

public class DependencyResolver
{
    public IProjectRepository ProjectRepository { get; }
    public ITagRepository TagRepository { get; }
    public IFrameRepository FrameRepository { get; }

    public DependencyResolver(
        IProjectRepository projectRepository,
        IFrameRepository frameRepository,
        ITagRepository tagRepository
    )
    {
        ProjectRepository = projectRepository;
        FrameRepository = frameRepository;
        TagRepository = tagRepository;
    }
}