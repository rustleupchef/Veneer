```text
██╗   ██╗███████╗███╗   ██╗███████╗███████╗██████╗ 
██║   ██║██╔════╝████╗  ██║██╔════╝██╔════╝██╔══██╗
██║   ██║█████╗  ██╔██╗ ██║█████╗  █████╗  ██████╔╝
╚██╗ ██╔╝██╔══╝  ██║╚██╗██║██╔══╝  ██╔══╝  ██╔══██╗
 ╚████╔╝ ███████╗██║ ╚████║███████╗███████╗██║  ██║
  ╚═══╝  ╚══════╝╚═╝  ╚═══╝╚══════╝╚══════╝╚═╝  ╚═╝
             ── A Large Scope Polygot Language ──
```

## About
Veneer is a language that was intended to make using code from other programming languages much more easily

As of now Veneer has support for: C, C++, Rust, Java, C#, Python, Javascript, Typescript, and Go.

<comment> Note that these languages are only partially supported in that there is no library support or complex data type support</comment>

## Usage
The Veneer Compiler sadly doesn't bundle the necessary compilers and software for all the programs so you need to install a few things

Requirements:
- An installation of python on a typical path install
- A graaval Java install (must be primary Java install at least on terminal session where veneer is ran)
- GCC and G++ (regardless of the operating system)
- GO compiler
- Rust compiler
- Dotnet

<comment> Note that the software will still run even if you don't have every single software noted here because each software is attached to a specific compiler for a specific language; however, you must have dotnet otherwise the compiler will not work </comment>

### Syntax
Before continuing with the unique syntax of the language, note that everything not listed here is the same as c#

#### Functions
```csharp
// func <[RETURN_TYPE]> [FUNCTION_NAME] ([ARGUMENTS])
func<void> foo (int bar) 
{
    // Whatever logic is needed to be here I suppose
}
```

<comment> while technically speaking the functions will handle you just writing raw csharp code in here that are only available under the System namespace I believe that it is in your best interest to run all csharp related code inside a tooth function</comment>

#### Teeth
```csharp
// tooth <[RETURN_TYPE]> [FUNCTION_NAME] ([ARGUMENTS]) language("[LANGUAGE_NAME]") => [OPENING_TAG]
// [FOREIGN_FUNCTION_BODY]
// [CLOSING_TAG]
tooth<void> foo (int bar) language("C") => 
{
    // whatever c logic you want to do
}
```

The tooth function acts very different compared to the base function of this language

Valid Language Strings (Not case sensitive):
- CPP, C++ 
- Csharp
- Java
- Python
- Javascript
- Typescript
- Go
- Rust

After the => token will be listed your opening tag; this tag is one that the user defines as the character that open and closes the foreign code block.

The only valid characters for the opening tag identifier are:
- Alpha
- Digits
- |, {, [, _

These are the only characters that will be recognized for the opening tage.

Generally your opening tag will be equivalent to your closing tag there are a few symbols that don't do such

| Opening Tag | Closing Tag  |
|-------------|--------------| 
| {           | }            |
| [           | ]            |

These characters will only swap to their mirrors if they make the entirety of the composition of the opening tag; otherwise, the opening tags and closing tags will simply be the same

Understand that these opening tags can not be written inside your foreign code block in any form or fashion, so attempt to pick a tag that you can guarantee will not show up in any substring of your foreign code block.

### Disclaimers
There are heavy limitations on the uses of foreign languages, and many more to note.

None of teeth (foreign language functions) have access to functions out of their scope with the sole exception of csharp which functions as a regular veneer function.

This programming language is designed to compile down to one file, but because of the JVE it was very difficult to make a clean implementation for Java; this resulted in the current architecture of the language desinging a C wrapper for the Java exported DLL, but this C wrapper is by default using a static reference to the Java DLL meaning that moving or deleting the Java DLL in anyware is destructive for the program, this makes Java a terrible use case for any code that will be used outside your computer (so just don't).

Javascript in the current moment is running using Jint; Jint doesn't have access to nearly any of the tools that are found in nodejs making javascript's implementation closer to obsolete than any of the other languages, and because the typescript handling uses the same library; typescript is also far behind.

This program needs access to your temp directory so if for some reason you don't have one or you don't give the program access to it then you will not be able to run the compiler.

### Running The Compiler
The way the compiler works is by parsing in two arguments for your build and source directory:

```bash
    -s, --source => "this is directory that contains all your .v (veneer) files"
    -b, --build => "this is directory where the executable and DLL files get compiled"
```

Now all you have to do is run the compiler in an terminal environment that has access to all your compilers, and the program will output a single executable in the build directory that you provided