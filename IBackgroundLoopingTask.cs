using TheDialgaTeam.mscorlib.System.Threading.Tasks;

namespace TheDialgaTeam.Worktips.Discord.Bot
{
    public interface IBackgroundLoopingTask
    {
        BackgroundLoopingTask RunningTask { get; }
    }
}