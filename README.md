# Veneer Project Development Outline
Veneer is a attempt to make a programming language that can easily use components from other languages; it's an attempt to remove the barriers to code created by the capabilities and challenges of certain languages. While the project isn't even in a prototype stage I have created a plan for the project.

This comprehensive roadmap details the architecture, compilation pipeline, and implementation strategy for **Veneer**—an easy-to-setup, customizable programming language built on a high-performance C# framework.

---

## Executive Summary: The Veneer Vision

Veneer provides a rigid, high-performance C-like base language (supporting loops, variables, and math) where extensibility is achieved not by modifying syntax, but by importing foreign code packets called **"teeth"**. These teeth are managed directly inside a native standard library file rather than external configuration scripts.

```
[Veneer Code File] ──┐
                     ├──> [ C# Compiler Front End ] ──> [ Tooth Registry ] ──> [ Back End Execution ]
[Standard Library] ──┘

```

---

## Phase 1: Compiler Front End (C#)

The front end treats both user code and the standard library exactly the same, using a static, highly optimized parsing pipeline.

* **Lexical Analyzer (Scanner):** * Hand-coded in C# for peak performance and descriptive error messages.
* Converts source characters into standard tokens (e.g., `FOR`, `WHILE`, `INT`, `IDENTIFIER`).
* Recognizes the specialized **`tooth`** keyword to safely mark foreign code boundaries.


* **Parser:**
* Utilizes a **Recursive-Descent parsing** strategy ideal for the static C-like grammar.
* Constructs a clean **Abstract Syntax Tree (AST)** representing the program's control flow.
* Enforces a strict grammar where users have no permission to modify base language syntax.


* **Semantic Analyzer:**
* Manages the global symbol table to resolve scope and variable assignments.
* Performs type checking against a predictable, reused type system layout (e.g., matching standard primitive sizes).



---

## Phase 2: The Standard Library & Bootstrapping

Instead of a separate text config file, the language uses its own code files to map and embed external features.

* **The Blueprint (`std.vn`):**
* Written entirely using Veneer syntax, acting as the mandatory root of the application loop.
* Explicitly registers foreign functions using clear identifier blocks:
```c
tooth fast_calculation(int x) language("rust") {
    // Foreign instructions or library mappings go here
}

```




* **The Bootstrapping Pipeline:**
1. The compiler initializes and implicitly loads `std.vn` before checking user code.
2. The front end parses `std.vn` and automatically builds the **Tooth Registry**.
3. The compiler proceeds to parse the user's primary source script, linking local calls back to the active Tooth Registry entries.



---

## Phase 3: The Interop Layer & Marshalling Engine

The core engine written in C# acts as a high-speed data translation layer between Veneer and foreign runtimes.

### Tooth Execution Matrix

| Tooth Mode | Handling Method | Mechanism | Target Ecosystem |
| --- | --- | --- | --- |
| **Pre-compiled Library** | Dynamic Loading | **P/Invoke (Platform Invoke)** binds to shared files (`.dll`/`.so`). | C, C++, Rust, Go |
| **Inline Interpreted** | Embedded Runtime | Managed wrapper scripts pass raw text into an active sub-interpreter. | Python (via PythonNET) |
| **Inline Compiled** | Side-Compilation | Background processes invoke local toolchains natively to output temporary libraries. | Rust (`rustc`), Go (`go build`) |

* **The Marshalling Engine:**
* Maintains the speed of the wrapper by enforcing **blittable types** (primitives that match memory layouts identically across boundaries).
* Copies flat memory blocks directly across runtimes to avoid slow serialization bottlenecks.



---

## Phase 4: Back-End Execution Strategy

To guarantee the compiler pipeline feels cohesive and lightweight, choose one primary target architecture for execution:

> **Primary Choice: .NET CIL Bytecode Transpilation**
> By utilizing C# to generate **Common Intermediate Language (CIL)** bytecode, Veneer programs run natively on the modern .NET runtime. This path simplifies compilation, leverages an incredibly fast Just-In-Time (JIT) optimizer, and makes loading pre-compiled or inline foreign teeth highly performant via native .NET interoperability systems.

---

## Phase 5: Implementation Milestones

* **Milestone 1: The Core Loop**
* Deliver a basic C# console app that scans, parses, and executes a plain loop or addition problem natively.


* **Milestone 2: The Static Gateway**
* Incorporate the `tooth` keyword syntax parser. Create a manual C# integration testing a hardcoded P/Invoke call to a standard C library function.


* **Milestone 3: Standard Library Interoperability**
* Build out the implicit file reader for `std.vn`. Enable the Tooth Registry to map multiple distinct functions dynamically from a file at runtime.


* **Milestone 4: The Inline Expansion**
* Incorporate embedded interpreter runtimes (like PythonNET) and basic automated background compilation tools to seamlessly manage raw code packets.
