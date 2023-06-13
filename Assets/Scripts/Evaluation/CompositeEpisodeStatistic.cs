
using System.Collections.Generic;

public class CompositeEpisodeStatistic
{
    public int compositeEpisodeNumber = -1;
    public bool globalSuccess = false;
    public int steps = -1;
    public int postConditionReachedCount = 0;
    public int accViolatedCount = 0;
    public int localResetCount = 0;
    public Dictionary<string, ActionStatistic> actionStatistics = new Dictionary<string, ActionStatistic>();

    public CompositeEpisodeStatistic(IEnumerable<BTTest.LearningActionAgentSwitcher> actions)
    {
        foreach (BTTest.LearningActionAgentSwitcher action in actions)
        {
            actionStatistics.Add(action.Name, new ActionStatistic(action));
        }
    }
}

public class ActionStatistic
{
    public string actionName;
    public int episodeCount = 0;
    public List<EpisodeStatistic> episodes = new List<EpisodeStatistic>();
    public int postConditionReachedCount = 0;
    public int accViolatedCount = 0;
    public int localResetCount = 0;
    public Dictionary<string, ACCViolatedStatistic> accViolatedStatistics = new Dictionary<string, ACCViolatedStatistic>();

    public ActionStatistic(BTTest.LearningActionAgentSwitcher action)
    {
        actionName = action.Name;
        if (action.accs != null)
        {
            foreach (var acc in action.accs)
            {
                accViolatedStatistics.Add(acc.Name, new ACCViolatedStatistic { accName = acc.Name });
            }
        }
    }
}

public class EpisodeStatistic
{
    public int steps = 0;
    public float reward = 0;
    public ActionTerminationCause cause;
    public ACCViolatedInfo accInfo = null;
}

public class ACCViolatedInfo
{
    public string accName;
    public int stepsToRecover;
    public bool recovered;
}

public class ACCViolatedStatistic
{
    public string accName;
    public int count = 0;
    public List<int> stepsToRecover = new List<int>();
    public List<bool> recovered = new List<bool>();
}
