using System.Net;
using ReactiveUI;
using Rogero.ReactiveSourceGenerator.Attributes2;

namespace Rogero.ReactiveSourceGenerator.TryItOut;

public partial class Class1 : ReactiveObject
{
    [MakeReactiveProperty] private string _firstName;
    [MakeReactiveProperty] private string _lastName8;
    [MakeReactiveProperty] private string _lastName2;

    public Class1() { }
}

public partial class Class2 : ReactiveObject
{
    [MakeReactiveProperty] private int       _something0;
    [MakeReactiveProperty] private int       _something1;
    [MakeReactiveProperty] private int       _something2;
    [MakeReactiveProperty] private int       _something9;
    [MakeReactiveProperty] private IPAddress _something4;

    public Class2()
    {
    }
}