# Battlefield Project Rules

## Change reporting

After modifying project files, always report the following to the user:

1. Every script or asset file changed.
2. The specific code or serialized value that changed.
3. How the behavior changed, including concise before-and-after snippets when useful.
4. Why the change was necessary.
5. What validation was performed and what still requires Unity Editor confirmation.

Do not hide implementation details behind a summary. Keep the report focused on the relevant changed sections rather than dumping entire files.

## Code removal and replacement

Before deleting existing code, serialized fields, components, references, or assets, or replacing them with a different implementation:

1. Tell the user exactly what existing implementation will be removed or replaced before making the change.
2. Show the relevant previous code or serialized values and explain the behavior they currently provide.
3. Explain what will replace them and how the behavior will change.
4. Do not silently remove an earlier implementation merely because a newer request supersedes it.

After the change, explicitly report every removed or replaced method, field, component, reference, or asset and why it was no longer needed.

## Pre-change comparison

Before modifying project files, compare the current file and asset state with the state from the previous task.

1. Identify changes made by the user or Unity Editor since the previous task.
2. Tell the user which relevant files, references, serialized values, hierarchy positions, or settings changed before starting the modification.
3. Explain any overlap or conflict between those changes and the planned work.
4. Preserve the newer user changes and adjust the implementation around them.

Do not begin modifying overlapping files until the relevant differences have been reported to the user.

## User-defined values and structure

Treat every value, count, name, hierarchy structure, and setting explicitly provided or changed by the user as the current source of truth.

1. Do not change, extend, reduce, rename, or reinterpret user-defined values or structures based on assumptions about future work.
2. Do not replace the user's current choice with a previously discussed value or an inferred recommendation.
3. If a different value or structure appears necessary, explain the exact proposed difference and reason before making any change, then wait for the user's approval.
4. Base all follow-up instructions and implementations on the user's latest visible state.

## Combat feature testing

Implement and verify new hit, damage, projectile, and firing behavior with the dummy test range before integrating it into vehicles or other equipment.

1. Use the allied dummy shooter as the projectile owner.
2. Verify enemy, neutral, and allied collision behavior with the test dummies.
3. Integrate the verified behavior into equipment only after the dummy test passes.

## Serialized field attributes

Do not use `[SerializeField, Range(...)]`. Use `[SerializeField]` without the `Range` attribute.

## New feature object design

Before implementing a new feature, consider whether it should be separated into its own class, component, or data object.

1. Review the feature's responsibility, lifecycle, state, dependencies, reuse potential, and independent testability before choosing its structure.
2. Prefer a separate object when the feature has a distinct responsibility or lifecycle, owns meaningful state, may be reused, or can be tested independently.
3. Keep small behavior inside an existing object when extraction would add indirection without creating a clear responsibility boundary.
4. Tell the user which structure was chosen and why before implementation.
5. Avoid expanding an existing manager or controller with unrelated responsibilities merely because it is already available.

## Object-oriented design principles

Apply the four core object-oriented principles to new and modified code:

1. Abstraction: expose only the behavior required by callers and hide irrelevant implementation details behind clear contracts.
2. Encapsulation: keep state ownership inside the responsible object, restrict direct mutation, and provide intention-revealing methods or properties.
3. Inheritance: use inheritance only for a genuine substitutable "is-a" relationship; prefer composition when behavior merely needs to be reused or combined.
4. Polymorphism: use interfaces, abstract contracts, or overridable behavior when multiple implementations must be interchangeable without caller-side type checks.

Do not introduce inheritance, interfaces, or abstraction layers only to claim adherence. Each abstraction must create a clear responsibility boundary or a real variation point.

## SOLID principles

New and modified code must follow the five SOLID principles:

1. Single Responsibility Principle: each class or component should have one clear reason to change and should not combine unrelated gameplay, input, presentation, persistence, or infrastructure responsibilities.
2. Open/Closed Principle: prefer extension through composition, interfaces, strategies, events, or new implementations instead of repeatedly modifying stable code for every variant.
3. Liskov Substitution Principle: derived classes and interface implementations must preserve the contract, valid inputs, expected outputs, and side effects of the type they replace.
4. Interface Segregation Principle: keep interfaces focused so consumers depend only on operations they actually use; split broad interfaces when clients require unrelated subsets.
5. Dependency Inversion Principle: high-level gameplay policy should depend on stable abstractions rather than concrete low-level implementations; provide dependencies explicitly through serialized references, constructors for plain C# objects, or initialization methods as appropriate for Unity.

Before completing a feature, review the changed design against these principles and report any intentional exception and its reason to the user.
