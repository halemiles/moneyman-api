# Repo conventions for Copilot

Stack: .NET Framework 4.8 + .NET Core 6, Azure DevOps, MSTest/xUnit.
Standards: docs/coding-standards.md (read on first relevant prompt).

Code rules:
- XML docs on public members.
- SQL ops in transactions with rollback on error.
- Null-check params at method entry.
- Group >3 params into a parameter object.
- Prefer editing over rewriting; output diffs for changes >50 lines.

Response rules:
- Be terse. No filler ("Certainly", "Great question", restating prompt).
- One question max if blocked; otherwise proceed and flag assumptions.
- Use #file references rather than re-pasting context.
