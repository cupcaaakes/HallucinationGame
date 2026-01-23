
using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class GAIAStats : MonoBehaviour
{
    public static GAIAStats I { get; private set; }

    public GAIAData data;

    string Folder => Application.persistentDataPath;
    string TodayDate => DateTime.Now.ToString("yyyy-MM-dd");
    string CurrentFilePath;

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        LoadOrCreateDailyFile();
    }

    void LoadOrCreateDailyFile()
    {
        Directory.CreateDirectory(Folder);

        var latest = Directory.GetFiles(Folder, "*_GAIAData.json")
            .Select(f => new { path = f, num = ParseLeadingNumber(Path.GetFileName(f)) })
            .Where(x => x.num >= 0)
            .OrderByDescending(x => x.num)
            .FirstOrDefault();

        // First ever file
        if (latest == null)
        {
            data = new GAIAData { date = TodayDate };
            CurrentFilePath = Path.Combine(Folder, "0001_GAIAData.json");
            Save();
            return;
        }

        // Load latest file
        var loaded = Load(latest.path);

        // Same day -> keep using it
        if (loaded != null && loaded.date == TodayDate)
        {
            data = loaded;
            CurrentFilePath = latest.path;
            return;
        }

        // New day -> create next file, carry totals, reset TODAY
        int nextNum = latest.num + 1;
        CurrentFilePath = Path.Combine(Folder, nextNum.ToString("0000") + "_GAIAData.json");

        data = loaded ?? new GAIAData();

        data.date = TodayDate;
        data.todayRuns = 0;

        data.thinkerToday = 0;
        data.techEnthusiastToday = 0;
        data.revolutionaryToday = 0;
        data.bridgeBuilderToday = 0;
        data.doubterToday = 0;
        data.opinionShaperToday = 0;
        data.individualistToday = 0;
        data.humanistToday = 0;

        Save();
    }

    int ParseLeadingNumber(string filename)
    {
        int underscore = filename.IndexOf('_');
        if (underscore <= 0) return -1;
        string prefix = filename.Substring(0, underscore);
        return int.TryParse(prefix, out int n) ? n : -1;
    }

    GAIAData Load(string path)
    {
        try
        {
            return JsonUtility.FromJson<GAIAData>(File.ReadAllText(path));
        }
        catch
        {
            return null;
        }
    }

    void Save()
    {
        try
        {
            File.WriteAllText(CurrentFilePath, JsonUtility.ToJson(data, true));
        }
        catch { }
    }

    // Call this when a rank is awarded
    public void RecordRank(RankId rank)
    {
        data.totalRuns++;
        data.todayRuns++;

        switch (rank)
        {
            case RankId.Thinker: data.thinkerTotal++; data.thinkerToday++; break;
            case RankId.TechEnthusiast: data.techEnthusiastTotal++; data.techEnthusiastToday++; break;
            case RankId.Revolutionary: data.revolutionaryTotal++; data.revolutionaryToday++; break;
            case RankId.BridgeBuilder: data.bridgeBuilderTotal++; data.bridgeBuilderToday++; break;
            case RankId.Doubter: data.doubterTotal++; data.doubterToday++; break;
            case RankId.OpinionShaper: data.opinionShaperTotal++; data.opinionShaperToday++; break;
            case RankId.Individualist: data.individualistTotal++; data.individualistToday++; break;
            case RankId.Humanist: data.humanistTotal++; data.humanistToday++; break;
        }

        Save();
    }

    public int GetTotal(RankId r) => r switch
    {
        RankId.Thinker => data.thinkerTotal,
        RankId.TechEnthusiast => data.techEnthusiastTotal,
        RankId.Revolutionary => data.revolutionaryTotal,
        RankId.BridgeBuilder => data.bridgeBuilderTotal,
        RankId.Doubter => data.doubterTotal,
        RankId.OpinionShaper => data.opinionShaperTotal,
        RankId.Individualist => data.individualistTotal,
        RankId.Humanist => data.humanistTotal,
        _ => 0
    };

    public int GetToday(RankId r) => r switch
    {
        RankId.Thinker => data.thinkerToday,
        RankId.TechEnthusiast => data.techEnthusiastToday,
        RankId.Revolutionary => data.revolutionaryToday,
        RankId.BridgeBuilder => data.bridgeBuilderToday,
        RankId.Doubter => data.doubterToday,
        RankId.OpinionShaper => data.opinionShaperToday,
        RankId.Individualist => data.individualistToday,
        RankId.Humanist => data.humanistToday,
        _ => 0
    };

    public float GetPercentOverall(RankId r)
    {
        if (data.totalRuns <= 0) return 0f;
        return (GetTotal(r) / (float)data.totalRuns) * 100f;
    }

    public float GetPercentToday(RankId r)
    {
        if (data.todayRuns <= 0) return 0f;
        return (GetToday(r) / (float)data.todayRuns) * 100f;
    }
}
