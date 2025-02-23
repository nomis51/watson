using Watson.Core.Repositories.Abstractions;
using Watson.Models.Abstractions;

namespace Watson.Models;

public class DependencyResolver : IDependencyResolver
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