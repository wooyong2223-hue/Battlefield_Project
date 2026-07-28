# Battlefield Project Rules

## Change reporting

After modifying project files, always report the following to the user:

1. Every script or asset file changed.
2. The specific code or serialized value that changed.
3. How the behavior changed, including concise before-and-after snippets when useful.
4. Why the change was necessary.
5. What validation was performed and what still requires Unity Editor confirmation.

Do not hide implementation details behind a summary. Keep the report focused on the relevant changed sections rather than dumping entire files.

## Pre-change comparison

Before modifying project files, compare the current file and asset state with the state from the previous task.

1. Identify changes made by the user or Unity Editor since the previous task.
2. Tell the user which relevant files, references, serialized values, hierarchy positions, or settings changed before starting the modification.
3. Explain any overlap or conflict between those changes and the planned work.
4. Preserve the newer user changes and adjust the implementation around them.

Do not begin modifying overlapping files until the relevant differences have been reported to the user.
