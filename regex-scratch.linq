<Query Kind="Statements" />

//var pathParser = new Regex(@"^(.+?)(?: \((\d+)\))?(\.[^.]+)?$");
var pathParser =
    new Regex(@"^(?<prefix>.+?)(?: \((?<number>\d+)\))?(?<suffix>\.[^.]+)?$");

string Increment(string path)
{
    var match = pathParser.Match(path);

    if (!match.Success) {
        throw new InvalidOperationException(
                "Can't increment path: " + path);
    }

    var number = match.Groups["number"].Success
                    ? int.Parse(match.Groups["number"].Value)
                    : 1;

    return $"{match.Groups["prefix"]} ({number + 1}){match.Groups["suffix"]}";
}

void Test(string path) => Increment(path).Dump(path);

Test("foo (3).txt");
Test("bar.txt");
Test("baz (2).txt");
Test("quux");
