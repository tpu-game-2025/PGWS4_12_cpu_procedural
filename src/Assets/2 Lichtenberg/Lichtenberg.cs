using System.Collections.Generic;

public class Lichtenberg
{
    public enum MODE
    {
        ALL,                    // 全ての範囲を埋める
        FINISH_AT_FIRST_ARRIVE, // 最初に端に到達したら終了
    }

    public enum State
    {
        Running,                // 構築中
        FinishedAll,            // 全て埋まった
        FinishedAtFirstArrive,  // 最初に端に到達して終了
    }

    public int Width { get; private set; } = 96;
    public int Height { get; private set; } = 54;

    public MODE Mode { get; private set; } = MODE.ALL;

    // 出力値
    int StartIndex = -1;// 開始インデックス
    public ushort[] Value { get; private set; } = null;// 各セルの値
    public ushort ValueMax { get { return StartIndex < 0 ? (ushort)0 : Value[StartIndex]; } }
    public int ArriveIndex { get; private set; } = -1;// 最初に端に到達したインデックス

    // 内部管理用の親インデックス
    public int[] Parent { get; private set; } = null;

    // 拡張候補のエッジ
    struct Edge
    {
        public int Parent;
        public int Child;
    }
    List<Edge> edge = new List<Edge>();

    public Lichtenberg(int height, int width, MODE mode = MODE.ALL)
    {
        Height = height;
        Width = width;
        Mode = mode;

        int N = width * height;
        Parent = new int[N];
        Value = new ushort[N];

        ResetInternalState(); // 初期クリアもコンストラクタで1回行っておく
    }

    void ResetInternalState()
    {
        // Parent, Value をクリア
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
        if (    0 <      y && Parent[iu] == -1) { num++; edge.Add(new Edge { Parent = pos, Child = iu }); }
        if (y + 1 < Height && Parent[id] == -1) { num++; edge.Add(new Edge { Parent = pos, Child = id }); }
        if (    0 <      x && Parent[il] == -1) { num++; edge.Add(new Edge { Parent = pos, Child = il }); }
        if (x + 1 <  Width && Parent[ir] == -1) { num++; edge.Add(new Edge { Parent = pos, Child = ir }); }

        return num;
    }

    public void Initialize(int y, int x)
    {
        ResetInternalState();

        int idx = y * Width + x;
        StartIndex = idx;
        AddEdge(idx, idx);// 親が自分自身なのを終了条件とする
    }

    // 親をさかのぼって最大距離を埋める
    private void searchRoot(int idx)
    {
        ushort v = 1;
        while (Parent[idx] != idx)
        {
            // 既に大きな値が設定されていれば譲る
            if (v <= Value[idx]) return;

            Value[idx] = v++;
            idx = Parent[idx];
        }

        // ルートの処理
        Value[idx] = v < Value[idx] ? Value[idx] : v;
    }

    public State Update()
    {
        // 行先を選ぶ
        int q;// edgeのインデックス
        int pos;// 行先インデックス
        int pa;// 親インデックス

        do
        {
            // 行ける場所がないなら終了
            int c = edge.Count;
            if (c == 0)
            {
                return State.FinishedAll;
            }

            // 行先をランダムに選ぶ
            q = UnityEngine.Random.Range(0, c);
            pos = edge[q].Child;
            pa = edge[q].Parent;

            // 選ばれたエッジは削除
            edge[q] = edge[c - 1];
            edge.RemoveAt(c - 1);

        }while (Parent[pos] != -1); // 既に埋まっているなら繰り返し

        int num = AddEdge(pos, pa);// 新たなエッジを追加

        // 最初に端に到達したら終了
        if (Mode == MODE.FINISH_AT_FIRST_ARRIVE)
        {
            if (pos / Width == 0)
            {
                ArriveIndex = pos;
                searchRoot(pos);// 値の更新
                edge.Clear(); // 空にして強制終了
                return State.FinishedAtFirstArrive;
            }
        }

        //既に周囲は埋まっていた
        if (num == 0)
        {
            searchRoot(pos);// 値の更新
        }

        return State.Running;
    }
}
