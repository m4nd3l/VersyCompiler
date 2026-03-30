# Versy Programming Language
[![Version](https://img.shields.io/badge/version-0.0.1-blue.svg)](https://github.com/m4nd3l/VersyCompiler/releases)
[![Language](https://img.shields.io/badge/language-Versy-purple.svg)](#)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](https://github.com/m4nd3l/VersyCompiler/blob/main/LICENSE)


[**Download Latest**](https://github.com/m4nd3l/VersyCompiler/releases)   |   [**Report a Bug**](https://github.com/m4nd3l/VersyCompiler/issues)

| Project Info | |
| :--- | :---: |
| Author | [M4nd3l ](https://m4nd3l.github.io/) |
| Backend | Roslyn (.NET) |
| Extension | ```.vv``` |
| Status | Active development |
| Goal | C# Power, Easy to learn and use |

**Versy** is a streamlined, statically-typed programming language created by **M4nd3l**.  
It combines the architectural strength of C# and Java with a simplified, modern syntax designed for readability and rapid development.

---

## Why Versy?
- **Minimalist Syntax:** Reduced boilerplate compared to traditional C-style languages.
- **Strongly Typed:** Catch errors early with a clear type system.
- **Native Performance:** Compiled directly into optimized executables via the .NET Roslyn engine.
- **Open Source:** Built by the community, for the community.

## Syntax at a Glance
Save your code with the `.vv` extension. Here is a preview of what Versy looks like:

```rust
const pi: decimal = 3.14159;
var radius: int = 15;

var area: decimal = (radius ^ 2) * pi;
var circumference: decimal = 2 * radius * pi;

var isGreaterThan30: bool = circumference > 30;
var result: string;

if (isGreaterThan30) {
    result = "The circumference is greater than 30.";
} elif (circumference >= 15) {
    result = "The circumference is greater or equal to 15.";
} else {
    result = "The circumference isn't bigger than 15.";
}
```

---

## Installation

1. **Download:** Get the latest installer from the [[Releases](https://github.com/m4nd3l/VersyCompiler/releases)](https://github.com/m4nd3l/VersyCompiler/releases) page.
2. **Install:** Run the installer and follow the prompts. **Crucial:** Make sure to check the **"Add to PATH"** checkbox.
3. **Verify:** Open a new CMD or PowerShell window and run:
```bash
versy -v
```
   If successful, you will see the version info and author credits.

---

## Compiler Usage

To compile your code, open the terminal in your project folder and use the following command:

`versy <versy path> <output path> [arguments]`

### Available Arguments

| Argument | Description |
| :--- | :--- |
| `-h`, `--help` | Show all available commands and usage instructions. |
| `-v`, `--version` | Display the current compiler version. |
| `-t`, `--outputtk <path>` | **Debug:** Save the list of Lexer tokens to a file. |
| `-a`, `--outputast <path>` | **Debug:** Save the generated Abstract Syntax Tree (AST). |
| `-c`, `--csoutput <path>` | Export the generated C# code for debugging. |
| `-r`, `--run` | Automatically execute the program after a successful build. |
| `-s`, `--singlefile` | Produces one single file. |

---

## Technical Stack
The Versy Compiler is built entirely in **C#** and uses these high-performance libraries:
* **[Spectre.Console](https://spectreconsole.net/):** For an interactive and styled terminal experience.
* **[Dumpify](https://github.com/MoaidHathot/Dumpify):** For advanced visualization of internal data structures.

---

## Contributing
Versy is [open source](https://github.com/m4nd3l/VersyCompiler). Feel free to fork the repo, report issues, or submit pull requests to help improve the language!
