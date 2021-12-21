using ReactiveUI;

namespace Rogero.ReactiveSourceGenerator.NugetTest;

public partial class Class1 : ReactiveObject
{
    [MakeReactiveProperty] private string _name;
    [MakeReactiveProperty] private string _city;
    [MakeReactiveProperty] private string _state;

    public Class1()
    {

    }
}

internal class MakeReactivePropertyAttribute : Attribute { }