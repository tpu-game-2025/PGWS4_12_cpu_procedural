using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class Lichtenberg
{
    public enum MODE
    {
        ALL,
        FINISH_AT_FIRST_ARRLIVE,
    }

    public enum State
    {
        Running,
        FinishedAll,
        FinishedAtFirstArrive,
    }

    public int Width { get; private set; } = 96;
    public int Height { get; private set; } = 54;

    public MODE Mode { get; private set; } = MODE.ALL;


    int StartIndex = -1;
    public ushort[] Value { get; private set; } = null;
    public ushort ValueMax
    {
        get { return StartIndex < 0 ? (ushort)0 : Value[StartIndex]; }
    }
    public int ArriveIndex { get; private set; } = -1;

    public int[] Parent {  get; private set; } = null;

    struct Edge
    {
        public int Parent;
        public int Child;
    }
    List<Edge> edge = new();


    public Lichtenberg(int height, int width, MODE mode = MODE.ALL)
    {
        Height = height;
        Width = width;
        Mode = mode;

        int N = height * width;
        Parent = new int[N];
        Value = new ushort[N];

        ResetInternalState();
    }

    void ResetInternalState()
    {
        for (int i = 0; i < Width * Height; i++)
        {
            Parent[i] = -1;
            Value[i] = 0;
        }

        edge.Clear();
        ArriveIndex = -1;
        StartIndex = -1;
    }

    int AddEdge(int pos, int parentIndex)
    {
        Parent[pos] = parentIndex;

        int num = 0;
        int y = pos / Width;
        int x = pos % Width;

        int iu = pos - Width;
        int id = pos + Width;
        int il = pos - 1;
        int ir = pos + 1;
        if (0 < y && Parent[iu] == -1)
            { num++; edge.Add(new Edge { Parent = pos, Child = iu }); }
        if (y + 1 < Height && Parent[id] == -1) 
            { num++; edge.Add(new Edge { Parent = pos, Child = id }); }
        if (0 < x && Parent[il] == -1) 
            { num++; edge.Add(new Edge { Parent = pos, Child = il }); }
        if (x + 1 < Width && Parent[ir] == -1) 
            { num++; edge.Add(new Edge { Parent = pos, Child = ir }); }

        return num;
    }

    public void Initialize(int y, int x)
    {
        ResetInternalState();

        int idx = y * Width + x;
        StartIndex = idx;
        AddEdge(idx, idx);
    }

    private void searchRoot(int idx)
    {
        ushort v = 1;
        while(Parent[idx] != idx)
        {
            if (v <= Value[idx]) return;

            Value[idx] = v++;
            idx = Parent[idx];
        }

        Value[idx] = v < Value[idx] ? Value[idx] : v; //Mathf.Max(Value[idx], v);
    }

    public State Update()
    {
        int q;
        int pos;
        int pa;

        do
        {
            int c = edge.Count;
            if (c == 0)
            {
                return State.FinishedAll;
            }

            q = UnityEngine.Random.Range(0, c);
            pos = edge[q].Child;
            pa = edge[q].Parent;

            // edge.RemoveAt(q)
            edge[q] = edge[c - 1];
            edge.RemoveAt(c - 1);

        } while (Parent[pos] != -1);

        int num = AddEdge(pos, pa);

        if (num == 0)
        {
            searchRoot(pos);

            if(Mode == MODE.FINISH_AT_FIRST_ARRLIVE)
            {
                if(pos / Width == 0)
                {
                    ArriveIndex = pos;
                    edge.Clear();
                    return State.FinishedAtFirstArrive;
                }
            }
        }

        return State.Running;
    }
}
