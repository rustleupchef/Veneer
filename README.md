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
- A GraalVM Java install (must be the primary Java installation at least on terminal session where veneer is ran)
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

Valid Language Strings (Not case-sensitive):
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
- |, {, [, (, _

These are the only characters that will be recognized for the opening tage.

Generally your opening tag will be equivalent to your closing tag there are a few symbols that don't do such

| Opening Tag | Closing Tag |
|-------------|-------------| 
| {           | }           |
| [           | ]           |
| (           | )           |

These characters will only swap to their mirrors if they make the entirety of the composition of the opening tag; otherwise, the opening tags and closing tags will simply be the same

Understand that these opening tags can not be written inside your foreign code block in any form or fashion, so attempt to pick a tag that you can guarantee will not show up in any substring of your foreign code block.

### Config File
the config file is handled like a simple json structure with a very basic structure
```json
{
  "[LANGUAGE_NAME]" : {
    "imports" : "[LEADING_STRING]",
    "libraries" : "[CUSTOM_STRING_FORMAT]"
  }
}
```

you simply make a json object that has attributes titled after the language they are for (note that these must all be upper case, and you must match all your veneer tooth names to the ones listed in the file (eg. cpp in tooth files and "CPP" in config.json))
these attributes must be connected a json object that has attributes "imports" and libraries
- imports: this is the string that will be typically appended at the top of files and are imports (eg. "#include <math.h>", etc)
- libraries: this is a string that will be parsed and formatted uniquely based on the language  (it hasn't been properly implemented yet)

### Disclaimers
There are heavy limitations on the uses of foreign languages and many more that I likely accidentally missed.

Because this language makes heavy use of P invoke and DLL Import it does not have to capabilities to handle complex data types quite yet and those will need to be handled via byte arrays and serialized json otherwise will be impossible to share.

None of teeth (foreign language functions) have access to functions out of their scope with the sole exception of csharp which functions as a regular veneer function.

This program needs access to your temp directory so if for some reason you don't have one, or you don't give the program access to it then you will not be able to run the compiler.

Java is being run by wrapping the GraalVM generated DLL files in a C wrapper (to attempt to avoid collisions), but now runs into a separate issue of having its input types being put to the same limitations as C

### Running The Compiler

You run the compiler with these command line arguments.
```bash
    # MANDATORY ARGUMENTS
    -s, --source => "this is directory that contains all your .v (veneer) files"
    -b, --build => "this is directory where the executable and DLL files get compiled"
    
    # OPTIONAL COMMANDS
    -n, --name => "the output name of the executable; default: main"
    -c, --config => "the is the file directory that contains the config for compiler"
```

Now all you have to do is run the compiler in a terminal environment that has access to all your compilers, and the program will output a single executable in the build directory that you provided