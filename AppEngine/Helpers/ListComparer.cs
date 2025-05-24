using AppEngine.Types;

namespace AppEngine.Helpers;

public class ListComparer
{
    public record CompareResult<T1, T2>(IEnumerable<T1> OnlyLeft,
                                        IEnumerable<T2> OnlyRight,
                                        IEnumerable<(T1, T2)> Matches);

    public static CompareResult<T1, T2> Compare<T1, T2>(IEnumerable<T1> left,
                                                        IEnumerable<T2> right,
                                                        params Func<T1, T2, bool>[] hasSameIdentity)
        where T1 : class
        where T2 : class
    {
        var leftRemaining = left.AsList();
        var rightRemaining = right.AsList();
        var matches = new List<(T1 Left, T2 Right)>();
        var leftNotMatched = new HashSet<T1>(leftRemaining);
        var rightNotMatched = new HashSet<T2>(rightRemaining);

        foreach (var compare in hasSameIdentity)
        {
            foreach (var rgt in rightRemaining)
            {
                var leftMatch = leftRemaining.FirstOrDefault(lft => compare(lft, rgt));

                if (leftMatch != null)
                {
                    leftNotMatched.Remove(leftMatch);
                    rightNotMatched.Remove(rgt);
                    matches.Add((leftMatch, rgt));
                }
            }

            leftRemaining = leftNotMatched.ToList();
            rightRemaining = rightNotMatched.ToList();
        }

        return new CompareResult<T1, T2>(leftRemaining,
                                         rightRemaining,
                                         matches.Where(mat => mat is { Left: not null, Right: not null })
                                                .Select(mat => (mat.Left!, mat.Right!)));
    }
}