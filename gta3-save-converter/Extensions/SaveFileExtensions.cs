using System;
using GTASaveData;
using GTASaveData.GTA3;
using GTASaveData.VC;

namespace SaveConverter.Extensions
{
    public static class SaveFileExtensions
    {
        public static string GetSaveName(this SaveFileGTA3VC save)
        {
            if (save is SaveFileGTA3 gta3)
            {
                return gta3.GetSaveName();
            }
            if (save is SaveFileVC vc)
            {
                return vc.GetSaveName();
            }

            throw new InvalidOperationException("Unknown save type!");
        }

        public static string GetSaveName(this SaveFileGTA3 save)
        {
            string name = save.SimpleVars.LastMissionPassedName;
            if (string.IsNullOrEmpty(name))
            {
                name = "(empty)";
            }
            else if (name[0] == '\xFFFF')
            {
                App.GxtGTA3.TryGetValue(name.Substring(1), out string gxtName);
                name = gxtName ?? "(invalid GXT key)";
            }

            return name;
        }

        public static string GetSaveName(this SaveFileVC save)
        {
            App.GxtVC["MAIN"].TryGetValue(save.SimpleVars.LastMissionPassedName, out string gxtName);
            return gxtName ?? "(invalid GXT key)";
        }

        public static float GetProgress(this SaveFileGTA3VC save)
        {
            if (save is SaveFileGTA3 gta3)
            {
                return (float) gta3.Stats.ProgressMade / gta3.Stats.TotalProgressInGame;
            }
            if (save is SaveFileVC vc)
            {
                return vc.Stats.ProgressMade / vc.Stats.TotalProgressInGame;
            }

            throw new InvalidOperationException("Unknown save type!");
        }
    }
}
