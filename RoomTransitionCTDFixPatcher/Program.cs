using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;

namespace RoomTransitionCTDFixPatcher
{
    public class Program
    {
        private const string ScriptName = "OStimCellTransitionCrashFix";

        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "RoomTransitionCTDFix_Doors.esp")
                .Run(args);
        }

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            string errorLogPath = Path.Combine(Path.GetDirectoryName(state.OutputPath)!, "patcher_errors.log");
            int checkedCount = 0, matchCount = 0, appliedCount = 0, errorCount = 0;

            foreach (var context in state.LoadOrder.PriorityOrder
                .WinningContextOverrides<ISkyrimMod, ISkyrimModGetter, IPlacedObject, IPlacedObjectGetter>(state.LinkCache))
            {
                checkedCount++;

                try
                {
                    var placedObjectGetter = context.Record;

                    if (placedObjectGetter.TeleportDestination is null) continue;
                    if (!placedObjectGetter.Base.TryResolve(state.LinkCache, out var baseRecord)) continue;
                    if (baseRecord is not IDoorGetter) continue;

                    matchCount++;

                    var placedObject = context.GetOrAddAsOverride(state.PatchMod);

                    placedObject.VirtualMachineAdapter ??= new VirtualMachineAdapter();

                    bool alreadyHasScript = placedObject.VirtualMachineAdapter.Scripts
                        .Any(s => s.Name == ScriptName);

                    if (!alreadyHasScript)
                    {
                        placedObject.VirtualMachineAdapter.Scripts.Add(new ScriptEntry
                        {
                            Name = ScriptName,
                            Flags = ScriptEntry.Flag.Local,
                        });
                        appliedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errorCount++;
                    File.AppendAllText(errorLogPath,
                        $"[{context.Record.FormKey}] {ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{Environment.NewLine}");
                }
            }

            Console.WriteLine($"Referencias comprobadas: {checkedCount}");
            Console.WriteLine($"Puertas de teletransporte detectadas: {matchCount}");
            Console.WriteLine($"Script anadido a: {appliedCount}");
            if (errorCount > 0)
            {
                Console.WriteLine($"Registros con error (omitidos): {errorCount} - detalle en {errorLogPath}");
            }

            int nameTouched = 0;
            foreach (var cell in state.PatchMod.EnumerateMajorRecords<Cell>())
            {
                if (cell.Name?.String is string s)
                {
                    cell.Name = new Mutagen.Bethesda.Strings.TranslatedString(Mutagen.Bethesda.Strings.Language.English, s);
                    nameTouched++;
                }
            }
            foreach (var worldspace in state.PatchMod.EnumerateMajorRecords<Worldspace>())
            {
                if (worldspace.Name?.String is string s)
                {
                    worldspace.Name = new Mutagen.Bethesda.Strings.TranslatedString(Mutagen.Bethesda.Strings.Language.English, s);
                    nameTouched++;
                }
            }
            Console.WriteLine($"Nombres de celda/worldspace re-escritos (mismo valor, forzando el registro): {nameTouched}");
        }
    }
}
