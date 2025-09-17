# paste-md

> **Transform Markdown into beautifully formatted text with Ctrl+Shift+V**

A Windows system tray application that renders Markdown as rich text when pasting into Microsoft Office applications (Word, OneNote, Outlook). Features GitHub-style code blocks with gray backgrounds and monospace fonts.

## ✨ Features

- **🎯 Smart Detection**: Automatically detects Markdown in your clipboard (requires 2+ patterns)
- **📝 Universal Compatibility**: Works with Microsoft Office suite (Word, OneNote, Outlook, PowerPoint)
- **⚡ Instant Conversion**: No preview needed - paste renders immediately
- **💻 Code Block Support**: Renders code blocks with GitHub-style gray background and monospace font
- **🎨 Full Markdown Support**: Headers, bold, italic, lists, links, tables, blockquotes, and more
- **🔄 Dual Options**: "Paste as Rendered Markdown" or "Paste as Plain Markdown"
- **↩️ Undo Support**: Ctrl+Z always available to revert

## 📦 Installation

### Quick Start
```powershell
# Clone the repository
git clone https://github.com/PatrickRutledge/paste-md.git
cd paste-md

# Build the project
dotnet build --configuration Release

# Install (Run as Administrator)
.\INSTALL.bat
```

### Build from Source
```powershell
# Build the tray application
dotnet build src/paste-md.TrayApp/paste-md.TrayApp.csproj --configuration Release

# The executable will be in:
# src/paste-md.TrayApp/bin/Release/net48/paste-md.exe
```

## 🚀 Usage

### System Tray Application (Recommended)

1. **Run** paste-md from Start Menu or system tray
2. **Copy** any Markdown text from GitHub, VS Code, Notion, or anywhere else
3. **Press** `Ctrl+Shift+V` in Word, OneNote, or Outlook
4. **Enjoy** beautifully formatted text with code blocks in gray backgrounds!

**Tray Menu Options:**
- Right-click the tray icon for options
- "Test with Sample" - loads sample Markdown
- "Exit" - closes the application

**Note:** Regular `Ctrl+V` still pastes plain text. Only `Ctrl+Shift+V` renders the Markdown.

### Example Markdown Support

| Markdown | Rendered Result |
|----------|-----------------|
| `# Header` | **Large bold header** |
| `**bold**` | **bold text** |
| `*italic*` | *italic text* |
| `` `code` `` | `inline code with gray background` |
| ` ```code block``` ` | Gray box with monospace font |
| `- item` | • bulleted list |
| `[link](url)` | <u>blue underlined link</u> |

## 🛠️ Requirements

- Windows 10 or Windows 11 (x64)
- .NET 6.0 Runtime
- Microsoft Office applications for best experience

## 📂 Project Structure

```
paste-md/
├── src/
│   └── paste-md.Core/       # Main shell extension code
│       ├── Extensions/      # Context menu implementation
│       ├── Services/        # Markdown processing & clipboard
│       ├── Formatters/      # RTF/HTML conversion
│       └── Interfaces/      # COM interfaces
├── installer/               # WiX installer configuration
├── tools/                   # Build scripts
└── docs/                    # Documentation
```

## 🔧 Development

### Prerequisites
- Visual Studio 2022 (Community or higher)
- .NET 6.0 SDK
- WiX Toolset v4 (for installer)

### Building
```powershell
# Debug build with local registration
.\tools\build.ps1 -Configuration Debug -RegisterLocal

# Release build with installer
.\tools\build.ps1 -Configuration Release -CreateInstaller -SignBinaries
```

### Testing
1. Copy Markdown text: `**Hello World!**`
2. Right-click in Word or OneNote
3. Select "Paste as Rendered Markdown"
4. Should paste as: **Hello World!**

## 🐛 Troubleshooting

### Hotkey not working?
- Ensure the tray icon is visible in system tray
- Try using the tray menu instead: Right-click → "Paste Rendered Markdown"
- Check that you copied Markdown text (needs 2+ patterns like headers, bold, lists)

### Not formatting correctly?
- Regular `Ctrl+V` pastes plain text (this is normal)
- Use `Ctrl+Shift+V` for rendered Markdown
- PowerPoint is not supported (use screenshots instead)

## 📝 License

MIT License - See [LICENSE](LICENSE) file for details

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 🙏 Acknowledgments

- [Markdig](https://github.com/lunet-io/markdig) - Excellent Markdown processor for .NET
- [WiX Toolset](https://wixtoolset.org/) - Windows installer framework

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/PatrickRutledge/paste-md/issues)

---

**Made with ❤️ for the Markdown community**