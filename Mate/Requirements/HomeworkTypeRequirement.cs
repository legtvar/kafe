namespace Kafe.Mate.Requirements;

public class HomeworkTypeRequirement(
    string HomeworkType
) : IRequirement
{
    public static string Moniker => "homework-type";
}
