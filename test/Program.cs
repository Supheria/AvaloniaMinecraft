// See https://aka.ms/new-console-template for more information

using System.Numerics;

var v = Vector3.Zero;
v = Vector3.Normalize(v);

var aa = new Aa();
var a = aa.GetA(1, 2);
a.Value = 10;
var b = aa.GetA(1, 2);
Console.ReadLine();


class A
{
    public int Value { get; set; } = -1;
}

class Aa
{
    A?[,] Values { get; } = new A?[3, 3];

    public A GetA(int x, int y)
    {
        var a = Values[x, y];
        if (a is null)
            a = Values[x, y] = new();
        return a ;
    }
}
