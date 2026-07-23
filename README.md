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
    "imports" : ["[LEADING_STRINGS]"],
    "libraries" : ["[CUSTOM_STRINGS_FORMAT]"]
  }
}
```
you simply make a json object that has attributes titled after the language they are for (note that these must all be upper case, and you must match all your veneer tooth names to the ones listed in the file (eg. cpp in tooth files and "CPP" in config.json))
these attributes must be connected a json object that has attributes "imports" and libraries

#### Imports
The imports section will be handled in the same syntax of the language; that being, when your write the text for the imports it will be directly pasted in between the standard imports and the foreign function that you created.
You simply write every line of imports you desire in your code in a list of strings.

#### Libraries
This a list of strings that has unique structure for each individual language to be able to include libraries for the supported languages.

- Python: To install usable packages for python all that needs to be done is to create a VENV that contains all the packages that you want and use that VENV in your terminal session when running your veneer application.
- C#: To install usable package for C# you need to make the list of string for the libraries attribute contain a list of package reference blocks seen in that of an actual .csproj file
- Javascript: To install usable packages set up a package.json using commonjs or a module inside a folder of your choice. Inside the list of string for libraries write down all the folders that contains the files you want to be in the relative path of the node scripts and in this case that would be the folder containing your node_modules and package.json folder.
- C/C++: To install and libraries all you have to do is write the file paths of all the header files that you want to use in your project and they will be deposited relative to your c/c++ code where you can access them by doing #include "[NAME].h"

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