// See https://aka.ms/new-console-template for more information

using System.Numerics;

public static class Program
{
    static Dictionary<int, Dictionary<int, string>> Dics = [];

    public static void Main()
    {
        var v = Vector3.Zero;
        v = Vector3.Normalize(v);

        var aa = new Aa();
        var a = aa.GetA(1, 2);
        a.Value = 10;
        var b = aa.GetA(1, 2);

        var ss = new List<S>() { new(), new() };
        ss[0] = ss[0].SetValue(10);
        var s0 = ss[0];
        ss[0].SetFx(100);
        
        var dic = GetDic(5);
        dic[0] = "hello";
        
        dic = GetDic(5);
        dic[5] = "world";
        
        var ints = new int[50];

        Console.ReadLine();
    }

    private static Dictionary<int, string> GetDic(int index)
    {
        if (Dics.TryGetValue(index, out var dic))
            return dic;
        Dics[index] = [];
        return Dics[index];
    }
}

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
        return a;
    }
}

struct S
{
    public int Value { get; private set; }

    public S()
    {
        Value = 7;
    }

    public S SetValue(int value)
    {
        Value = value;
        return this;
    }

    public void SetFx(int value)
    {
        Value = value;
    }
}
