# Conduit:example reporter (vanilla PB)

A minimal, **pure-vanilla** Space Engineers Programmable Block script showing how to write a packet the
[Conduit plugin](https://github.com/arsnekfcn/Conduit) will pick up. It's a **template**. Copy it and change
the tag and payload to whatever you want to export.

## What it does

Every few seconds it sums this construct's inventory by item subtype and writes one packet to the PB's own
Custom Data:

```
[CDT:conduit.example.v1]
{"grid":"Butler HQ","entityId":1234567,"inventory":[{"subtype":"Iron","amount":50000}]}
```

The **first line is the only contract** Conduit cares about: `[CDT:<tag>]`. Everything after the first newline
is your payload, verbatim (JSON here, but it can be anything). Conduit reads this Custom Data whenever you can
vanilla-access the grid — you're controlling it, on foot at it, or in antenna range, and forwards it to the
backend or local file you configured. See the plugin's [SCHEMA.md](https://github.com/arsnekfcn/Conduit) for
the envelope Conduit wraps around your packet.

## Install

1. Put a Programmable Block on a grid you want reported.
2. Paste the contents of `ExampleReporter.cs` into it and **Recompile**.
3. Done — no arguments, no config. The PB's Custom Data fills with the packet immediately (check the PB's
   detail panel / Custom Data to see it).

## Make it your own

- **`TAG`**: rename `conduit.example.v1` to whatever names *your* format (e.g. `mybase.power.v1`). A backend
  keys/dispatches on this tag, so a different writer just uses a different tag.
- **`BuildPacket()`**: replace the inventory dump with any data you can read off the grid (power, gas,
  production, block states, …). The payload is just text; JSON is only convention.

That's the whole idea: a script writes `[CDT:<tag>]\n<your data>`, and Conduit pipes it out. Anything more
elaborate (multi-grid aggregation, dashboards, quota tracking) is your own build on top.

## License

MIT. Copyright (c) arsnek (arsnekfcn).
