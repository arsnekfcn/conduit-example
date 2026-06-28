// ─────────────────────────────────────────────────────────────────────────────
// Conduit example reporter: A minimal vanilla Programmable Block script.
//
// A TEMPLATE for writing a packet the Conduit plugin will pick up. It does one thing:
// every few seconds it sums this construct's inventory and writes a [CDT:<tag>] packet
// to its OWN Custom Data. Conduit reads that Custom Data when you can vanilla-access this
// grid and forwards it to the backend (or local file) you configured.
//
// The only contract Conduit cares about is the FIRST LINE:   [CDT:<tag>]
// Everything after the first newline is your payload, verbatim. JSON here, but it can be
// anything. Pick your own <tag> and payload; this inventory dump is just a working example.
//
// No config, no dependencies. Drop it on a Programmable Block and Recompile.
// ─────────────────────────────────────────────────────────────────────────────

// The marker Conduit scans for is "[CDT:" + this tag. Change it to whatever names YOUR
// format, e.g. "mybase.power.v1". A backend keys/dispatches on this tag.
const string TAG = "conduit.example.v1";

const double REFRESH_SEC = 3.0;   // how often to rebuild the packet (the inventory scan is the only real cost)

readonly List<IMyTerminalBlock> _blocks = new List<IMyTerminalBlock>();
readonly List<MyInventoryItem> _items = new List<MyInventoryItem>();
double _since = REFRESH_SEC;      // force a build on the first run

Program()
{
    Runtime.UpdateFrequency = UpdateFrequency.Update100;   // ~1.6s per tick
}

void Main(string argument, UpdateType updateSource)
{
    _since += Runtime.TimeSinceLastRun.TotalSeconds;
    if (_since < REFRESH_SEC) return;
    _since = 0;

    Me.CustomData = BuildPacket();
    Echo("Conduit example: wrote [CDT:" + TAG + "]\n" + _blocks.Count + " inventories scanned");
}

// Build "[CDT:<tag>]\n<json>". The JSON is a small example, the grid's name/id and its
// inventory totaled by item subtype. Replace the payload with whatever you want to export.
string BuildPacket()
{
    var grid = Me.CubeGrid;

    // Sum every item across this construct's inventories, keyed by subtype.
    var totals = new Dictionary<string, double>();
    GridTerminalSystem.GetBlocksOfType(_blocks, b => b.IsSameConstructAs(Me) && b.HasInventory);
    foreach (var b in _blocks)
        for (int i = 0; i < b.InventoryCount; i++)
        {
            _items.Clear();
            b.GetInventory(i).GetItems(_items);
            foreach (var it in _items)
            {
                double amt;
                totals.TryGetValue(it.Type.SubtypeId, out amt);
                totals[it.Type.SubtypeId] = amt + (double)it.Amount;
            }
        }

    // Hand-roll the JSON (a PB has no JSON library). FIRST LINE is the Conduit marker.
    var sb = new StringBuilder();
    sb.Append("[CDT:").Append(TAG).Append("]\n");
    sb.Append("{\"grid\":").Append(Json(grid.CustomName));
    sb.Append(",\"entityId\":").Append(grid.EntityId);
    sb.Append(",\"inventory\":[");
    bool first = true;
    foreach (var kv in totals)
    {
        if (!first) sb.Append(',');
        first = false;
        sb.Append("{\"subtype\":").Append(Json(kv.Key))
          .Append(",\"amount\":").Append(Num(kv.Value)).Append('}');
    }
    sb.Append("]}");
    return sb.ToString();
}

// Minimal JSON string escaping (quotes, backslashes, control chars).
static string Json(string s)
{
    if (s == null) return "\"\"";
    var sb = new StringBuilder("\"");
    foreach (char c in s)
    {
        if (c == '"' || c == '\\') sb.Append('\\');
        sb.Append(c < ' ' ? ' ' : c);
    }
    return sb.Append('"').ToString();
}

static string Num(double v) => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
