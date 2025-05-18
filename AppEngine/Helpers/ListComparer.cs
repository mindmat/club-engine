using AppEngine.Types;

namespace AppEngine.Helpers;

public class ListComparer
{
    public record CompareResult<T1, T2>(IEnumerable<T1> OnlyLeft,
                                        IEnumerable<T2> OnlyRight,
                                        IEnumerable<(T1, T2)> Matches);

    public static CompareResult<T1, T2> Compare<T1, T2>(IEnumerable<T1> left,
                                                        IEnumerable<T2> right,
                                                        Func<T1, T2, bool> hasSameIdentity)
        where T1 : class
        where T2 : class
    {
        var leftList = left.AsList();
        var rightList = right.AsList();
        var matches = new List<(T1? Left, T2? Right)>();
        var leftNotMatched = new HashSet<T1>(leftList);

        foreach (var rgt in rightList)
        {
            var leftMatch = leftList.FirstOrDefault(lft => hasSameIdentity(lft, rgt));

            if (leftMatch != null)
            {
                leftNotMatched.Remove(leftMatch);
            }

            matches.Add((leftMatch, rgt));
        }

        matches.AddRange(leftNotMatched.Select(lft => ((T1?)lft, (T2?)null)));

        var onlyLeft = matches.Where(mat => mat.Right == null)
                              .Select(mat => mat.Left)
                              .WhereNotNull()
                              .ToList();

        var onlyRight = matches.Where(mat => mat.Left == null)
                               .Select(mat => mat.Right)
                               .WhereNotNull()
                               .ToList();

        return new CompareResult<T1, T2>(onlyLeft,
                                         onlyRight,
                                         matches.Where(mat => mat is { Left: not null, Right: not null })
                                                .Select(mat => (mat.Left!, mat.Right!)));
    }
}