using Watson.Core.Repositories.Abstractions;
using Watson.Helpers.Abstractions;
using Watson.Models.Abstractions;

namespace Watson.Models;

public class DependencyResolver : IDependencyResolver
{
    public IProjectRepository ProjectRepository { get; }
    public ITagRepository TagRepository { get; }
    public IFrameRepository FrameRepository { get; }
    public ITimeHelper TimeHelper { get; }
    public IFrameHelper FrameHelper { get; }

    public DependencyResolver(
        IProjectRepository projectRepository,
        IFrameRepository frameRepository,
        ITagRepository tagRepository,
        ITimeHelper timeHelper,
        IFrameHelper frameHelper
    )
    {
        ProjectRepository = projectRepository;
        FrameRepository = frameRepository;
        TagRepository = tagRepository;
        TimeHelper = timeHelper;
        FrameHelper = frameHelper;
    }
}